// Copyright 2023-2024 Paul Scobell. Subject to the GPL-3.0 license.

using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace RinceDCS.Models;

public partial class RinceDCSAircraft : ObservableObject, IComparable<RinceDCSAircraft>, IEquatable<RinceDCSAircraft>
{
    [ObservableProperty]
    private string name;

    public RinceDCSAircraft(string name)
    {
        Name = name;
    }

    public int CompareTo(RinceDCSAircraft other)
    {
        return Name.CompareTo(other.Name);
    }

    public bool Equals(RinceDCSAircraft other)
    {
        return Name == other.Name;
    }
}
