using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VPet_Simulator.Windows.Interface;

namespace VPet.Plugin.ModMaker;

public class ModInfo
{
    public string Name { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string GameVersion { get; set; } = string.Empty;
    public string ModVersion { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public List<Food> Foods { get; set; } = new();
    public List<ClickText> ClickTexts { get; set; } = new();
    public List<LowText> LowTexts { get; set; } = new();
}
