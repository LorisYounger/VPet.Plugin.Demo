using LinePutScript;
using LinePutScript.Localization.WPF;
using Panuon.WPF.UI;
using Steamworks;
using Steamworks.Data;
using Steamworks.ServerList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VPet_Simulator.Core;
using VPet_Simulator.Windows.Interface;
using static VPet_Simulator.Core.GraphInfo;
using static VPet_Simulator.Windows.Interface.MPMessage;

namespace VPet.MutiPlayer.Stream;
/// <summary>
/// winMutiPlayer.xaml 的交互逻辑
/// </summary>
public partial class winMutiPlayer : WindowX, IMPWindows
{
    public Lobby lb;
    IMainWindow? mw;
    public MutiPlayerStream? mps;
    /// <summary>
    /// 好友宠物模块
    /// </summary>
    public List<MPUserControl> MPUserControls = new List<MPUserControl>();
    public winMutiPlayer(MutiPlayerStream mps, ulong? lobbyid = null)
    {
        Resources = Application.Current.Resources;
        this.mps = mps;
        this.mw = mps.MW;
        InitializeComponent();


        swAllowTouch.IsChecked = !mw.Set.MPNOTouch;

        //显示设置
        swAllowTalk.IsChecked = mps.Set.AllowTalk;
        swAllowName.IsChecked = mps.Set.AllowName;
        swWordsCheck.IsChecked = mps.Set.WordsCheck;
        swWorkReplace.IsChecked = mps.Set.WorkReplace;
        tbDIYSensitive.Password = mps.Set.DIYSensitive;
        tbWhiteJoinList.Text = mps.Set.WhiteJoinList;
        tbWhiteTalkList.Text = mps.Set.WhiteTalkList;

        if (lobbyid == null)
            CreateLobby();
        else
            JoinLobby(lobbyid.Value);
    }


    public async void JoinLobby(ulong lobbyid)
    {
        var lbt = (await SteamMatchmaking.JoinLobbyAsync((SteamId)lobbyid));
        if (!lbt.HasValue || lbt.Value.Owner.Id.Value == 0)
        {
            MessageBoxX.Show("加入/创建访客表失败，请检查网络连接或重启游戏".Translate());
            Close();
            return;
        }
        lb = lbt.Value;
        ShowLobbyInfo();
    }
    public async void CreateLobby()
    {
        var lbt = (await SteamMatchmaking.CreateLobbyAsync());
        if (!lbt.HasValue)
        {
            MessageBoxX.Show("加入/创建访客表失败，请检查网络连接或重启游戏".Translate());
            Close();
            return;
        }
        lb = lbt.Value;
        lb.SetJoinable(true);
        lb.SetPublic();
        IsHost = true;
        swAllowJoin.IsEnabled = true;
        ShowLobbyInfo();
    }
    public static ImageSource ConvertToImageSource(Steamworks.Data.Image? img)
    {
        if (img == null)
        {
            return new BitmapImage(new Uri("pack://application:,,,/Res/vpeticon.png"));
        }
        var image = img.Value;
        int stride = (int)((image.Width * 32 + 7) / 8); // 32 bits per pixel
                                                        // Convert RGBA to BGRA
        for (int i = 0; i < image.Data.Length; i += 4)
        {
            byte r = image.Data[i];
            image.Data[i] = image.Data[i + 2];
            image.Data[i + 2] = r;
        }
        var bitmap = BitmapSource.Create(
            (int)image.Width,
            (int)image.Height,
            96, 96, // dpi x, dpi y
            PixelFormats.Bgra32, // Pixel format
            null, // Bitmap palette
            image.Data, // Pixel data
            stride // Stride
        );

        // Convert to ImageSource
        var stream = new MemoryStream();
        var encoder = new PngBitmapEncoder(); // or use another encoder if you want
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        encoder.Save(stream);
        stream.Seek(0, SeekOrigin.Begin);

        BitmapFrame result = BitmapFrame.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
        return result;
    }

    public ulong HostID { get; set; }
    public bool IsHost { get; set; } = false;

    public ulong LobbyID => lb.Id.Value;

    public bool Joinable { get; set; } = true;

    public IEnumerable<IMPFriend> Friends => new List<IMPFriend>();

    public bool IsGameRunning { get; set; }

    public TabControl TabControl => tabControl;

    public void ShowLobbyInfo()
    {
        _ = Task.Run(async () =>
        {
            lb.SetMemberData("save", mw.GameSavesData.GameSave.ToLine().ToString());
            lb.SetMemberData("onmod", ((ILPS)mw.Set).FindLine("onmod")?.ToString() ?? "onmod");
            lb.SetMemberData("petgraph", ((ILPS)mw.Set)["gameconfig"].GetString("petgraph", "vup"));
            lb.SetMemberData("notouch", mw.Set.MPNOTouch.ToString());

            SteamMatchmaking.OnLobbyMemberJoined += SteamMatchmaking_OnLobbyMemberJoined;
            SteamMatchmaking.OnLobbyMemberLeave += SteamMatchmaking_OnLobbyMemberLeave;
            SteamMatchmaking.OnLobbyDataChanged += SteamMatchmaking_OnLobbyDataChanged;

            Steamworks.Data.Image? img = await lb.Owner.GetMediumAvatarAsync();

            Dispatcher.Invoke(() =>
            {
                hostName.Text = lb.Owner.Name;
                HostID = lb.Owner.Id.Value;
                lbLid.Text = "双击显示房间ID".Translate();
                HostHead.ImageSource = ConvertToImageSource(img.Value);
            });

            SteamNetworking.AllowP2PPacketRelay(true);
            SteamNetworking.OnP2PSessionRequest = (steamid) =>
            {
                SteamNetworking.AcceptP2PSessionWithUser(steamid);
            };

            //给自己动画添加绑定
            mw.Main.GraphDisplayHandler += Main_GraphDisplayHandler;
            mw.Main.TimeHandle += Main_TimeHandle;
            if (IsHost)
            {
                Dispatcher.Invoke(() =>
                {
                    hostPet.Text = mw.GameSavesData.GameSave.Name;
                    Title = "{0}的访客表".Translate(mw.GameSavesData.GameSave.Name);
                });
            }
            //获取成员列表
            foreach (var v in lb.Members)
            {
                if (v.Id == SteamClient.SteamId) continue;
                Dispatcher.Invoke(() =>
              {
                  var mpuc = new MPUserControl(this, v, lb);
                  MUUCList.Children.Add(mpuc);
                  MPUserControls.Add(mpuc);
              });
                if (v.Id == lb.Owner.Id)
                    _ = Task.Run(() =>
                    {
                        //加载lobby传过来的数据
                        string tmp = lb.GetMemberData(v, "save");
                        while (string.IsNullOrEmpty(tmp))
                        {
                            Thread.Sleep(500);
                            tmp = lb.GetMemberData(v, "save");
                        }
                        var Save = GameSave_VPet.Load(new Line(tmp));
                        Dispatcher.Invoke(() =>
                        {
                            Title = "{0}的访客表".Translate(Save.Name);
                            hostPet.Text = Save.Name;
                        });
                    });
            }
            mw.MutiPlayerStart(this);
            Log("已成功连接到访客表".Translate());
            LoopP2PPacket();
        });
    }

    private void Main_TimeHandle(Main obj)
    {
        lb.SetMemberData("save", mw.GameSavesData.GameSave.ToLine().ToString());
    }

    private void SteamMatchmaking_OnLobbyDataChanged(Lobby lobby)
    {
        if (lb.Id == lobby.Id)
        {
            if (lb.GetData("kick") == SteamClient.SteamId.Value.ToString())
            {
                Task.Run(() => MessageBox.Show("访客表已被房主{0}关闭".Translate(lb.Owner.Name)));//温柔的谎言
                lb.Leave();
                lb = default(Lobby);
                Close();
            }

            if (lb.GetData("nojoin") == "true")
            {
                Joinable = false;
                Dispatcher.Invoke(() => swAllowJoin.IsChecked = false);
            }
            else
            {
                Joinable = true;
                Dispatcher.Invoke(() => swAllowJoin.IsChecked = true);
            }
        }
    }

    public event Action<ulong> OnMemberLeave;
    private void SteamMatchmaking_OnLobbyMemberLeave(Lobby lobby, Friend friend)
    {
        if (lobby.Id != lb.Id) return;
        OnMemberLeave?.Invoke(friend.Id);
        if (friend.Id == HostID)
        {
            Task.Run(() => MessageBox.Show("访客表已被房主{0}关闭".Translate(mps.FilterName(friend))));
            lb = default(Lobby);
            Close();
        }
        else
        {
            var mpuc = MPUserControls.Find(x => x.friend.Id == friend.Id);
            if (mpuc != null)
            {
                MPUserControls.Remove(mpuc);
                MUUCList.Children.Remove(mpuc);
            }
            Log("好友{0}已退出访客表".Translate(mps.FilterName(friend)));
        }
    }
    GraphInfo lastgraph = new GraphInfo() { Type = GraphType.Common };
    private void Main_GraphDisplayHandler(GraphInfo info)
    {
        if (info.Type == GraphType.Shutdown || info.Type == GraphType.Common || info.Type == GraphType.Move
            || info.Type == GraphType.Raised_Dynamic || info.Type == GraphType.Raised_Static || info.Type == GraphType.Say)
        {
            return;
        }
        //如果是同一个动画就不发送
        if (lastgraph.Type == info.Type && lastgraph.Animat == info.Animat && info.Name == lastgraph.Name)
            return;
        lastgraph = info;
        MPMessage msg = new MPMessage();
        msg.Type = (int)MSGType.DispayGraph;
        msg.SetContent(info);
        msg.To = SteamClient.SteamId.Value;
        SendMessageALL(msg);
    }
    /// <summary>
    /// 给指定好友发送消息
    /// </summary>
    public bool SendMessage(ulong friendid, MPMessage msg)
    {
        byte[] data = ConverTo(msg);
        return SteamNetworking.SendP2PPacket(friendid, data);
    }
    /// <summary>
    /// 给所有人发送消息
    /// </summary>
    public void SendMessageALL(MPMessage msg)
    {
        byte[] data = ConverTo(msg);
        for (int i = 0; i < MPUserControls.Count; i++)
        {
            SteamNetworking.SendP2PPacket(MPUserControls[i].friend.Id, data);
        }
    }

    /// <summary>
    /// 发送日志消息
    /// </summary>
    /// <param name="message">日志</param>
    public void Log(string message)
    {
        Dispatcher.Invoke(() => tbLog.AppendText($"[{DateTime.Now.ToShortTimeString()}]{message}\n"));
    }

    /// <summary>
    /// 事件:成员加入
    /// </summary>
    public event Action<ulong> OnMemberJoined;
    private void SteamMatchmaking_OnLobbyMemberJoined(Lobby lobby, Friend friend)
    {
        if (lobby.Id == lb.Id && MPUserControls.Find(x => x.friend.Id == friend.Id) == null)
        { //如果有未处理的退出,不管
            Log("好友{0}已加入访客表".Translate(mps.FilterName(friend)));
            var mpuc = new MPUserControl(this, friend, lb);
            MUUCList.Children.Add(mpuc);
            MPUserControls.Add(mpuc);
            OnMemberJoined?.Invoke(friend.Id);
        }
    }
    private void LoopP2PPacket()
    {
        while (isOPEN)
            try
            {
                while (SteamNetworking.IsP2PPacketAvailable())
                {
                    var packet = SteamNetworking.ReadP2PPacket();
                    if (packet.HasValue)
                    {
                        SteamId From = packet.Value.SteamId;
                        var MSG = ConverTo(packet.Value.Data);
                        ReceivedMessage?.Invoke(From.Value, MSG);
                        switch (MSG.Type)
                        {
                            case (int)MSGType.DispayGraph:
                                break;
                            case (int)MSGType.Chat:
                                if (!mps.Set.AllowTalk)
                                    break;
                                var msg = MSG.GetContent<Chat>();
                                var byname = mps.FilterName(msg.SendName, (long)From.Value);
                                var content = mps.FilterTalk(msg.Content, (long)From.Value);
                                switch (msg.ChatType)
                                {
                                    case Chat.Type.Private:
                                        mw.Main.Say("{0} 悄悄地对你说: {1}".Translate(byname, content));
                                        Log("{0} 悄悄地对你说: {1}".Translate(byname, content));
                                        break;
                                    case Chat.Type.Internal:
                                        mw.Main.Say("{0} 对你说: {1}".Translate(byname, content));
                                        Log("{0} 对你说: {1}".Translate(byname, content));
                                        break;
                                    case Chat.Type.Public:
                                        mw.Main.Say("{0} 对大家说: {1}".Translate(byname, content));
                                        Log("{0} 对大家说: {1}".Translate(byname, content));
                                        break;
                                }
                                break;
                            case (int)MSGType.Interact:
                                byname = mps.FilterName(lb.Members.First(x => x.Id == From));
                                var interact = MSG.GetContent<Interact>();
                                if (MSG.To == SteamClient.SteamId.Value)
                                {
                                    if (mw.Set.MPNOTouch) return;
                                    bool isok = !IMPFriend.InConvenience(mw.Main);
                                    switch (interact)
                                    {
                                        case Interact.TouchHead:
                                            mw.Main.LabelDisplayShow("{0}在摸{1}的头".Translate(byname, mw.Core.Save.Name), 3000);
                                            if (isok)
                                                DisplayNOCALTouchHead();
                                            break;
                                        case Interact.TouchBody:
                                            mw.Main.LabelDisplayShow("{0}在摸{1}的头".Translate(byname, mw.Core.Save.Name), 3000);
                                            if (isok)
                                                DisplayNOCALTouchBody();
                                            break;
                                        case Interact.TouchPinch:
                                            mw.Main.LabelDisplayShow("{0}在捏{1}的脸".Translate(byname, mw.Core.Save.Name), 3000);
                                            if (isok)
                                                DisplayNOCALTouchPinch();
                                            break;
                                    }
                                }
                                break;
                            case (int)MSGType.Feed:
                                byname = mps.FilterName(lb.Members.First(x => x.Id == From));
                                var feed = MSG.GetContent<Feed>();
                                if (MSG.To == SteamClient.SteamId.Value)
                                {
                                    var item = feed.Item;
                                    feed.Item.ImageSource = Dispatcher.Invoke(() => mw.ImageSources.FindImage("food_" + (item.Image ?? item.Name), "food"));
                                    mw.DisplayFoodAnimation(feed.Item.GetGraph(), feed.Item.ImageSource);
                                    var itemname = mps.Filter(feed.Item.TranslateName);
                                    if (feed.EnableFunction)
                                    {
                                        mw.Main.LabelDisplayShow("{0}花费${3}给{1}买了{2}".Translate(byname, mw.GameSavesData.GameSave.Name, itemname, feed.Item.Price), 6000);
                                        Log("{0}花费${3}给{1}买了{2}".Translate(byname, mw.GameSavesData.GameSave.Name, itemname, feed.Item.Price));
                                        //对于要修改数据的物品一定要再次检查,避免联机开挂毁存档
                                        if (item.Price >= 10 && item.Price <= 1000 && item.Health >= 0 && item.Exp >= 0 && item.Likability >= 0 && giveprice < 1000
                                           && item.Strength >= 0 && item.StrengthDrink >= 0 && item.StrengthFood >= 0 && item.Feeling >= 0)
                                        {//单次联机收礼物上限1000
                                            giveprice += item.Price;
                                            mw.Core.Save.Money += item.Price;
                                            mw.TakeItem(feed.Item);
                                        }
                                    }
                                    else
                                    {
                                        mw.Main.LabelDisplayShow("{0}给{1}买了{2}".Translate(byname, mw.GameSavesData.GameSave.Name, itemname), 6000);
                                        Log("{0}给{1}买了{2}".Translate(byname, mw.GameSavesData.GameSave.Name, itemname));
                                    }
                                }
                                break;
                        }
                    }
                    Thread.Sleep(100);
                }
                Thread.Sleep(1000);
            }
            catch
            {

            }
    }
    private double giveprice = 0;
    public event Action<ulong, MPMessage> ReceivedMessage;
    private void Window_Closed(object sender, EventArgs e)
    {
        mw.Main.TimeHandle -= Main_TimeHandle;
        mw.Main.GraphDisplayHandler -= Main_GraphDisplayHandler;
        SteamMatchmaking.OnLobbyMemberJoined -= SteamMatchmaking_OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave -= SteamMatchmaking_OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyDataChanged -= SteamMatchmaking_OnLobbyDataChanged;
        lb.Leave();
    }
    bool isOPEN = true;
    /// <summary>
    /// 事件: 结束访客表, 窗口关闭
    /// </summary>
    public event Action ClosingMutiPlayer;
    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (!lb.Equals(default(Lobby)))
            if (MessageBoxX.Show("确定要关闭访客表吗?".Translate(), "离开游戏", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            {
                e.Cancel = true;
                return;
            }
        ClosingMutiPlayer?.Invoke();
        isOPEN = false;
    }

    private void swAllowJoin_Checked(object sender, RoutedEventArgs e)
    {
        lb.SetData("nojoin", "false");
        lb.SetJoinable(true);
    }

    private void swAllowJoin_Unchecked(object sender, RoutedEventArgs e)
    {
        lb.SetData("nojoin", "true");
        lb.SetJoinable(false);
    }

    /// <summary>
    /// 显示本体摸头情况
    /// </summary>
    public void DisplayNOCALTouchHead()
    {
        if (mw.Main.DisplayType.Type == GraphType.Touch_Head)
        {
            if (mw.Main.DisplayType.Animat == AnimatType.A_Start)
                return;
            else if (mw.Main.DisplayType.Animat == AnimatType.B_Loop)
                if (Dispatcher.Invoke(() => mw.Main.PetGrid.Tag) is IGraph ig && ig.GraphInfo.Type == GraphType.Touch_Head && ig.GraphInfo.Animat == AnimatType.B_Loop)
                {
                    ig.IsContinue = true;
                    return;
                }
                else if (Dispatcher.Invoke(() => mw.Main.PetGrid2.Tag) is IGraph ig2 && ig2.GraphInfo.Type == GraphType.Touch_Head && ig2.GraphInfo.Animat == AnimatType.B_Loop)
                {
                    ig2.IsContinue = true;
                    return;
                }
        }
        mw.Main.Display(GraphType.Touch_Head, AnimatType.A_Start, (graphname) =>
           mw.Main.Display(graphname, AnimatType.B_Loop, (graphname) =>
           mw.Main.Display(graphname, AnimatType.B_Loop, (graphname) =>
           mw.Main.DisplayCEndtoNomal(graphname))));
    }
    /// <summary>
    /// 显示摸身体情况
    /// </summary>
    public void DisplayNOCALTouchBody()
    {
        if (mw.Main.DisplayType.Type == GraphType.Touch_Body)
        {
            if (mw.Main.DisplayType.Animat == AnimatType.A_Start)
                return;
            else if (mw.Main.DisplayType.Animat == AnimatType.B_Loop)
                if (Dispatcher.Invoke(() => mw.Main.PetGrid.Tag) is IGraph ig && ig.GraphInfo.Type == GraphType.Touch_Body && ig.GraphInfo.Animat == AnimatType.B_Loop)
                {
                    ig.IsContinue = true;
                    return;
                }
                else if (Dispatcher.Invoke(() => mw.Main.PetGrid2.Tag) is IGraph ig2 && ig2.GraphInfo.Type == GraphType.Touch_Body && ig2.GraphInfo.Animat == AnimatType.B_Loop)
                {
                    ig2.IsContinue = true;
                    return;
                }
        }
        mw.Main.Display(GraphType.Touch_Body, AnimatType.A_Start, (graphname) =>
         mw.Main.Display(graphname, AnimatType.B_Loop, (graphname) =>
         mw.Main.Display(graphname, AnimatType.B_Loop, (graphname) =>
         mw.Main.DisplayCEndtoNomal(graphname))));
    }
    /// <summary>
    /// 显示本体捏脸情况
    /// </summary>
    public void DisplayNOCALTouchPinch()
    {
        if (mw.Main.DisplayType.Name == "pinch")
        {
            if (mw.Main.DisplayType.Animat == AnimatType.A_Start)
                return;
            else if (mw.Main.DisplayType.Animat == AnimatType.B_Loop)
                if (Dispatcher.Invoke(() => mw.Main.PetGrid.Tag) is IGraph ig && ig.GraphInfo.Type == GraphType.Touch_Head && ig.GraphInfo.Animat == AnimatType.B_Loop)
                {
                    ig.IsContinue = true;
                    return;
                }
                else if (Dispatcher.Invoke(() => mw.Main.PetGrid2.Tag) is IGraph ig2 && ig2.GraphInfo.Type == GraphType.Touch_Head && ig2.GraphInfo.Animat == AnimatType.B_Loop)
                {
                    ig2.IsContinue = true;
                    return;
                }
        }
        mw.Main.Display("pinch", AnimatType.A_Start, (graphname) =>
           mw.Main.Display(graphname, AnimatType.B_Loop, (graphname) =>
           mw.Main.Display(graphname, AnimatType.B_Loop, (graphname) =>
           mw.Main.DisplayCEndtoNomal(graphname))));
    }

    private void swAllowTouch_Checked(object sender, RoutedEventArgs e)
    {
        if (mw == null) return;
        lb.SetMemberData("notouch", "false");
        mw.Set.MPNOTouch = false;
    }

    private void swAllowTouch_Unchecked(object sender, RoutedEventArgs e)
    {
        if (mw == null) return;
        lb.SetMemberData("notouch", "true");
        mw.Set.MPNOTouch = true;
    }

    private void lbLid_PreviewMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (lbLid.Text == "双击显示房间ID".Translate())
            lbLid.Text = lb.Id.Value.ToString("x");
        else
            lbLid.Text = "双击显示房间ID".Translate();
        e.Handled = true;
    }
}
