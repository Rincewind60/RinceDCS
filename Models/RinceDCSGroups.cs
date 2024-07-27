// Copyright 2023-2024 Paul Scobell. Subject to the GPL-3.0 license.

using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RinceDCS.Models;

public class RinceDCSGroups
{
    public List<RinceDCSGroup> Groups { get; set; } = new();

    public List<RinceDCSGroupModifier> Modifiers { get; set; } = new();

    public string DefaultModifierName { get; set; }

    [property: JsonIgnore]
    public Dictionary<string, RinceDCSGroup> AllGroups { get; set; } = new();

    [property: JsonIgnore]
    public HashSet<string> AllActions { get; set; } = new();

    [property: JsonIgnore]
    public HashSet<string> AllAircraftNames { get; set; } = new();
}

public class RinceDCSGroup
{
    public string Name { get; set; }
    public string Category { get; set; }
    public bool IsAxis { get; set; }
    public List<string> AircraftNames { get; set; } = new();
    public List<RinceDCSGroupAction> Actions { get; set; } = new();
    public List<RinceDCSGroupJoystick> Joysticks { get; set; } = new();
    public List<RinceDCSGroupAircraft> Aircraft { get; set; } = new();
}

public class RinceDCSGroupModifier
{
    public string Name { get; set; }
    public string Device { get; set; }
    public string Key { get; set; }
    public bool Switch { get; set; }
    public bool IsCoreGame { get; set; }
}

public class RinceDCSGroupAction
{
    public string Id { get; set; }
    public string Action { get; set; }
}

public class RinceDCSGroupJoystick
{
    public AttachedJoystick Joystick { get; set; }
    public List<RinceDCSGroupButton> Buttons { get; set; } = new();

    public string GetButtonsLabel()
    {
        string label = "";

        foreach (var button in Buttons)
        {
            label += button.Name + " ";
        }

        return label;
    }
}

public class RinceDCSGroupButton
{
    public string Name { get; set; }
    public List<string> Modifiers { get; set; } = new();
    public AxisFilter AxisFilter { get; set; }
    public bool IsModifier { get { return Modifiers.Count > 0; } }
}

public partial class RinceDCSGroupAircraft : ObservableObject, IEquatable<RinceDCSGroupAircraft>
{
    public string ActionId { get; set; }
    public string AircraftName { get; set; }

    [ObservableProperty]
    private bool isActive;
    public string Action { get; set; }
    public string Category { get; set; }

    public RinceDCSGroupAircraft()
    {
    }

    public RinceDCSGroupAircraft(string name)
    {
        AircraftName = name;
    }

    public bool Equals(RinceDCSGroupAircraft other)
    {
        return AircraftName == other.AircraftName;
    }
}
