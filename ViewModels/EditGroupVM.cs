// Copyright 2023-2024 Paul Scobell. Subject to the GPL-3.0 license.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using RinceDCS.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RinceDCS.ViewModels;

public partial class EditGroupVM : ObservableRecipient,
                                   IRecipient<PropertyChangedMessage<string>>
{
    [ObservableProperty]
    private RinceDCSGroup currentGroup;
    public ObservableCollection<RinceDCSGroupAircraft> Aircraft = [];

    private RinceDCSGroups Groups;
    private List<AttachedJoystick> Sticks;

    public EditGroupVM(RinceDCSGroups groups, List<AttachedJoystick> sticks)
    {
        Groups = groups;
        Sticks = sticks;

        ReBuildData();
    }

    public void Receive(PropertyChangedMessage<string> message)
    {
        if (message.Sender is FilterToolbarVM)
        {
            if (message.PropertyName == "SelectedAircraft" || message.PropertyName == "SelectedCategory" || message.PropertyName == "SelectedGroup")
            {
                ReBuildData();
            }
        }
    }

    public void ReBuildData()
    {
        Aircraft.Clear();
        CurrentGroup = null;

        if (FilterToolbarVM.Default.SelectedGroup == null || FilterToolbarVM.Default.SelectedGroup == "All")
        {
            return;
        }

        CurrentGroup = Groups.AllGroups[FilterToolbarVM.Default.SelectedGroup];

        List<RinceDCSGroupAircraft> aircraftToDisplay = new(CurrentGroup.Aircraft);

        if (FilterToolbarVM.Default.SelectedAircraft != "All")
        {
            aircraftToDisplay = (from aircraft in aircraftToDisplay where aircraft.AircraftName == FilterToolbarVM.Default.SelectedAircraft select aircraft).ToList();
        }
        if (FilterToolbarVM.Default.SelectedCategory != "All")
        {
            aircraftToDisplay = (from aircraft in aircraftToDisplay where aircraft.Category == FilterToolbarVM.Default.SelectedCategory select aircraft).ToList();
        }

        foreach (RinceDCSGroupAircraft aircraft in aircraftToDisplay)
        {
            Aircraft.Add(aircraft);
        }
    }
}
