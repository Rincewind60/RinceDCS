using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.Collections.ObjectModel;
using System.Linq;
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

    [ObservableProperty]
    private string fontColor;

    [ObservableProperty]
    private int defaultLabelHeight;

    [ObservableProperty]
    private int defaultLabelWidth;

    public byte[] Image {  get; set; }

    public GameJoystick()
    {
        Font = "Arial";
        FontSize = 14;
        FontColor = "#000000";
        DefaultLabelHeight = 40;
        DefaultLabelWidth = 120;
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
    private int topX;

    [ObservableProperty]
    private int topY;

    [ObservableProperty]
    private int width;

    [ObservableProperty]
    private int height;

    [ObservableProperty]
    private bool onLayout;

    [property: JsonIgnore]
    public string Font { get { return Stick.Font; } }

    [property: JsonIgnore]
    public int FontSize { get { return Stick.FontSize; } }

    [property: JsonIgnore]
    public GameJoystick Stick { get; set; }

    public GameJoystickButton(GameJoystick stick)
    {
        Stick = stick;
    }
}

public record GameAssignedButtonKey(string ButtonName, bool IsModifier);

public class GameAssignedButton
{
    public string CommandName { get; set; }
    public string CategoryName { get; set; }
    public GameJoystickButton BoundButton { get; set; }
    public string Font { get { return Stick.Font; } }
    public int FontSize { get { return Stick.FontSize; } }
    private GameJoystick Stick { get; set; }

    public GameAssignedButton(string commandName, string categoryName, GameJoystickButton boundButton, GameJoystick stick)
    {
        CommandName = commandName;
        CategoryName = categoryName;
        BoundButton = boundButton;
        Stick = stick;
    }
}
