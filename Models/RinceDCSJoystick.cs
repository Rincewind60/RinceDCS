// Copyright 2023-2024 Paul Scobell. Subject to the GPL-3.0 license.

using CommunityToolkit.Mvvm.ComponentModel;
using RinceDCS.Properties;
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
    public partial AttachedJoystick AttachedJoystick { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<RinceDCSJoystickImage> Images { get; set; }

    [ObservableProperty]
    public partial string ButtonFont {  get; set; }

    [ObservableProperty]
    public partial int ButtonFontSize { get; set; }

    [ObservableProperty]
    public partial string ButtonFontColor { get; set; }

    [ObservableProperty]
    public partial int ButtonHeight { get; set; }

    [ObservableProperty]
    public partial int ButtonWidth { get; set; }

    public RinceDCSJoystick()
    {
        Images = [];
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

public partial class RinceDCSJoystickImage : ObservableObject
{
    [ObservableProperty]
    public partial string Title { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<RinceDCSJoystickButton> Buttons { get; set; }

    public byte[] Image { get; set; }
}

public partial class RinceDCSJoystickButton : ObservableObject
{
    public string ButtonName { get; set; }

    public bool IsModifier { get; set; }

    public bool IsKeyButton { get; set; }

    [property: JsonIgnore]
    public string ButtonLabel { get { return IsModifier ? "MOD+" + ButtonName : ButtonName; } }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LineStartX))]
    public partial int TopX { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LineStartY))] 
    public partial int TopY { get; set; }

    [ObservableProperty]
    public partial bool DrawLine { get; set; }

    [ObservableProperty]
    public partial int LineEndX { get; set; }

    [ObservableProperty]
    public partial int LineEndY { get; set; }

    public int LineStartX 
    { 
        get
        {
            return TopX + (Stick.ButtonWidth / 2);
        }
    }

    public int LineStartY
    {
        get
        {
            return TopY + (Stick.ButtonHeight / 2);
        }
    }

    [ObservableProperty]
    public partial bool OnLayout { get; set; }

    [ObservableProperty]
    public partial string Alignment { get; set; }

    /// <summary>
    /// TODO: These proeprties should be removed, need to create a ViewModel version of GameJoystickButton and put them there.
    /// </summary>
    [ObservableProperty]
    [property: JsonIgnore]
    public partial bool IsSelected { get; set; }

    [ObservableProperty]
    [property: JsonIgnore]
    public partial RinceDCSJoystick Stick { get; set; }

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
            Alignment = oldValue;
#pragma warning restore MVVMTK0034
        }
    }

}
