using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace VPet.Plugin.VPetTTS
{
    /// <summary>
    /// 朗读文本过滤器：把括号包裹的动作描写从「要念出来的文本」里剔除。
    ///
    /// 气泡显示的是宿主原文，这里只加工送进 TTS 的副本，
    /// 所以效果是「动作描写看得到、听不到」。
    ///
    /// 过滤是幂等的（过滤后的文本再过一次不会变化），
    /// 因此允许在多个下游入口各自调用而不会互相破坏缓存键。
    /// </summary>
    public static class SpeechTextFilter
    {
        /// <summary>
        /// 一组成对的括号。开闭字符各自可以有多个变体，半角全角混着写也能配上。
        /// </summary>
        private readonly struct BracketGroup
        {
            public BracketGroup(string open, string close)
            {
                Open = open;
                Close = close;
            }

            public string Open { get; }
            public string Close { get; }
        }

        /// <summary>单条正则的匹配上限，防止写出灾难性回溯把说话卡死</summary>
        private static readonly TimeSpan PatternTimeout = TimeSpan.FromMilliseconds(200);

        private static readonly BracketGroup RoundGroup = new BracketGroup("(（﹙", ")）﹚");
        private static readonly BracketGroup SquareGroup = new BracketGroup("[【〔［", "]】〕］");
        private static readonly BracketGroup CurlyGroup = new BracketGroup("{｛", "}｝");

        /// <summary>纯尖括号。书名号单独成组，见 <see cref="BookTitleGroup"/></summary>
        private static readonly BracketGroup AngleGroup = new BracketGroup("<＜", ">＞");

        /// <summary>
        /// 书名号 《 》 〈 〉。中文里 〈 〉 是篇名号（书名号的一种），
        /// 跟数学上的尖括号不是一回事，所以归到这一组而不是 <see cref="AngleGroup"/>。
        /// </summary>
        private static readonly BracketGroup BookTitleGroup = new BracketGroup("《〈", "》〉");

        //标签名：首字符不能是数字，后面允许字母/数字/下划线/汉字和 . : -
        //（.NET 的 \w 默认含汉字，所以中文标签名不用另外写范围；[^\W\d] 就是「是词字符但不是数字」）
        private const string TagName = @"[^\W\d][\w.:-]*";

        /// <summary>
        /// 成对标签 <动作>轻轻摸了摸主人的头</动作>：连标签带里面的内容一起删掉。
        ///
        /// 用反向引用要求首尾同名，所以「5 < x and y > 3」这种文本不会被误当成标签。
        /// 惰性匹配到第一个同名闭合标签，因此 <动作>…<语气>…</语气>…</动作> 这类异名嵌套是对的。
        /// </summary>
        private static readonly Regex PairedTagPattern = new Regex(
            @"<(?<t>" + TagName + @")(?:\s[^<>]*)?>[\s\S]*?</\k<t>\s*>",
            RegexOptions.Compiled, PatternTimeout);

        /// <summary>自闭合标签 <动作/>。要求以 /> 收尾，不会误伤普通的大于小于号</summary>
        private static readonly Regex SelfClosingTagPattern = new Regex(
            @"<" + TagName + @"(?:\s[^<>]*?)?/>",
            RegexOptions.Compiled, PatternTimeout);

        /// <summary>
        /// 落单的标签壳子（只有开标签或只有闭标签）：只去掉标记本身，里面的话照念。
        /// 不允许带属性，免得把「a <b and c> d」这种句子也啃掉
        /// </summary>
        private static readonly Regex OrphanTagPattern = new Regex(
            @"</?" + TagName + @"\s*/?>",
            RegexOptions.Compiled, PatternTimeout);

        /// <summary>
        /// 成对星号包裹的动作描写：*摸摸头* / **摸摸头**。
        ///
        /// 沿用 Markdown 强调的判定：星号内侧不能紧挨空白，
        /// 这样「3 * 4 * 5」这类乘法算式就不会被当成动作描写吃掉。
        /// </summary>
        private static readonly Regex AsteriskPattern =
            new Regex(@"\*\*(?=\S)[^*]+(?<=\S)\*\*|\*(?=\S)[^*]+(?<=\S)\*", RegexOptions.Compiled);

        /// <summary>行内连续空白</summary>
        private static readonly Regex InlineWhitespacePattern =
            new Regex("[ \t　]+", RegexOptions.Compiled);

        /// <summary>剔除括号后留在句首的孤立标点</summary>
        private static readonly Regex LeadingPunctuationPattern =
            new Regex(@"^[\s,，、。．.;；:：!！?？~～…—\-]+", RegexOptions.Compiled);

        /// <summary>剔除括号后挤在一起的重复标点，保留最后一个</summary>
        private static readonly Regex DuplicatePunctuationPattern =
            new Regex(@"[,，、。．.;；:：!！?？]\s*(?=[,，、。．.;；:：!！?？])", RegexOptions.Compiled);

        /// <summary>
        /// 按设置过滤待朗读文本。
        /// </summary>
        /// <returns>
        /// 过滤后的文本；整句都是动作描写时返回空字符串，调用方据此跳过本次 TTS。
        /// 过滤未启用时原样返回。
        /// </returns>
        public static string Apply(string text, TextFilterSetting setting)
            => Apply(text, setting, null);

        /// <summary>
        /// 同上，另外收集自定义正则的报错，供设置界面回显。
        /// </summary>
        /// <param name="errors">写错的正则会往这里追加一条说明；传 null 表示不关心</param>
        public static string Apply(string text, TextFilterSetting setting, List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;
            if (setting is null || !setting.Enable)
                return text;

            var groups = BuildGroups(setting);
            var hasCustomRegex = !string.IsNullOrWhiteSpace(setting.CustomRegex);
            if (groups.Count == 0 && !setting.PairedTag && !setting.Asterisk && !hasCustomRegex)
                return text;

            var result = text;
            //标签必须排在括号规则前面：否则开了「尖括号」时，<动作> 会被当成一对尖括号删掉，
            //把里面的「轻轻摸了摸主人的头」留下来念出来，正好跟本意相反
            if (setting.PairedTag)
                result = RemoveTags(result);
            if (groups.Count > 0)
                result = RemoveBracketedSpans(result, groups);
            if (setting.Asterisk)
                result = AsteriskPattern.Replace(result, "");
            //自定义正则放在最后：括号规则先把常见情况处理掉，正则只补剩下的花样
            if (hasCustomRegex)
                result = RemoveCustomRegexMatches(result, setting.CustomRegex, errors);

            result = Normalize(result);

            // 只剩标点和空白，说明整句都是动作描写，直接不朗读
            return HasSpeakableContent(result) ? result : string.Empty;
        }

        /// <summary>
        /// 过滤是否真的改变了文本，仅用于日志，避免刷屏
        /// </summary>
        public static bool Changed(string original, string filtered)
            => !string.Equals(original, filtered, StringComparison.Ordinal);

        private static List<BracketGroup> BuildGroups(TextFilterSetting setting)
        {
            var groups = new List<BracketGroup>(6);
            if (setting.RoundBracket) groups.Add(RoundGroup);
            if (setting.SquareBracket) groups.Add(SquareGroup);
            if (setting.CurlyBracket) groups.Add(CurlyGroup);
            if (setting.AngleBracket) groups.Add(AngleGroup);
            if (setting.BookTitleMark) groups.Add(BookTitleGroup);
            groups.AddRange(ParseCustomPairs(setting.CustomPairs));
            return groups;
        }

        /// <summary>
        /// 解析自定义括号对。写法是「开闭两个字符为一组」，组间可用空格或逗号分隔。
        /// </summary>
        private static List<BracketGroup> ParseCustomPairs(string custom)
        {
            var groups = new List<BracketGroup>();
            if (string.IsNullOrWhiteSpace(custom))
                return groups;

            var chars = new List<char>(custom.Length);
            foreach (var c in custom)
            {
                if (char.IsWhiteSpace(c) || c == ',' || c == '，' || c == '、' || c == ';' || c == '；')
                    continue;
                chars.Add(c);
            }

            for (var i = 0; i + 1 < chars.Count; i += 2)
            {
                var open = chars[i];
                var close = chars[i + 1];
                if (open == close)
                    continue; // 同一个字符不成对，这类对称标记交给星号规则处理
                groups.Add(new BracketGroup(open.ToString(), close.ToString()));
            }

            return groups;
        }

        /// <summary>
        /// 删掉 <动作>轻轻摸了摸主人的头</动作> 这类标签，连内容一起。
        ///
        /// 三步走：先成对的（连内容删），再自闭合的，最后清掉落单的标签壳子 ——
        /// 壳子只删标记本身，里面的话该念还是要念，因为落单时根本判断不出范围。
        /// </summary>
        private static string RemoveTags(string text)
        {
            if (text.IndexOf('<') < 0)
                return text;

            try
            {
                text = PairedTagPattern.Replace(text, "");
                text = SelfClosingTagPattern.Replace(text, "");
                text = OrphanTagPattern.Replace(text, "");
            }
            catch (RegexMatchTimeoutException)
            {
                //极端输入下不硬扛，宁可这句不过滤也不能把说话卡住
            }

            return text;
        }

        /// <summary>
        /// 逐行拆出自定义正则。空行忽略，以 # 开头的行当注释。
        /// </summary>
        public static IEnumerable<string> EnumeratePatterns(string patterns)
        {
            if (string.IsNullOrWhiteSpace(patterns))
                yield break;

            //按 \n 切再 Trim，\r\n 和 \n 两种换行都能吃
            foreach (var raw in patterns.Split('\n'))
            {
                var line = raw.Trim();
                if (line.Length == 0 || line[0] == '#')
                    continue;
                yield return line;
            }
        }

        /// <summary>
        /// 校验一组自定义正则，返回每条写错的说明；全对时返回空列表。
        /// 供设置界面在保存前提示用户。
        /// </summary>
        public static List<string> ValidatePatterns(string patterns)
        {
            var errors = new List<string>();
            foreach (var pattern in EnumeratePatterns(patterns))
            {
                var compiled = GetCompiledPattern(pattern);
                if (compiled.Error != null)
                    errors.Add(compiled.Error);
            }
            return errors;
        }

        /// <summary>
        /// 依次套用每条自定义正则，把命中的内容删掉。
        ///
        /// 写错的那条只是被跳过，不牵连其他规则，更不会让整句话读不出来 ——
        /// 这条路径跑在宿主的说话回调里，抛出去就是一次哑火。
        /// </summary>
        private static string RemoveCustomRegexMatches(string text, string patterns, List<string> errors)
        {
            foreach (var pattern in EnumeratePatterns(patterns))
            {
                var compiled = GetCompiledPattern(pattern);
                if (compiled.Regex == null)
                {
                    errors?.Add(compiled.Error);
                    continue;
                }

                try
                {
                    text = compiled.Regex.Replace(text, "");
                }
                catch (RegexMatchTimeoutException)
                {
                    //回溯爆炸：这条作废，别把说话卡在这儿
                    errors?.Add($"正则匹配超时，已跳过：{pattern}");
                }
            }

            return text;
        }

        /// <summary>
        /// 编译结果：要么拿到 Regex，要么拿到一句人话的错误说明。
        /// </summary>
        private sealed class CompiledPattern
        {
            public Regex Regex;
            public string Error;
        }

        private static readonly Dictionary<string, CompiledPattern> PatternCache =
            new Dictionary<string, CompiledPattern>(StringComparer.Ordinal);
        private static readonly object PatternCacheLock = new object();


        /// <summary>
        /// 取（或编译并缓存）一条自定义正则。每次说话都会走一遍，所以不能每次重新编译。
        /// </summary>
        private static CompiledPattern GetCompiledPattern(string pattern)
        {
            lock (PatternCacheLock)
            {
                if (PatternCache.TryGetValue(pattern, out var cached))
                    return cached;

                var compiled = new CompiledPattern();
                try
                {
                    compiled.Regex = new Regex(pattern, RegexOptions.None, PatternTimeout);
                }
                catch (ArgumentException ex)
                {
                    compiled.Error = $"正则写错了，已跳过：{pattern} —— {ex.Message}";
                }

                PatternCache[pattern] = compiled;
                return compiled;
            }
        }

        /// <summary>
        /// 扫描并删除确实闭合了的括号区间（含括号本身）。
        ///
        /// 用栈记录未闭合的开括号，只有遇到匹配的闭括号才落成一个待删区间；
        /// 扫到结尾仍留在栈里的开括号视为普通文字保留下来，
        /// 否则一个落单的左括号会把后面整句话都吞掉。
        /// 嵌套时只记录最外层区间，内层随外层一并删除。
        /// </summary>
        private static string RemoveBracketedSpans(string text, List<BracketGroup> groups)
        {
            var openStack = new List<(int GroupIndex, int Position)>();
            var spans = new List<(int Start, int End)>();

            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];

                var closeGroup = IndexOfGroupByClose(groups, c);
                if (closeGroup >= 0 && TryPopMatchingOpen(openStack, closeGroup, out var openPosition))
                {
                    // 栈空说明刚闭合的是最外层，记录整段
                    if (openStack.Count == 0)
                        spans.Add((openPosition, i));
                    continue;
                }

                var openGroup = IndexOfGroupByOpen(groups, c);
                if (openGroup >= 0)
                    openStack.Add((openGroup, i));
            }

            if (spans.Count == 0)
                return text;

            var builder = new StringBuilder(text.Length);
            var cursor = 0;
            foreach (var (start, end) in spans)
            {
                if (start > cursor)
                    builder.Append(text, cursor, start - cursor);
                cursor = end + 1;
            }
            if (cursor < text.Length)
                builder.Append(text, cursor, text.Length - cursor);

            return builder.ToString();
        }

        /// <summary>
        /// 自栈顶向下找同组的开括号。找到就连同其上的未闭合项一起弹出，
        /// 那些是交错写法里的残次品，跟着一起删掉。
        /// </summary>
        private static bool TryPopMatchingOpen(
            List<(int GroupIndex, int Position)> openStack,
            int groupIndex,
            out int openPosition)
        {
            for (var i = openStack.Count - 1; i >= 0; i--)
            {
                if (openStack[i].GroupIndex != groupIndex)
                    continue;

                openPosition = openStack[i].Position;
                openStack.RemoveRange(i, openStack.Count - i);
                return true;
            }

            openPosition = -1;
            return false;
        }

        private static int IndexOfGroupByOpen(List<BracketGroup> groups, char c)
        {
            for (var i = 0; i < groups.Count; i++)
            {
                if (groups[i].Open.IndexOf(c) >= 0)
                    return i;
            }
            return -1;
        }

        private static int IndexOfGroupByClose(List<BracketGroup> groups, char c)
        {
            for (var i = 0; i < groups.Count; i++)
            {
                if (groups[i].Close.IndexOf(c) >= 0)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// 收拾删除后留下的空白和孤立标点，避免 TTS 念出奇怪的停顿
        /// </summary>
        private static string Normalize(string text)
        {
            text = InlineWhitespacePattern.Replace(text, " ");
            text = DuplicatePunctuationPattern.Replace(text, "");

            var lines = text.Split('\n');
            var kept = new List<string>(lines.Length);
            foreach (var raw in lines)
            {
                var line = LeadingPunctuationPattern.Replace(raw.Trim(), "").Trim();
                if (line.Length > 0)
                    kept.Add(line);
            }

            return string.Join("\n", kept);
        }

        /// <summary>
        /// 是否还有值得念的内容，有字母或数字即算
        /// </summary>
        private static bool HasSpeakableContent(string text)
        {
            foreach (var c in text)
            {
                if (char.IsLetterOrDigit(c))
                    return true;
            }
            return false;
        }
    }
}
