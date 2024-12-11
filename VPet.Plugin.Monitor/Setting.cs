using LinePutScript;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VPet.Plugin.Monitor
{
    public class Setting : Line
    {
        public double PlaceTop
        {
            get => GetDouble("PlaceTop", 0);
            set => SetDouble("PlaceTop", value);
        }
        public double WindowOpacity
        {
            get => GetDouble("Opacity", 1);
            set => SetDouble("Opacity", value);
        }
        public int GPUSelected
        {
            get => GetInt("GPUSelected", 0);
            set => SetInt("GPUSelected", value);
        }
        public string NetSelected
        {
            get => GetString("NetSelected", string.Empty);
            set => SetString("NetSelected", value);
        }

        public bool IsGPUWoring
        {
            get => !GetBool("NoGPUWorking");
            set => SetBool("NoGPUWorking", !value);
        }
        public bool isverticalshow;
        public bool IsVerticalShow
        {
            get => isverticalshow;
            set
            {
                isverticalshow = value;
                SetBool("IsVerticalShow", value);
            }
        }
        public bool ishorizonalshow;
        public bool IsHorizonalShow
        {
            get => ishorizonalshow;
            set
            {
                ishorizonalshow = value;
                SetBool("IsHorizonalShow", value);
            }
        }
        public bool isfrond;
        public bool IsFrond
        {
            get => isfrond;
            set
            {
                isfrond = value;
                SetBool("IsForeground", value);
            }
        }
        public bool IsColorful
        {
            get => GetBool("IsColorful");
            set => SetBool("IsColorful", value);
        }
        public Setting(ILine line) : base(line)
        {
            isverticalshow = GetBool("IsVerticalShow");
            ishorizonalshow = GetBool("IsHorizonalShow");
            isfrond = GetBool("IsForeground");
        }
    }
}
