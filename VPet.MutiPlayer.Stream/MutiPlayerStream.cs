
using LinePutScript;
using LinePutScript.Localization.WPF;
using Panuon.WPF.UI;
using Steamworks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using ToolGood.Words;
using VPet_Simulator.Core;
using VPet_Simulator.Windows.Interface;

namespace VPet.MutiPlayer.Stream
{
    public class MutiPlayerStream : MainPlugin
    {
        public MutiPlayerStream(IMainWindow mainwin) : base(mainwin)
        {

        }
        public override void Setting()
        {
            if (!MW.IsSteamUser)
                return;
            if (winMutiPlayer == null)
            {
                winMutiPlayer = new winMutiPlayer(this);
                winMutiPlayer.Show();
            }
            else
            {
                MessageBoxX.Show("已经有加入了一个访客表,无法再创建更多".Translate());
                winMutiPlayer.Focus();
            }
        }
        public override void LoadPlugin()
        {
            if (!MW.IsSteamUser)
                return;
            //最新版本LPS自动转义设置功能, 如果不会用, 可以参考别的项目的旧版本加载方式
            var line = new Line_C<Setting>(MW.Set["MutiPlayerStream"]);
            if (line.Value == null)
            {
                Set = new Setting();
                line.Value = Set;
            }
            else
            {
                Set = line.Value;
            }
            MW.Set["MutiPlayerStream"] = line;

            Task.Run(() =>
            {
                wordsSearch.SetKeywords(Set.DIYSensitive.Split(',').Select(q => q.Trim()).ToArray());

                Thread.Sleep(1000);

                MenuItem? menuItem = null;

                while (menuItem == null)
                {
                    menuItem = FindMenuInteract();
                    Thread.Sleep(1000);
                }

                MW.Main.Dispatcher.Invoke(() =>
                {

                    var menuCreate = new MenuItem()
                    {
                        Header = "[主播]创建".Translate(),
                        HorizontalContentAlignment = HorizontalAlignment.Center
                    };

                    menuCreate.Click += (_, _) =>
                    {
                        if (winMutiPlayer == null)
                        {
                            winMutiPlayer = new winMutiPlayer(this);
                            winMutiPlayer.Show();
                        }
                        else
                        {
                            MessageBoxX.Show("已经有加入了一个访客表,无法再创建更多".Translate());
                            winMutiPlayer.Focus();
                        }
                    };
                    menuItem.Items.Add(menuCreate);

                    //TODO: 加入访客表
                    //var menuJoin = new MenuItem()
                    //{
                    //    Header = "加入".Translate(),
                    //    HorizontalContentAlignment = HorizontalAlignment.Center
                    //};
                    //menuJoin.Click += (_, _) =>
                    //{
                    //    if (winMutiPlayer == null)
                    //    {
                    //        winInputBox.Show(this, "请输入访客表ID".Translate(), "[主播]加入访客表".Translate(), "", (id) =>
                    //        {
                    //            if (ulong.TryParse(id, NumberStyles.HexNumber, null, out ulong lid))
                    //            {
                    //                winMutiPlayer = new winMutiPlayer(this, lid);
                    //                winMutiPlayer.Show();
                    //            }
                    //        });
                    //    }
                    //    else
                    //    {
                    //        MessageBoxX.Show("已经有加入了一个访客表,无法再创建更多".Translate());
                    //        winMutiPlayer.Focus();
                    //    }
                    //};
                    //menuItem.Items.Add(menuJoin);
                });
            });
        }
        winMutiPlayer winMutiPlayer;

        private MenuItem? FindMenuInteract()
        {
            return MW.Main.Dispatcher.Invoke(() =>
               {
                   foreach (var item in MW.Main.ToolBar.MenuInteract.Items)
                   {
                       if (item is MenuItem menu && menu.Header is string hd && hd == "访客表".Translate())
                       {
                           return menu;
                       }
                   }
                   return null;
               });
        }

#pragma warning disable CS0618 // 类型或成员已过时
        IllegalWordsSearch wordsSearch = new IllegalWordsSearch();

        public string Filter(string text)
        {
            if (!Set.WordsCheck)
                return text;
            if (Set.WorkReplace)
                return wordsSearch.Replace(text, '*');
            else
                return wordsSearch.FindFirst(text) == null ? "" : text;
        }
        public string FilterName(Friend friend) => FilterName(friend.Name, (long)friend.Id.Value);
        public string FilterName(string name, long steamid)
        {
            if (!Set.AllowName && Set.WhiteListJoin && Set.WhiteJoinList.Contains(steamid.ToString()))
                return "桌宠好友".Translate() + (steamid % 256).ToString("x");
            return Filter(name);
        }
        public string FilterTalk(string talk, Friend friend) => FilterTalk(talk, (long)friend.Id.Value);
        public string FilterTalk(string talk, long steamid)
        {
            if (Set.WhiteListTalk && Set.WhiteTalkList.Contains(steamid.ToString()))
            {
                if (Set.AllowTalk)
                    return talk;
                else
                    return Filter(talk);
            }
            if (!Set.AllowTalk)
                return "";
            return Filter(talk);
        }
#pragma warning restore CS0618 // 类型或成员已过时

        public override string PluginName => "MutiPlayerStream";

        public Setting Set = new Setting();
    }

}
