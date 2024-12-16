// Copyright 2023-2024 Paul Scobell. Subject to the GPL-3.0 license.

using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace RinceDCS.Models;

public record AttachedJoystick(Guid JoystickGuid, string Name)
{
    public string DCSName => Name + " {" + JoystickGuid + "}";
}

public partial class RinceDCSJoystick : ObservableObject, IComparable
{
    [ObservableProperty]
    private AttachedJoystick attachedJoystick;

    [ObservableProperty]
    private ObservableCollection<RinceDCSJoystickButton> buttons;

    [ObservableProperty]
    private string buttonFont;

    [ObservableProperty]
    private int buttonFontSize;

    [ObservableProperty]
    private string buttonFontColor;

    [ObservableProperty]
    private int buttonHeight;

    [ObservableProperty]
    private int buttonWidth;

    public byte[] Image { get; set; }

    public RinceDCSJoystick()
    {
        ButtonFont = "Arial";
        ButtonFontSize = 14;
        ButtonFontColor = "#000000";
        ButtonHeight = 40;
        ButtonWidth = 120;
    }

    public int CompareTo(object obj)
    {
        return AttachedJoystick.Name.CompareTo(((RinceDCSJoystick)obj).AttachedJoystick.Name);
    }
}

public partial class RinceDCSJoystickButton : ObservableObject
{
    public string ButtonName { get; set; }

    public bool IsModifier { get; set; }

    public bool IsKeyButton { get; set; }

    [property: JsonIgnore]
    public string ButtonLabel { get { return IsModifier ? "MOD+" + ButtonName : ButtonName; } }

    [ObservableProperty]
    private int topX;

    [ObservableProperty]
    private int topY;

    [ObservableProperty]
    private bool onLayout;

    [ObservableProperty]
    private string alignment;

    /// <summary>
    /// TODO: These proeprties should be removed, need to create a ViewModel version of GameJoystickButton and put them there.
    /// </summary>
    [ObservableProperty]
    [property: JsonIgnore]
    private bool isSelected;

    [ObservableProperty]
    [property: JsonIgnore]
    private RinceDCSJoystick stick;

    public RinceDCSJoystickButton(RinceDCSJoystick stick)
    {
        Stick = stick;
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
