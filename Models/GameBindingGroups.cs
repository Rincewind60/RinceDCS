using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Windows.Networking.Vpn;

namespace RinceDCS.Models;

public class GameBindingGroups
{
    public List<GameBindingGroup> Groups { get; set; } = new();

    [property: JsonIgnore]
    public Dictionary<string, GameBindingGroup> AllGroups { get; set; } = new();

    [property: JsonIgnore] 
    public Dictionary<string, GameBinding> AllBindings { get; set; } = new();
}

public class GameBindingGroup
{
    public string Name { get; set; }
    public bool IsAxisBinding { get; set; }
    public bool IsKeyBinding { get { return !IsAxisBinding; } }
    public List<GameBinding> Bindings { get; set; } = new();
    public List<GameBindingJoystick> Joysticks { get; set; } = new();
    public List<GameAircraft> Aircraft { get; set; } = new();
    public List<GameBoundAircraft> BoundAircraft { get; set; } = new();
}

public class GameBinding
{
    public string Id { get; set; }
    public string CommandName { get; set; }
}

public class GameBindingJoystick
{
    public AttachedJoystick Joystick { get; set; }
    public List<GameAssignedButton> Buttons { get; set; }= new();
}

public class GameBoundAircraft
{
    public string BindingId { get; set; }
    public string AircraftName { get; set; }
    public bool IsActive { get; set; }
    public string CommandName { get; set; }
    public string CategoryName { get; set; }
}
