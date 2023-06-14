using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Serialization;
using Windows.Gaming.Input;

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

    partial void OnFontChanged(string value)
    {
        if (Buttons == null) return;

        foreach (GameJoystickButton button in Buttons)
        {
            button.Font = value;
        }
    }

    partial void OnFontSizeChanged(int value)
    {
        if (Buttons == null) return;

        foreach (GameJoystickButton button in Buttons)
        {
            button.FontSize = value;
        }
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

    [ObservableProperty]
    [property: JsonIgnore]
    private string font;

    [ObservableProperty]
    [property: JsonIgnore]
    private int fontSize;
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
