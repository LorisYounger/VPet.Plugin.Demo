using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace VPet.Plugin.ModMaker;

internal static class Utils
{
    public static BitmapImage LoadImageToStream(string imagePath)
    {
        BitmapImage bitmapImage = new();
        bitmapImage.BeginInit();
        bitmapImage.StreamSource = new StreamReader(imagePath).BaseStream;
        bitmapImage.EndInit();
        return bitmapImage;
    }

    public static BitmapImage LoadImageToMemoryStream(string imagePath)
    {
        BitmapImage bitmapImage = new();
        bitmapImage.BeginInit();
        var ms = new MemoryStream();
        var sr = new StreamReader(imagePath);
        sr.BaseStream.CopyTo(ms);
        sr.Close();
        bitmapImage.StreamSource = ms;
        bitmapImage.EndInit();
        return bitmapImage;
    }

    public static void ShowDialogX(this Window window, Window owner)
    {
        owner.IsEnabled = false;
        window.Owner = owner;
        window.Closed += (s, e) =>
        {
            owner.IsEnabled = true;
        };
        window.ShowDialog();
    }
}

public delegate void ShowDialogXHandler(Window window);
