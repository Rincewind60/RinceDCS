using CommunityToolkit.Mvvm.ComponentModel;
using RinceDCS.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RinceDCS.Models;

public record AttachedJoystick(Guid JoystickGuid, string Name)
{
    public string DCSName => Name + " {" + JoystickGuid + "}";
}

public partial class GameJoystick : ObservableObject
{
    [ObservableProperty]
    private AttachedJoystick attachedJoystick;

    [ObservableProperty]
    private string imagePath;

    [ObservableProperty]
    private ObservableCollection<GameJoystickButton> buttons;
}

public partial class GameJoystickButton : ObservableObject
{
    public string ButtonName { get; set; }

    public bool IsModifier { get; set; }

    [property: JsonIgnore]
    public string ButtonLabel {  get {  return IsModifier ? "MOD+" + ButtonName : ButtonName; } }

    [ObservableProperty]
    private double topX;

    [ObservableProperty]
    private double topY;

    [ObservableProperty]
    private int width;

    [ObservableProperty]
    private int height;

    [ObservableProperty]
    private bool onLayout;
}

