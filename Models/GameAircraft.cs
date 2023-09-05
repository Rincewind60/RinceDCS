using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Xml.Linq;

namespace RinceDCS.Models;

public partial class GameAircraft : ObservableObject, IComparable<GameAircraft>
{
    [ObservableProperty]
    private string name;

    public GameAircraft(string name)
    {
        Name = name;
    }

    public int CompareTo(GameAircraft other)
    {
        return Name.CompareTo(other.Name);
    }
}
