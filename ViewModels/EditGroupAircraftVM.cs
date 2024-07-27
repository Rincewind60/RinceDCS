// Copyright 2023-2024 Paul Scobell. Subject to the GPL-3.0 license.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using RinceDCS.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RinceDCS.ViewModels;

public class EditGroupAircraftData
{
    public RinceDCSGroup Group { get; set; }
    public RinceDCSGroupAircraft Aircraft { get; set; }
}

public partial class EditGroupAircraftVM : ObservableRecipient,
                                           IRecipient<PropertyChangedMessage<string>>
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

    public EditGroupAircraftVM(List<string> aircraftNames, Dictionary<string, RinceDCSGroup> groups)
    {
        AircraftNames = aircraftNames;
        AircraftNames.Sort((x, y) =>
        {
            return x.CompareTo(y);
        });
        Groups = groups;
    }

    public void Receive(PropertyChangedMessage<string> message)
    {
        if (message.Sender is FilterToolbarVM)
        {
            if (message.PropertyName == "SelectedAircraft")
            {

            }
            else if (message.PropertyName == "SelectedCategory")
            {

            }
            else if (message.PropertyName == "SelectedGroup")
            {

            }
        }
    }

    public void CurrentAircraftChanged()
    {
        AircraftData = null;

        if (!IsAircraftSelected) return;

        AircraftData = new();

        var query = from grp in Groups.Values
                    from aircraft in grp.Aircraft
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
