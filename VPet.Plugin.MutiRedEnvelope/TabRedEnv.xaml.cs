using LinePutScript.Localization.WPF;
using Microsoft.VisualBasic;
using Panuon.WPF.UI;
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
using static System.Net.Mime.MediaTypeNames;
using static VPet_Simulator.Windows.Interface.MPMessage;

namespace VPet.Plugin.MutiRedEnvelope
{
    /// <summary>
    /// TabRedEnv.xaml 的交互逻辑
    /// </summary>
    public partial class TabRedEnv : TabItem
    {
        IMPWindows IMP;
        IMainWindow IMW;
        /// <summary>
        /// 红包列表, 由主机维护并发送给玩家
        /// </summary>
        public List<RedMessageData> RedList = new List<RedMessageData>();

        public TabRedEnv(IMPWindows imp, IMainWindow imw)
        {
            InitializeComponent();
            IMP = imp;
            IMW = imw;
            IMP.TabControl.Items.Add(this);
            imp.ClosingMutiPlayer += IMP_ClosingMutiPlayer;
            imp.ReceivedMessage += IMP_ReceivedMessage;
            imp.OnMemberLeave += Imp_OnMemberLeave;
            TxtWishes.Text = "恭喜发财, 大吉大利".Translate();
            if (!imp.IsHost)
            {//如果不是主机, 就请求红包列表
                MPMessage mpm = new MPMessage();
                mpm.Type = (int)MPType.REDENV_Want_List;
                NotiNoHost.Visibility = Visibility.Visible;
                TabCR.IsEnabled = false;                
                IMP.SendMessage(IMP.HostID, mpm);
            }
        }

        private void Imp_OnMemberLeave(ulong obj)
        {
            if (IMP.IsHost)
            {
                //如果是主机, 因为这个玩家离开退款了, 把这个玩家的红包剩余金钱更新为0, 并群发红包列表更新
                RedList.FindAll(x => x.SenderID == obj).ForEach(x => x.LeftMoney = 0);
                var mpm = new MPMessage();
                mpm.Type = (int)MPType.REDENV_RedList;
                mpm.SetContent(RedList);
                IMP.SendMessageALL(mpm);
            }
        }

        /// <summary>
        /// 红包使用的通信频道 -192000-192999
        /// </summary>
        /// 其他联机MOD请勿使用此频道，以免冲突, 可以随便取个其他频道, 只要不和其他MOD冲突即可
        public enum MPType
        {
            /// <summary>
            /// 想要红包列表 (一般是新玩家给主机发送,获取红包列表)
            /// </summary>
            REDENV_Want_List = -192001,
            /// <summary>
            /// 红包列表 (主机发送,给玩家发送红包列表(更新数据:eg:有人领取了红包等))
            /// </summary>
            REDENV_RedList = -192002,
            /// <summary>
            /// 新红包 (主机发送,告诉所有人有新红包了)
            /// </summary>
            REDENV_NewRed = -192003,
            /// <summary>
            /// 创建红包 (玩家发送,告诉主机想要创建一个红包,然后由主机群发)
            /// </summary>
            REDENV_CreateRed = -192004,
            /// <summary>
            /// 领取红包 (玩家发送,告诉主机想要领取红包,然后由主机处理并更新红包列表RedList)
            /// </summary>
            REDENV_GetRed = -192005,
            /// <summary>
            /// 返回领取红包结果 (主机返回, 告诉玩家 领取成功,领取失败等)
            /// </summary>
            REDENV_GetRedResp = -192006,
        }

        /// <summary>
        /// 收到联机消息时触发 注意要UI线程操作界面需要 Dispatcher.Invoke 或者 Dispatcher.BeginInvoke
        /// </summary>
        private void IMP_ReceivedMessage(ulong from, MPMessage message)
        {
            switch (message.Type)
            {
                case (int)MPType.REDENV_Want_List:
                    if (IMP.IsHost)
                    {
                        //给请求者发送红包列表
                        var mpm = new MPMessage();
                        mpm.Type = (int)MPType.REDENV_RedList;
                        mpm.SetContent(RedList);
                        IMP.SendMessage(from, mpm);
                    }
                    break;
                case (int)MPType.REDENV_RedList:
                    if (!IMP.IsHost)
                    {
                        //收到红包列表, 更新本地数据
                        var list = message.GetContent<List<RedMessageData>>();
                        RedList = list;//缓存红包列表数据, 以后可以用来退款等操作
                        //更新界面
                        Dispatcher.Invoke(() =>
                        {
                            UpdateRedList(list);
                            if (TabCR.IsEnabled == false)
                            {
                                NotiNoHost.Visibility = Visibility.Collapsed;
                                TabCR.IsEnabled = true;
                            }
                        });
                    }
                    break;
                case (int)MPType.REDENV_CreateRed:
                    if (IMP.IsHost)
                    {
                        //收到新红包消息, 更新本地数据
                        var red = message.GetContent<RedMessageData>();
                        red.SenderID = from;
                        red.REDID = RedList.Count + 1;
                        //更新列表
                        RedList.Add(red);
                        //群发消息
                        var mpm = new MPMessage();
                        mpm.Type = (int)MPType.REDENV_NewRed;
                        mpm.SetContent(red);
                        IMP.SendMessageALL(mpm);
                        //更新界面
                        Dispatcher.Invoke(() => UpdateRedList(RedList));
                    }
                    break;
                case (int)MPType.REDENV_GetRed:
                    if (IMP.IsHost)
                    { //收到领取红包消息, 处理红包领取逻辑
                        Dispatcher.Invoke(() => GetRed(message.GetContent<int>(), from));
                    }
                    break;
                case (int)MPType.REDENV_GetRedResp:
                    if (!IMP.IsHost)
                    {
                        //收到领取红包结果, 可以弹个消息什么的
                        var resp = message.GetContent<ReturnGetRed>();
                        Dispatcher.Invoke(() =>
                        {
                            if (resp.IsSuccess)
                            {
                                var red = RedList.FirstOrDefault(x => x.REDID == resp.RedID);
                                if (red != null && red.Type == RedMessageData.RedMessageType.Talk)
                                {
                                    var simp = IMP.SelftoIMPFriend();
                                    MPMessage msg = new();
                                    msg.Type = (int)MSGType.Chat;
                                    msg.SetContent(new Chat()
                                    {
                                        Content = red.Message,
                                        ChatType = Chat.Type.Public,
                                        SendName = simp.Name,
                                    });
                                    msg.To = simp.FriendID;
                                    IMP.SendMessageALL(msg);
                                    IMW.Main.Say("{0} 对大家说: {1}".Translate(simp.Name, red.Message));
                                    IMP.Log("{0} 对大家说: {1}".Translate(simp.Name, red.Message));
                                }
                                MessageBoxX.Show($"成功领取红包! 金额: $".Translate() + resp.Money.ToString("f2"));
                                IMP.Log($"成功领取红包! 金额: $".Translate() + resp.Money.ToString("f2"));
                            }
                            else
                            {
                                MessageBoxX.Show($"领取红包失败! 红包已经被领完了".Translate());
                                IMP.Log($"领取红包失败! 红包已经被领完了".Translate());
                            }
                        });
                    }
                    break;
                case (int)MPType.REDENV_NewRed:
                    if (!IMP.IsHost)
                    {
                        //收到新红包消息, 更新本地数据
                        var reddata = message.GetContent<RedMessageData>();
                        Dispatcher.Invoke(() => StackRedMessage.Children.Add(new RedMessage(IMP, IMW, this, reddata)));
                    }
                    break;
            }
            //其他类型的消息不处理, 一般是通信或者其他MOD的消息
        }
        /// <summary>
        /// 领取红包逻辑, 由主机处理
        /// </summary>
        /// <param name="rid">红包ID</param>
        /// <param name="from">领取者ID(可以是自己)</param>
        public void GetRed(int rid, ulong from)
        {
            bool isself = from == IMW.SteamID;
            var red = RedList.FirstOrDefault(x => x.REDID == rid);
            if (red != null && !red.GetDatas.Any(x => x.GetterID == from))
            {//如果红包存在并且这个玩家没有领取过, 就处理领取
                //看看能不能领
                if (red.Type == RedMessageData.RedMessageType.Someone && red.TargetID != from)
                {
                    //如果是指定红包, 但是领取者不是目标玩家, 就不给领
                    if (isself)
                    {
                        IMP.Log("领取红包失败! 红包已经被领完了".Translate());
                        MessageBoxX.Show("领取红包失败! 红包已经被领完了".Translate());
                    }
                    else
                    {
                        var resp = new MPMessage();
                        resp.Type = (int)MPType.REDENV_GetRedResp;
                        resp.SetContent(new ReturnGetRed()
                        {
                            RedID = rid,
                            IsSuccess = false,
                            Money = 0,
                        });
                        IMP.SendMessage(from, resp);
                    }
                    return;
                }


                //领取
                if (red.Count == red.GetDatas.Count)
                {
                    //红包已经被领完了
                    if (isself)
                    {
                        IMP.Log("领取红包失败! 红包已经被领完了".Translate());
                        MessageBoxX.Show("领取红包失败! 红包已经被领完了".Translate());
                    }
                    else
                    {
                        var resp = new MPMessage();
                        resp.Type = (int)MPType.REDENV_GetRedResp;
                        resp.SetContent(new ReturnGetRed()
                        {
                            RedID = rid,
                            IsSuccess = false,
                            Money = 0,
                        });
                        IMP.SendMessage(from, resp);
                    }
                    return;
                }
                else
                {
                    var ReturnGetRed = new ReturnGetRed()
                    {
                        RedID = rid,
                        IsSuccess = true,
                    };
                    var vcount = red.Count - red.GetDatas.Count;
                    if (vcount == 1)
                    {
                        //如果只剩最后一个了, 就把剩余金额都给这个玩家
                        red.GetDatas.Add(new RedMessageData.GetData()
                        {
                            GetterID = from,
                            Money = red.LeftMoney,
                            GetTime = DateTime.Now,
                        });
                        ReturnGetRed.Money = red.LeftMoney;
                        red.LeftMoney = 0;
                    }
                    else
                    {
                        double money;
                        if (red.Type == RedMessageData.RedMessageType.Normal)
                        {
                            money = Math.Round((double)red.Money / red.Count, 2);//平均金额, 保留两位小数
                            if (red.LeftMoney < money)
                            {
                                money = red.LeftMoney / vcount;
                            }
                        }
                        else
                        {
                            double max = red.LeftMoney / vcount * 2;//随机金额, 平均金额的两倍
                            money = (int)(new Random().NextDouble() * max * 100) / 100;//保留两位小数
                        }
                        if (money < 0.01)
                        {
                            money = 0.01;
                        }
                        red.GetDatas.Add(new RedMessageData.GetData()
                        {
                            GetterID = from,
                            Money = money,
                            GetTime = DateTime.Now,
                        });
                        ReturnGetRed.Money = money;
                        red.LeftMoney -= money;
                    }
                    if (isself)
                    {
                        //如果是自己领取的,就进行加钱等操作,并进行通知
                        MessageBoxX.Show("成功领取红包! 金额: $".Translate() + ReturnGetRed.Money.ToString("f2"));
                        IMP.Log("成功领取红包! 金额: $".Translate() + ReturnGetRed.Money.ToString("f2"));
                        //加钱等操作
                        if (!red.IsFake)
                        {
                            IMW.Core.Save.Money += ReturnGetRed.Money;
                        }
                        //如果是口令红包,群发口令
                        if (red.Type == RedMessageData.RedMessageType.Talk)
                        {
                            var simp = IMP.SelftoIMPFriend();
                            MPMessage msg = new();
                            msg.Type = (int)MSGType.Chat;
                            msg.SetContent(new Chat()
                            {
                                Content = red.Message,
                                ChatType = Chat.Type.Public,
                                SendName = simp.Name,
                            });
                            msg.To = simp.FriendID;
                            IMP.SendMessageALL(msg);
                            IMW.Main.Say("{0} 对大家说: {1}".Translate(simp.Name, red.Message));
                            IMP.Log("{0} 对大家说: {1}".Translate(simp.Name, red.Message));
                        }
                    }
                    else
                    {
                        //如果是客户端, 就回复消息
                        var resp = new MPMessage();
                        resp.Type = (int)MPType.REDENV_GetRedResp;
                        resp.SetContent(ReturnGetRed);
                        IMP.SendMessage(from, resp);
                    }
                    //更新红包列表, 群发给所有人
                    var mpm = new MPMessage();
                    mpm.Type = (int)MPType.REDENV_RedList;
                    mpm.SetContent(RedList);
                    IMP.SendMessageALL(mpm);
                    //更新界面
                    UpdateRedList(RedList);
                }
            }
        }

        private void IMP_ClosingMutiPlayer()
        {//给红包发送者退款
            IMP.ClosingMutiPlayer -= IMP_ClosingMutiPlayer;
            IMP.ReceivedMessage -= IMP_ReceivedMessage;
            IMP.OnMemberLeave -= Imp_OnMemberLeave;
            var refound = RedList.Where(x => x.SenderID == IMW.SteamID && !x.IsFake).Select(x => x.LeftMoney).Sum();
            if (refound > 0)
            {
                IMW.Main.LabelDisplayShow("联机结束未领取红包退款 ${0:f2}".Translate(refound), 5000);
                IMW.Core.Save.Money += refound;
            }
        }
        bool isselecge = false;
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//这玩意好像会被调用几百次的样子, 加个防止多次调用的判断
            if (!IsLoaded) return;
            if (isselecge) return;
            isselecge = true;
            UpdateLimitInfo();
            SVStack.ScrollToEnd();
            isselecge = false;
        }
        /// <summary>
        /// 单次大可以发送的红包金额
        /// </summary>
        public int pmaxmoney = 10000;
        /// <summary>
        /// 更新发红包界面上的限制信息等
        /// </summary>
        public void UpdateLimitInfo()
        {
            //没HashCheck的默认使用假钱
            if (!IMW.GameSavesData.HashCheck)
            {
                chkFake.IsChecked = true;
                chkFake.IsEnabled = false;
            }
            pmaxmoney = 50 + (50 * (IMW.GameSavesData.GameSave.LevelMax + 1) + IMW.GameSavesData.GameSave.Level * 5);
            if (IMP.Friends.Count() != 0)
            {
                pmaxmoney += (int)IMP.Friends.Select(x => x.Core.Save.Level).Average() * 5;
            }
            runMaxValue.Text = pmaxmoney.ToString();
            runMaxNumber.Text = (IMP.Friends.Count() + 1).ToString();
            if(combTarget.Items.Count == IMP.Friends.Count())
            {
                return;
            }
            combTarget.Items.Clear();
            foreach (var f in IMP.Friends)
            {
                combTarget.Items.Add(f.Name);
            }
            combTarget.SelectedIndex = 0;
        }
        /// <summary>
        /// 发送红包
        /// </summary>
        private void BtnSendRedEnv_Click(object sender, RoutedEventArgs e)
        {
            //检查限制
            if (!int.TryParse(TxtAmount.Text, out int money))
            {
                return;
            }
            if (money < 10)
            {
                MessageBoxX.Show("红包金额不能小于10".Translate());
                return;
            }
            if (money > pmaxmoney)
            {
                MessageBoxX.Show("红包金额不能大于{0}".Translate(pmaxmoney));
                return;
            }
            if (!int.TryParse(TxtCount.Text, out int count))
            {
                return;
            }
            if (count < 1)
            {
                MessageBoxX.Show("红包数量不能小于1".Translate());
                return;
            }
            if (count > IMP.Friends.Count() + 1)
            {
                MessageBoxX.Show("红包数量不能大于{0}".Translate(IMP.Friends.Count() + 1));
                return;
            }
            ulong targetid = 0;
            if (combType.SelectedIndex == 2)
            {
                if (combTarget.SelectedIndex == -1)
                {
                    MessageBoxX.Show("请指定红包的目标玩家".Translate());
                    return;
                }
                else
                {
                    targetid = IMP.Friends.FirstOrDefault(x => x.Name == combTarget.Text)?.FriendID ?? IMW.SteamID;
                    count = 1;//指定红包只能发一个
                }
            }
            if (string.IsNullOrWhiteSpace(TxtWishes.Text))
            {
                TxtWishes.Text = "恭喜发财, 大吉大利".Translate();
            }
            if (chkFake.IsChecked != true)
            {
                if (IMW.Core.Save.Money < money)
                {
                    MessageBoxX.Show("你的金钱不足, 无法发送红包".Translate());
                    return;
                }
                //扣除金钱
                IMW.Core.Save.Money -= money;
            }

            var red = new RedMessageData()
            {
                SenderID = IMW.SteamID,
                Money = int.Parse(TxtAmount.Text),
                Count = int.Parse(TxtCount.Text),
                Message = TxtWishes.Text,
                Type = (RedMessageData.RedMessageType)combType.SelectedIndex,
                IsFake = (chkFake.IsChecked == true),
                TargetID = targetid,
            };
            red.LeftMoney = red.Money;

            if (IMP.IsHost)
            {
                //直接群发
                red.REDID = RedList.Count + 1;
                RedList.Add(red);
                var mpm = new MPMessage();
                mpm.Type = (int)MPType.REDENV_NewRed;
                mpm.SetContent(red);
                IMP.SendMessageALL(mpm);
                UpdateRedList(RedList);
            }
            else
            {
                //发送给主机, 让主机处理           
                var mpm = new MPMessage();
                mpm.Type = (int)MPType.REDENV_CreateRed;
                mpm.SetContent(red);
                IMP.SendMessage(IMP.HostID, mpm);
            }
            var simp = IMP.SelftoIMPFriend();
            //普通的通知信息
            MPMessage msg = new();
            msg.Type = (int)MSGType.Chat;
            msg.SetContent(new Chat()
            {
                Content = "[联机红包]{0}发送一个{1}红包, 请在联机面板中查看 (需要订阅联机红包MOD)".Translate(simp.Name, combType.Text),
                ChatType = Chat.Type.Public,
                SendName = simp.Name,
                ToName = "EVE"
            });
            msg.To = simp.FriendID;
            IMP.SendMessageALL(msg);
            //MSG弹窗
            TabCR.SelectedIndex = 0;
            MessageBoxX.Show("红包发送成功,快来让好友领取吧".Translate());
        }

        /// <summary>
        /// 更新界面上的红包列表
        /// </summary>
        public void UpdateRedList(List<RedMessageData> rlist)
        {
            if (rlist.Count != 0)
            {
                NotiNoRed.Visibility = Visibility.Collapsed;
            }
            else
            {
                return;
            }
            List<RedMessage> rms = new List<RedMessage>();
            foreach (RedMessage rm in StackRedMessage.Children)
            {
                rms.Add(rm);
            }
            foreach (RedMessageData rmd in rlist)
            {
                //看看界面上有没有这个红包了, 没有就创建一个
                var rm = rms.FirstOrDefault(x => x.RedID == rmd.REDID);
                if (rm == null)
                {
                    rm = new RedMessage(IMP, IMW, this, rmd);
                    StackRedMessage.Children.Add(rm);
                }
                else
                {
                    rm.UpdateData(rmd, IMW);
                }
            }
        }

        private void combType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
                return;
            if (combType.SelectedIndex == 2)
            {
                labTarget.Visibility = Visibility.Visible;
                combTarget.Visibility = Visibility.Visible;
            }
            else
            {
                labTarget.Visibility = Visibility.Collapsed;
                combTarget.Visibility = Visibility.Collapsed;
            }
        }
    }
}
