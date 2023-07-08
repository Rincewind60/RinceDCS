using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using RinceDCS.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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
    private string alignment;

    /// <summary>
    /// TODO: These proeprties should be removed, need to create a ViewModel version of GameJoystickButton and put them there.
    /// </summary>
    [ObservableProperty]
    [property: JsonIgnore]
    private string font;

    [ObservableProperty]
    [property: JsonIgnore]
    private int fontSize;

    [ObservableProperty]
    [property: JsonIgnore]
    private bool isSelected;

    public GameJoystickButton()
    {
        Alignment = "Left";
    }

    partial void OnAlignmentChanged(string oldValue, string newValue)
    {
        if (newValue == null)
        {
#pragma warning disable MVVMTK0034
            alignment = oldValue;
#pragma warning restore MVVMTK0034
        }
    }

}

public record GameAssignedButtonKey(string ButtonName, bool IsModifier);

public class GameAssignedCommand
{
    public string BindID { get; }
    public string CommandName { get; }
    public string CategoryName { get; }

    public GameAssignedCommand(string bindID, string commandName, string categoryName)
    {
        BindID = bindID;
        CommandName = commandName;
        CategoryName = categoryName;
    }
}

public class GameAssignedButton
{
    public GameJoystickButton JoystickButton { get; }

    public List<GameAssignedCommand> Commands { get; } = new();

    // DCS Button Details
    public bool IsAxisButton { get; set; }
    public List<int> Curvature { get; } = new();
    public bool HasUserCurve {  get { return Curvature.Count > 1; } }
    public int Deadzone { get; set; }
    public bool HardwareDetent { get; set; }
    public int HardwareDetentAB { get; set; }
    public int HardwareDetentMax { get; set; }
    public bool Invert { get; set; }
    public int SaturationX { get; set; }
    public int SaturationY { get; set; }
    public bool Slider { get; set; }
    public List<string> Modifiers { get; } = new();

    public string Command
    {
        get
        {
            string command = Commands[0].CommandName; 
            for(int i = 1; i < Commands.Count; i++)
            {
                command += "|" + Commands[i].CommandName;
            }
            return command;
        } 
    }

    public string Category
    {
        get
        {
            string category = Commands[0].CategoryName;
            for (int i = 1; i < Commands.Count; i++)
            {
                category += "|" + Commands[i].CategoryName;
            }
            return category;
        }
    }

    public string ID
    {
        get
        {
            string id = Commands[0].BindID;
            for (int i = 1; i < Commands.Count; i++)
            {
                id += "|" + Commands[i].BindID;
            }
            return id;
        }
    }

    public string Modifier
    {
        get
        {
            string modifier = Modifiers.Count > 0 ? Modifiers[0] : "";
            for (int i = 1; i < Modifiers.Count; i++)
            {
                modifier += "," + Modifiers[i];
            }
            return modifier;
        }
    }

    public bool IsValid { get { return Commands.Count == 1; } }

    public GameAssignedButton(GameJoystickButton joyButton)
    {
        JoystickButton = joyButton;
    }
}
