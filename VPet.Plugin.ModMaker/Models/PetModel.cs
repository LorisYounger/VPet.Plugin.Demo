using HKW.HKWViewModels.SimpleObservable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace VPet.Plugin.ModMaker.Models;

public class PetModel
{
    public ObservableValue<BitmapImage> Image { get; } = new();
    public ObservableValue<ObservableInt32Rect> TouchHeadRect { get; } = new(new());
    public ObservableValue<MultiStateRect> TouchRaisedRect { get; } = new(new());
    public ObservableValue<MultiStatePoint> RaisePoint { get; } = new(new());
}

public class MultiStateRect
{
    public ObservableValue<ObservableInt32Rect> Happy { get; } = new(new());
    public ObservableValue<ObservableInt32Rect> Nomal { get; } = new(new());
    public ObservableValue<ObservableInt32Rect> PoorCondition { get; } = new(new());
    public ObservableValue<ObservableInt32Rect> Ill { get; } = new(new());
}

public class MultiStatePoint
{
    public ObservableValue<ObservablePoint> Happy { get; } = new(new());
    public ObservableValue<ObservablePoint> Nomal { get; } = new(new());
    public ObservableValue<ObservablePoint> PoorCondition { get; } = new(new());
    public ObservableValue<ObservablePoint> Ill { get; } = new(new());
}

public class ObservableInt32Rect
{
    public ObservableValue<int> X { get; } = new();
    public ObservableValue<int> Y { get; } = new();
    public ObservableValue<int> Width { get; } = new();
    public ObservableValue<int> Height { get; } = new();
}

public class ObservablePoint
{
    public ObservableValue<double> X { get; } = new();
    public ObservableValue<double> Y { get; } = new();
}
