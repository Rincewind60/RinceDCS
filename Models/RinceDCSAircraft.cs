using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Xml.Linq;

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
