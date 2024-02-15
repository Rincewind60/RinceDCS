using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RinceDCS.Models;

public sealed class AxisFilter
{
    public List<double> Curvature { get; set; } = new();
    public bool HasUserCurve { get { return Curvature.Count > 1; } }
    public double Deadzone { get; set; }
    public bool HardwareDetent { get; set; }
    public double HardwareDetentAB { get; set; }
    public double HardwareDetentMax { get; set; }
    public bool Invert { get; set; }
    public double SaturationX { get; set; }
    public double SaturationY { get; set; }
    public bool Slider { get; set; }

    public AxisFilter()
    {
        //  Set to default values
        Deadzone = 0;
        HardwareDetent = false;
        HardwareDetentAB = 0;
        HardwareDetentMax = 0;
        Invert = false;
        SaturationX = 1;
        SaturationY = 1;
        Slider = false;
    }

    public AxisFilter(AxisFilter from)
    {
        //  Set to default values
        Curvature = from.Curvature.ToList();
        Deadzone = from.Deadzone;
        HardwareDetent = from.HardwareDetent;
        HardwareDetentAB = from.HardwareDetentAB;
        HardwareDetentMax = from.HardwareDetentMax;
        Invert = from.Invert;
        SaturationX = from.SaturationX;
        SaturationY = from.SaturationY;
        Slider = from.Slider;
    }

    public bool IsDefault()
    {
        return (Curvature.Count == 1 &&
                Curvature[0] == 0 &&
                Deadzone == 0 &&
                HardwareDetent == false &&
                HardwareDetentAB == 0 &&
                HardwareDetentMax == 0 &&
                Invert == false &&
                SaturationX == 1 &&
                SaturationY == 1 &&
                Slider == false);
    }
}