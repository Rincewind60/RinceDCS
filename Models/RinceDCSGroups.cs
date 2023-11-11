using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Networking.Vpn;

namespace RinceDCS.Models;

public class RinceDCSGroups
{
    public List<RinceDCSGroup> Groups { get; set; } = new();

    [property: JsonIgnore]
    public Dictionary<string, RinceDCSGroup> AllGroups { get; set; } = new();

    [property: JsonIgnore] 
    public Dictionary<string, RinceDCSGroupBinding> AllBindings { get; set; } = new();

    [property: JsonIgnore]
    public Dictionary<string, string> AllAircraftNames { get; set; } = new();
}

public class RinceDCSGroup
{
    public string Name { get; set; }
    public bool IsAxisBinding { get; set; }
    public bool IsKeyBinding { get { return !IsAxisBinding; } }
    public List<string> AircraftNames { get; set; } = new();
    public List<RinceDCSGroupBinding> Bindings { get; set; } = new();
    public List<RinceDCSGroupJoystick> JoystickBindings { get; set; } = new();
    public List<RinceDCSGroupAircraft> AircraftBindings { get; set; } = new();
}

public class RinceDCSGroupBinding
{
    public string Id { get; set; }
    public string CommandName { get; set; }
}

public class RinceDCSGroupJoystick
{
    public AttachedJoystick Joystick { get; set; }
    public List<IRinceDCSGroupButton> Buttons { get; set; }= new();
}

public interface IRinceDCSGroupButton
{
    public string ButtonName { get; set; }
}

public class RinceDCSGroupAxisButton : IRinceDCSGroupButton
{
    public string ButtonName { get; set; }
    public List<double> Curvature { get; set; } = new();
    public double Deadzone { get; set; }
    public bool HardwareDetent { get; set; }
    public double HardwareDetentAB { get; set; }
    public double HardwareDetentMax { get; set; }
    public bool Invert { get; set; }
    public double SaturationX { get; set; }
    public double SaturationY { get; set; }
    public bool Slider { get; set; }
}

public class RinceDCSGroupKeyButton : IRinceDCSGroupButton
{
    public string ButtonName { get; set; }
    public List<string> Modifiers { get; set; } = new();
}

public partial class RinceDCSGroupAircraft : ObservableObject, IEquatable<RinceDCSGroupAircraft>
{
    public string BindingId { get; set; }
    public string AircraftName { get; set; }

    [ObservableProperty]
    private bool isActive;
    public string CommandName { get; set; }
    public string CategoryName { get; set; }

    public RinceDCSGroupAircraft()
    {
    }

    public RinceDCSGroupAircraft(string name)
    {
        AircraftName = name;
    }

    public bool Equals(RinceDCSGroupAircraft other)
    {
        return AircraftName == other.AircraftName;
    }
}
