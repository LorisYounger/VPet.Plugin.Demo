using LinePutScript;
using LinePutScript.Localization.WPF;
using Panuon.WPF.UI;
using Steamworks;
using Steamworks.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using VPet_Simulator.Windows.Interface;

namespace VPet.MutiPlayer.Stream;
/// <summary>
/// MPUserControl.xaml 的交互逻辑
/// </summary>
public partial class MPUserControl : Border
{
    public Friend friend;
    winMutiPlayer wmp;
    Lobby lb;
    public MPUserControl(winMutiPlayer wmp, Friend friend, Lobby lb)
    {
        InitializeComponent();
        this.wmp = wmp;
        this.friend = friend;
        this.lb = lb;
        Resources = Application.Current.Resources;

        Task.Run(LoadInfo);
    }
    public void LoadInfo()
    {
        //加载lobby传过来的数据       
        string tmp = lb.GetMemberData(friend, "save");
        while (string.IsNullOrEmpty(tmp))
        {
            Thread.Sleep(500);
            tmp = lb.GetMemberData(friend, "save");
        }
        var Save = GameSave_VPet.Load(new Line(tmp));
        Dispatcher.Invoke(async () =>
        {
            rPetName.Text = Save.Name;
            hostName.Text = friend.Name;
            var img = await friend.GetMediumAvatarAsync();
            uimg.Source = winMutiPlayer.ConvertToImageSource(img);
            info.Text = "Lv " + Save.Level;
            if (lb.Owner.IsMe)
                Kick.Visibility = Visibility.Visible;
        });
    }

    private void btn_ReSetLocal(object sender, RoutedEventArgs e)
    {
        if (!wmp.mps.Set.WhiteTalkList.Contains(friend.Id.Value.ToString()))
        {
            wmp.tbWhiteTalkList.Text = wmp.mps.Set.WhiteJoinList + "," + friend.Id.Value.ToString();
            if (!wmp.mps.Set.WhiteJoinList.Contains(friend.Id.Value.ToString()))
            {
                wmp.tbWhiteJoinList.Text = wmp.mps.Set.WhiteJoinList + "," + friend.Id.Value.ToString();
            }
            MessageBoxX.Show("已将{0}加入白名单".Translate(friend.Name));
        }
        else
        {
            wmp.tbWhiteTalkList.Text = wmp.mps.Set.WhiteTalkList.Replace(friend.Id.Value.ToString(), "")
                .Replace(",,", "");
            if (wmp.mps.Set.WhiteJoinList.Contains(friend.Id.Value.ToString()))
            {
                wmp.tbWhiteJoinList.Text = wmp.mps.Set.WhiteJoinList.Replace(friend.Id.Value.ToString(), "")
                .Replace(",,", "");
            }
            MessageBoxX.Show("已将{0}移出白名单".Translate(wmp.mps.FilterName(friend)));
        }
    }


    private void Kick_Click(object sender, RoutedEventArgs e)
    {
        lb.SetData("kick", friend.Id.Value.ToString());
    }
}
