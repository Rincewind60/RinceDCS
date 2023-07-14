using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Vpn;

namespace RinceDCS.Models;

public class GameBindingGroups
{
    public List<GameBindingGroup> Groups { get; set; } = new();
}

public class GameBindingGroup
{
    public string Name { get; set; }
    public List<GameBindingJoystick> JoystickButtons { get; set; } = new();
    public List<GameBinding> GameBindings { get; set; } = new();
}

public class GameBindingJoystick
{
    public AttachedJoystick Joystick { get; set; }
    public List<GameAssignedButton> Buttons { get; set; }= new();
}

public class GameBinding
{
    public string Id { get; set; }
    public string CommandName { get; set; }
    public List<GameBindingAircraft> BoundAircraft { get; set; } = new();
}

public class GameBindingAircraft
{
    public GameAircraft Aircraft { get; set; }
    public bool IsActive { get; set; }
    public string CommandName { get; set; }
    public string CategoryName { get; set; }
}
