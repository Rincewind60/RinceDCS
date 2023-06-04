using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

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
    private ObservableCollection<GameJoystickButton> buttons;

    [ObservableProperty]
    private string font;

    [ObservableProperty]
    private int fontSize;

    public byte[] Image {  get; set; }

    public GameJoystick()
    {
        Font = "Arial";
        FontSize = 11;
    }
}

public partial class GameJoystickButton : ObservableObject
{
    public string ButtonName { get; set; }

    public bool IsModifier { get; set; }

    public bool IsKeyButton { get; set; }

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

