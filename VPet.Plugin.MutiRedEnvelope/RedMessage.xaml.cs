using LinePutScript.Localization.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VPet_Simulator.Windows.Interface;
using static VPet.Plugin.MutiRedEnvelope.TabRedEnv;

namespace VPet.Plugin.MutiRedEnvelope
{
    /// <summary>
    /// RedMessage.xaml 的交互逻辑
    /// </summary>
    public partial class RedMessage : Border
    {
        IMPWindows IMP;
        TabRedEnv TRE;
        bool isget = false;
        public int RedID = -1;
        public RedMessage(IMPWindows imp, IMainWindow imw, TabRedEnv tre, RedMessageData data)
        {
            InitializeComponent();
            IMP = imp;
            TRE = tre;
            RedID = data.REDID;
            if (data.IsFake)
                isfake.Text = "(玩具钞)".Translate();

            IMPFriend? sender;
            //先看看发送者是不是自己
            if (imw.SteamID == data.SenderID)
            {
                sender = imp.SelftoIMPFriend();
            }
            else
            {
                sender = IMP.Friends.FirstOrDefault(x => x.FriendID == data.SenderID);
            }
            hostName.Text = sender?.Name ?? data.SenderID.ToString("X");
            rPetName.Text = sender?.Core.Save.Name ?? "萝莉斯";
            uimg.Source = sender?.Avatar;

            totalvalue.Text = "$" + data.Money.ToString("f2");
            msg.Text = data.Message;

            switch (data.Type)
            {
                case RedMessageData.RedMessageType.Random:
                    redType.Text = "拼手气".Translate();
                    break;
                case RedMessageData.RedMessageType.Normal:
                    redType.Text = "平分".Translate();
                    break;
                case RedMessageData.RedMessageType.Someone:
                    IMPFriend? target;
                    if (data.TargetID == imw.SteamID)
                    {
                        target = imp.SelftoIMPFriend();
                    }
                    else
                    {
                        target = IMP.Friends.FirstOrDefault(x => x.FriendID == data.TargetID);
                    }
                    redType.Text = "专属".Translate() + target?.Name ?? "";
                    break;
                case RedMessageData.RedMessageType.Talk:
                    redType.Text = "口令".Translate();
                    break;
            }

            UpdateData(data, imw);
        }

        public void UpdateButton()
        {
            if (isget)
                if (MoreInfo.Visibility == Visibility.Visible)
                {
                    btnOpen.Content = "收起".Translate();
                }
                else
                {
                    btnOpen.Content = "展开".Translate();
                }
            else
            {
                btnOpen.Content = "领取".Translate();
            }
        }

        public void UpdateData(RedMessageData data, IMainWindow imw)
        {
            //先看看自己领了吗
            int dataids = data.GetDatas.FindIndex(x => x.GetterID == imw.SteamID);
            if (dataids != -1)
            {
                isget = true;
            }
            else if (data.Type == RedMessageData.RedMessageType.Someone && data.TargetID != imw.SteamID)
            {//如果是专属红包, 但是不是自己的, 那么就当做领过了
                isget = true;
            }
            UpdateButton();
            //更新信息
            receivedMouney.Text = (data.Money - data.LeftMoney).ToString("f2");
            TotalMouney.Text = data.Money.ToString("f2");
            receivedCount.Text = data.GetDatas.Count.ToString();
            totalCount.Text = data.Count.ToString();
            for (int i = MoreInfo.Children.Count - 1; i < data.GetDatas.Count; i++)
            {
                IMPFriend? getter;
                if (data.GetDatas[i].GetterID == imw.SteamID)
                {
                    getter = IMP.SelftoIMPFriend();
                }
                else
                    getter = IMP.Friends.FirstOrDefault(x => x.FriendID == data.GetDatas[i].GetterID);
                var blg = new BorderListGet(getter, data.GetDatas[i]);
                MoreInfo.Children.Add(blg);
            }
            if (data.Count == data.GetDatas.Count)
            {
                //如果红包已经被领完了, 就看看谁是手气最佳
                double max = data.GetDatas.Max(x => x.Money);
                var best = data.GetDatas.FirstOrDefault(x => x.Money == max);
                if (best != null)
                {
                    var bestborder = MoreInfo.Children[data.GetDatas.IndexOf(best) + 1] as BorderListGet;
                    bestborder?.SetIsBest();
                }
            }
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            if (isget)
            {
                if (MoreInfo.Visibility == Visibility.Visible)
                {
                    MoreInfo.Visibility = Visibility.Collapsed;
                }
                else
                {
                    MoreInfo.Visibility = Visibility.Visible;
                }
            }
            else
            {//如果没有领取过, 就展开, 然后发送申请领取情况
                if (IMP.IsHost)
                {//直接内部处理
                    TRE.GetRed(RedID, IMP.HostID);
                }
                else
                {
                    //发送申请领取的消息
                    MPMessage msg = new MPMessage
                    {
                        Type = (int)MPType.REDENV_GetRed
                    };
                    msg.SetContent(RedID);
                    IMP.SendMessage(IMP.HostID, msg);
                }
                MoreInfo.Visibility = Visibility.Visible;
            }
            UpdateButton();
        }
    }
}
