using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging.Messages;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using RinceDCS.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RinceDCS.ViewModels.Messages;
using System.Collections.ObjectModel;

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
