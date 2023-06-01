﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace RinceDCS.Models;

public record DCSAircraftKey(string Name);
public record DCSJoystickKey(Guid Id);
public record DCSBindingKey(string Id);
public record DCSButtonKey(string Name);

public record DCSAircraftJoystickKey(string AircraftName, Guid JoystickID);

public class DCSAircraft
{
    public DCSAircraftKey Key { get; set; }

    public Dictionary<DCSBindingKey, DCSBinding> Bindings { get; set; } = new();
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
    public Dictionary<DCSAircraftKey, DCSAircraftBinding> AircraftWithBinding { get; set; } = new();
    public Dictionary<DCSJoystickKey, DCSJoystick> JoysticksWithBinding { get; set; } = new();
    public Dictionary<DCSAircraftJoystickKey, DCSAircraftJoystickBinding> AircraftJoystickBindings { get; set; } = new();
}

public class DCSAircraftJoystickBinding
{
    public DCSAircraftKey AircraftKey { get; set; }
    public DCSJoystickKey JoystickKey { get; set; }
    public Dictionary<DCSButtonKey, DCSButton> AssignedButtons { get; set; } = new();
    public List<DCSAxisButton> ChangedAxisButtons { get; set; } = new();
    public List<DCSButton> RemovedAxisButtons { get; set; } = new();
    public List<DCSKeyButton> AddedKeyButtons { get; set; } = new();
    public List<DCSButton> RemovedKeyButtons { get; set; } = new();
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
    public List<string> Modifiers { get; set; } = new();
}

public class DCSData
{
    public Dictionary<DCSAircraftKey, DCSAircraft> Aircraft { get; set; } = new();
    public Dictionary<DCSJoystickKey, DCSJoystick> Joysticks { get; set; } = new ();
    public Dictionary<DCSBindingKey, DCSBinding> Bindings { get; set; } = new();
}