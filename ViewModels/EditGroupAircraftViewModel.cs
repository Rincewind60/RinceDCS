using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using RinceDCS.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RinceDCS.ViewModels;

public class EditGroupAircraftData
{
    public RinceDCSGroup Group { get; set; }
    public RinceDCSGroupAircraft Aircraft { get; set; }
}

public partial class EditGroupAircraftViewModel : ObservableObject
{
    [ObservableProperty]
    public List<string> aircraftNames;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAircraftSelected))]
    private string currentAircraft;
    public bool IsAircraftSelected { get { return CurrentAircraft != null; } }
    [ObservableProperty]
    private ObservableCollection<EditGroupAircraftData> aircraftData;

    private Dictionary<string, RinceDCSGroup> Groups;

    public EditGroupAircraftViewModel(List<string> aircraftNames, Dictionary<string, RinceDCSGroup> groups)
    {
        AircraftNames = aircraftNames;
        AircraftNames.Sort((x, y) =>
        {
            return x.CompareTo(y);
        });
        Groups = groups;
    }

    public void CurrentAircraftChanged()
    {
        AircraftData = null;

        if (!IsAircraftSelected) return;

        AircraftData = new();

        var query = from grp in Groups.Values
                    from aircraft in grp.AircraftBindings
                    where aircraft.AircraftName == CurrentAircraft
                    orderby grp.Name
                    select Tuple.Create(grp, aircraft);
        foreach (var result in query)
        {
            EditGroupAircraftData newAircraftData = new()
            {
                Group = result.Item1,
                Aircraft = result.Item2
            };
            AircraftData.Add(newAircraftData);
        }
    }
}
