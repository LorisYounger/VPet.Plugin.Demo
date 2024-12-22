using CloudSaves.Client;
using LinePutScript;
using LinePutScript.Localization.WPF;
using Microsoft.VisualBasic;
using Panuon.WPF.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VPet_Simulator.Core;
using VPet_Simulator.Windows.Interface;
using static CloudSaves.Client.ReturnStructure;

namespace VPet.Plugin.CloudSaves
{
    /// <summary>
    /// winSetting.xaml 的交互逻辑
    /// </summary>
    public partial class winSave : Window
    {
        CloudSave CS;
        public winSave(CloudSave CS)
        {
            InitializeComponent();
            this.CS = CS;
            Rels();
        }

        private async void load_click(object sender, RoutedEventArgs e)
        {
            if (dgsavelist.SelectedItem == null || dgsavelist.SelectedItem is not SaveList savelist) return;

            if (MessageBoxX.Show("是否确认读取存档".Translate() + " " + savelist.Name, "CloudSaves", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    var save = await CS.SavesClient.GetGameSave(savelist.id);
                    if (save.Count == 0)
                    {
                        MessageBoxX.Show("找不到存档".Translate(), "存档读取失败".Translate());
                        return;
                    }

                    ILPS tmp = new LPS(save.First().SaveData);
                    if (CS.MW.Main.State != Main.WorkingState.Nomal)
                    {
                        CS.MW.Main.WorkTimer.Visibility = Visibility.Collapsed;
                        CS.MW.Main.State = Main.WorkingState.Nomal;
                    }

                    var mi = CS.MW.GetType().GetMethod("SavesLoad");
                    if (mi != null && mi.Invoke(CS.MW, [tmp]) is bool success && success)
                        MessageBoxX.Show("存档读取成功".Translate(), "CloudSaves");
                    else
                        MessageBoxX.Show("存档读取失败".Translate(), "CloudSaves");
                }
                catch (Exception ex)
                {
                    MessageBoxX.Show(ex.Message, "操作失败,请检查存档服务器设置".Translate());
                }
            }
        }

        private void remove_click(object sender, RoutedEventArgs e)
        {
            if (dgsavelist.SelectedItem == null || dgsavelist.SelectedItem is not SaveList savelist) return;
            try
            {
                var v = CS.SavesClient.RemoveGameSave(savelist.id);
            }
            catch (Exception ex)
            {
                MessageBoxX.Show(ex.Message, "操作失败,请检查存档服务器设置".Translate());
            }
            Rels();
        }

        private void delall_click(object sender, RoutedEventArgs e)
        {
            if (MessageBoxX.Show("是否确认删除所有存档".Translate(), "CloudSaves", MessageBoxButton.YesNo, MessageBoxIcon.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    var v = CS.SavesClient.DeleteGame("vpet");
                }
                catch
                {
                    MessageBoxX.Show("操作失败,请检查存档服务器设置".Translate());
                }
                Rels();
            }
        }

        private async void save_click(object sender, RoutedEventArgs e)
        {
            try
            {
                var res = await CS.SavesClient.AddManualSave("vpet", $"data:|lv#{CS.MW.GameSavesData.GameSave.Level}:|money#{(int)CS.MW.GameSavesData.GameSave.Money}:|hash#{CS.MW.GameSavesData.HashCheck}:|"
                         , string.IsNullOrWhiteSpace(savename.Text) ? "手动存档".Translate() : savename.Text, CS.MW.GameSavesData.ToLPS().ToString());
                if (res)
                {
                    MessageBoxX.Show("存档成功".Translate());
                }
                else
                {
                    MessageBoxX.Show("操作失败,请检查存档服务器设置".Translate(), "存档失败".Translate());
                }
            }
            catch (Exception ex)
            {
                MessageBoxX.Show(ex.Message, "操作失败,请检查存档服务器设置".Translate());
            }
            Rels();
        }

        public async void Rels()
        {
            try
            {
                SaveLists = new List<SaveList>();
                var v = await CS.SavesClient.ListGameSaves("vpet");
                foreach (var i in v)
                {
                    SaveLists.Add(new SaveList(i));
                }
                dgsavelist.ItemsSource = SaveLists;
            }
            catch { dgsavelist.ItemsSource = null; }
        }
        private void rels_click(object sender, RoutedEventArgs e)
        {
            Rels();
        }

        public List<SaveList> SaveLists { get; set; }

        public class SaveList
        {
            public SaveList()
            {
            }
            public SaveList(GameSaveData data)
            {
                id = data.SaveID;
                Name = data.SaveName;
                Time = TimeZoneInfo.ConvertTimeFromUtc(data.SaveTime, TimeZoneInfo.Local);
                IsAutoSave = data.IsAutoSave;

                Line intor = new Line(data.Introduce);

                Level = intor[(gint)"lv"];
                Money = intor[(gint)"money"];
                Hash = intor[(gbol)"hash"];

            }
            public long id { get; set; }
            public string Name { get; set; }
            public DateTime Time { get; set; }
            public int Level { get; set; }
            public int Money { get; set; }
            public bool Hash { get; set; }
            public bool IsAutoSave { get; set; }

            public bool Search(string key)
            {
                if (Name.Contains(key))
                    return true;
                if (Level.ToString().StartsWith(key))
                    return true;
                if (Money.ToString().StartsWith(key))
                    return true;
                if (Time.ToString().Contains(key))
                    return true;
                if (Hash.ToString().ToLower().StartsWith(key.ToLower()))
                    return true;
                return false;
            }
        }

        private void search_text(object sender, TextChangedEventArgs e)
        {
            dgsavelist.ItemsSource = SaveLists?.FindAll(x => x.Search(search.Text)).ToList();
        }
    }
}
