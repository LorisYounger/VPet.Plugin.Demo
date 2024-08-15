using CloudSaves.Client;
using LinePutScript.Converter;
using LinePutScript.Localization.WPF;
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
using System.Windows.Shapes;

namespace VPet.Plugin.CloudSaves
{
    /// <summary>
    /// winSetting.xaml 的交互逻辑
    /// </summary>
    public partial class winSetting : Window
    {
        CloudSave CS;
        public winSetting(CloudSave CS)
        {
            InitializeComponent();
            this.CS = CS;
            textservermsg.Text = "点击测试连接查看服务器信息".Translate();
            tb_serverurl.Text = CS.Set.ServerURL ?? "";
            if (CS.Set.Passkey != 0)
                tb_passkey.Text = CS.Set.Passkey.ToString();
        }

        private async void test_connect(object sender, RoutedEventArgs e)
        {
            try
            {
                var v = await new SavesClient(tb_serverurl.Text, CS.MW.SteamID, 0).ServerInfo(LocalizeCore.CurrentCulture);
                textservermsg.Text = v.ContactInformation;
                textserversave.Text = v.TotalSave.ToString();
                textserverusr.Text = v.TotalUser.ToString();
                textserverver.Text = v.Version;
            }
            catch (Exception ex)
            {
                textservermsg.Text = ex.Message;
                textserversave.Text = textserversave.Text = textserverusr.Text = textserverver.Text = "-";
            }
        }

        private void rnd_gen(object sender, RoutedEventArgs e)
        {
            tb_passkey.Text = new Random().NextInt64().ToString();
        }

        bool forclose = false;
        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (forclose)
                return;
            switch (MessageBoxX.Show("是否保存设置?".Translate(), "CloudSaves", MessageBoxButton.YesNoCancel))
            {
                case MessageBoxResult.Yes:
                    if (!await SaveSetting())
                    {
                        e.Cancel = true;
                        return;
                    }
                    break;
                case MessageBoxResult.Cancel:
                    e.Cancel = true;
                    break;
            }
        }

        private async Task<bool> SaveSetting()
        {
            if (ulong.TryParse(tb_passkey.Text, out ulong passkey))
            {
                CS.Set.Passkey = passkey;
                CS.SavesClient.PassKey = passkey;
            }
            else
            {
                MessageBoxX.Show("Passkey只能是数字,且不能大于".Translate() + ulong.MaxValue);
                return false;
            }
            try
            {
                var v = await new SavesClient(tb_serverurl.Text, CS.MW.SteamID, 0).ServerInfo(LocalizeCore.CurrentCulture);
                CS.Set.ServerURL = tb_serverurl.Text;
                CS.SavesClient.ServerUrl = tb_serverurl.Text;
            }
            catch (Exception ex)
            {
                textservermsg.Text = ex.Message;
                textserversave.Text = textserversave.Text = textserverusr.Text = textserverver.Text = "-";
                MessageBoxX.Show("服务器连接失败,请检查服务器地址".Translate());
                return false;
            }
            CS.Set.BackupTime = (int)numbtime.Value;
            CS.BackupTimer.Interval = CS.Set.BackupTime * 60 * 1000;
            CS.MW.Set["CloudSave"] = LPSConvert.SerializeObject(CS.Set, "CloudSave");
            return true;
        }

        private void tb_passkey_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tb_passkey.Text) && !ulong.TryParse(tb_passkey.Text, out _))
                tb_passkey.Text = CS.SavesClient.PassKey.ToString();
        }

        private async void save_connect(object sender, RoutedEventArgs e)
        {
            if (!await SaveSetting())
                return;
            CS.ShowSave();
            forclose = true;
            Close();
        }
    }
}
