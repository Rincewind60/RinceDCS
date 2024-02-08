using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace RinceDCS.Models;

public record DCSAircraftKey(string Name);
public record DCSJoystickKey(Guid Id);
public record DCSBindingKey(string Id);
public record DCSAircraftJoystickKey(string AircraftName, Guid JoystickID);

public class DCSData
{
    public Dictionary<DCSAircraftKey, DCSAircraft> Aircraft { get; set; } = new();
    public Dictionary<DCSJoystickKey, DCSJoystick> Joysticks { get; set; } = new();
    public Dictionary<DCSBindingKey, DCSBinding> Bindings { get; set; } = new();
}

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

public class DCSBinding
{
    public DCSBindingKey Key { get; set; }
    public string Command { get; set; }
    public bool IsAxisBinding { get; set; }
    public bool IsKeyBinding { get { return !IsAxisBinding; } }
    public Dictionary<DCSAircraftKey, DCSAircraftBinding> AircraftWithBinding { get; set; } = new();
    public Dictionary<DCSJoystickKey, DCSJoystick> JoysticksWithBinding { get; set; } = new();
    public Dictionary<DCSAircraftJoystickKey, DCSAircraftJoystickBinding> AircraftJoystickBindings { get; set; } = new();
}

public class DCSAircraftBinding
{
    public DCSAircraftKey Key { get; set; }
    public string Command { get; set; }
    public string Category { get; set; }
}

public class DCSAircraftJoystickBinding
{
    public DCSAircraftKey AircraftKey { get; set; }
    public DCSJoystickKey JoystickKey { get; set; }
    public Dictionary<string, DCSButton> AssignedButtons { get; set; } = new();
    public DCSButtonChanges SavedGamesButtonChanges { get; set; } = new();
}

public class DCSButtonChanges
{
    public List<DCSButton> ChangedAxisButtons { get; set; } = new();
    public List<DCSButton> RemovedAxisButtons { get; set; } = new();
    public List<DCSButton> AddedAxisButtons { get; set; } = new();
    public List<DCSButton> AddedKeyButtons { get; set; } = new();
    public List<DCSButton> RemovedKeyButtons { get; set; } = new();
}

public class DCSButton
{
    public string Name { get; set; }
    public List<string> Modifiers { get; set; } = new();
    public AxisFilter AxisFilter { get; set; }
}
