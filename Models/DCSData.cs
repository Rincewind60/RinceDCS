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
    public Dictionary<string, DCSModifier> Modifiers { get; set; } = new();
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
    public bool IsAxis { get; set; }
    public Dictionary<DCSAircraftKey, DCSAircraftBinding> Aircraft { get; set; } = new();
    public Dictionary<DCSJoystickKey, DCSJoystick> Joysticks { get; set; } = new();
    public Dictionary<DCSAircraftJoystickKey, DCSAircraftJoystickBinding> AircraftJoysticks { get; set; } = new();
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
    public Dictionary<string, DCSButton> Buttons { get; set; } = new();
    public DCSButtonChanges ButtonChanges { get; set; } = new();
}

public class DCSButton
{
    public string Name { get; set; }
    public List<string> Modifiers { get; set; } = new();
    public AxisFilter AxisFilter { get; set; }
    public bool IsModifier { get { return Modifiers.Count > 0; } }
}

public class DCSButtonChanges
{
    public List<DCSButton> Changed { get; set; } = new();
    public List<DCSButton> Added { get; set; } = new();
    public List<DCSButton> Removed { get; set; } = new();
}

public class DCSModifier
{
    public string Name { get; set; }
    public string Device { get; set; }
    public string Key { get; set; }
    public bool Switch { get; set; }
}
