using System;
using System.Collections.Generic;

namespace RinceDCS.Models;

public record DCSAircraftKey(string Name);
public record DCSJoystickKey(Guid Id);
public record DCSBindingKey(string Id);
public record DCSButtonKey(string Name);

public class DCSAircraft
{
    public DCSAircraftKey Key { get; set; }
}

public class DCSJoystick
{
    public DCSJoystickKey Key { get; set; }
    public AttachedJoystick Joystick { get; set; }
}

public class DCSAircraftBinding
{
    public DCSAircraftKey Key { get; set; }
    public string CommandName { get; set; }
    public string CategoryName { get; set; }
}

public class DCSBinding
{
    public DCSBindingKey Key { get; set; }
    public string CommandName { get; set; }
    public bool IsAxisBinding { get; set; }
    public bool IsKeyBinding { get { return !IsAxisBinding; } }
    public Dictionary<DCSAircraftKey, DCSAircraftBinding> AircraftWithBinding { get; set; }
    public Dictionary<DCSJoystickKey, DCSJoystick> JoysticksWithBinding { get; set; }
    public Dictionary<Tuple<DCSAircraftKey, DCSJoystickKey>, DCSAircraftJoystickBinding> AircraftJoystickBindings { get; set; }

    public DCSBinding()
    {
        AircraftWithBinding = new();
        JoysticksWithBinding = new();
        AircraftJoystickBindings = new();
    }
}

public class DCSAircraftJoystickBinding
{
    public DCSAircraftKey AircraftKey { get; set; }
    public DCSJoystickKey JoystickKey { get; set; }
    public Dictionary<DCSButtonKey, DCSButton> AssignedButtons { get; set; }
    public List<DCSAxisButton> ChangedAxisButtons { get; set; }
    public List<DCSButton> RemovedAxisButtons { get; set; }
    public List<DCSKeyButton> AddedKeyButtons { get; set; }
    public List<DCSButton> RemovedKeyButtons { get; set; }

    public DCSAircraftJoystickBinding()
    {
        AssignedButtons = new();
        ChangedAxisButtons = new();
        RemovedAxisButtons = new();
        AddedKeyButtons = new();
        RemovedKeyButtons = new();
    }
}

public class DCSButton
{
    public DCSButtonKey Key;
}

public class DCSAxisButton : DCSButton
{
    public DCSAxisFilter Filter { get; set; }

    public DCSAxisButton()
    {
        Filter = new();
    }
}

public class DCSAxisFilter
{
    public List<double> Curvature { get; set; }
    public double Deadzone { get; set; }
    public bool HardwareDetent { get; set; }
    public double HardwareDetentAB { get; set; }
    public double HardwareDetentMax { get; set; }
    public bool Invert { get; set; }
    public double SaturationX { get; set; }
    public double SaturationY { get; set; }
    public bool Slider { get; set; }

    public DCSAxisFilter()
    {
        Curvature = new();
    }
}

public class DCSKeyButton : DCSButton
{
    public List<string> Modifiers { get; set; }

    public DCSKeyButton()
    {
        Modifiers = new();
    }
}

public class DCSData
{
    public Dictionary<DCSAircraftKey, DCSAircraft> Aircraft;
    public Dictionary<DCSJoystickKey, DCSJoystick> Joysticks;
    public Dictionary<DCSBindingKey, DCSBinding> Bindings;

    public DCSData()
    {
        Aircraft = new();
        Joysticks = new();
        Bindings = new();
    }
}