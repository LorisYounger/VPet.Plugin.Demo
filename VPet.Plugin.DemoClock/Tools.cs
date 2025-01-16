using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VPet.Plugin.DemoClock
{
    internal static class Tools
    {
        /// <summary>
        /// 语音类型检测函数
        /// </summary>
        /// <param name="input">原始语音内容</param>
        /// <param name="value">去掉类型符后的语音内容</param>
        /// <returns>语音类型</returns>
        public static bool TryGetInputTypeAndValue(string input, out string value)
        {
            value = string.Empty;

            if (string.IsNullOrEmpty(input))
            {
                return false;
            }

            // 检查 "file:" 前缀
            if (input.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
            {
                value = input.Substring(5); // 去掉 "file:"
                return true;
            }
            // 检查 "text:" 前缀
            else if (input.StartsWith("text:", StringComparison.OrdinalIgnoreCase))
            {
                value = input.Substring(5); // 去掉 "text:"
                return false;
            }
            return false;
        }
        /// <summary>
        /// 标志位替换函数
        /// </summary>
        /// <param name="input">原始文本，包含标志位(标志位格式为{标志位})(</param>
        /// <param name="replacements">替换字典</param>
        /// <returns>替换后文本</returns>
        public static string ReplacePlaceholders(string input, Dictionary<string, string> replacements)
        {
            if (string.IsNullOrEmpty(input) || replacements == null || replacements.Count == 0)
            {
                return input; // 如果输入为空或没有替换规则，返回原文本
            }

            // 使用正则表达式查找所有的标志符（格式为 {标志符}）
            string pattern = @"\{([^\}]+)\}";
            return Regex.Replace(input, pattern, match =>
            {
                string placeholder = match.Groups[1].Value; // 提取标志符名（去掉 {}）
                return replacements.ContainsKey(placeholder) ? replacements[placeholder] : match.Value; // 如果有替换值则替换，否则保持原样
            });
        }
    }
}
