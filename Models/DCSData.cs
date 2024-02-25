using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace RinceDCS.Models;



public record DCSAircraftKey(string Name);
public record DCSJoystickKey(Guid Id);
public record DCSActionKey(string Id);
public record DCSAircraftJoystickKey(string AircraftName, Guid JoystickID);

public class DCSData
{
    public Dictionary<DCSAircraftKey, DCSAircraft> Aircraft { get; set; } = new();
    public Dictionary<DCSJoystickKey, DCSJoystick> Joysticks { get; set; } = new();
    public Dictionary<DCSActionKey, DCSAction> Actions { get; set; } = new();
    public Dictionary<string, DCSModifier> Modifiers { get; set; } = new();
}

public class DCSAircraft
{
    public DCSAircraftKey Key { get; set; }
    public Dictionary<DCSActionKey, DCSAction> Actions { get; set; } = new();
}

public class DCSJoystick
{
    public DCSJoystickKey Key { get; set; }
    public AttachedJoystick Joystick { get; set; }
}

public class DCSAction
{
    public DCSActionKey Key { get; set; }
    public string Name { get; set; }
    public bool IsAxis { get; set; }
    public Dictionary<DCSAircraftKey, DCSAircraftAction> Aircraft { get; set; } = new();
    public Dictionary<DCSJoystickKey, DCSJoystick> Joysticks { get; set; } = new();
    public Dictionary<DCSAircraftJoystickKey, DCSAircraftJoystickAction> AircraftJoysticks { get; set; } = new();
}

public class DCSAircraftAction
{
    public DCSAircraftKey Key { get; set; }
    public string Action { get; set; }
    public string Category { get; set; }
}

public class DCSAircraftJoystickAction
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
