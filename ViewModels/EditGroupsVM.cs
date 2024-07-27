// Copyright 2023-2024 Paul Scobell. Subject to the GPL-3.0 license.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Microsoft.UI.Xaml;
using RinceDCS.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace RinceDCS.ViewModels;

public class EditGroupsTableData
{
    public List<string> Headings { get; set; } = new();
    public List<dynamic> Groups { get; set; } = new();
}


public partial class EditGroupsVM : ObservableRecipient,
                                    IRecipient<PropertyChangedMessage<string>>
{
    [ObservableProperty]
    private EditGroupsTableData groupsData;

    private RinceDCSGroups Groups;
    private List<AttachedJoystick> Sticks;

    public EditGroupsVM(RinceDCSGroups groups, List<AttachedJoystick> sticks)
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
        GroupsData = null;

        EditGroupsTableData newGroupsData = new();

        AttachedJoystick[] sticks = new AttachedJoystick[Sticks.Count]; ;
        Sticks.CopyTo(sticks, 0);
        sticks = sticks.OrderBy(x => x.Name).ToArray();

        for (int i = 0; i < sticks.Count(); i++)
        {
            newGroupsData.Headings.Add(sticks[i].Name);
        }

        List<RinceDCSGroup> groupsToDisplay = new(Groups.Groups);

        if (FilterToolbarVM.Default.SelectedAircraft != "All")
        {
            groupsToDisplay = (from grp in groupsToDisplay where grp.AircraftNames.Contains(FilterToolbarVM.Default.SelectedAircraft) select grp).OrderBy(row => row.Name).ToList();
        }
        if (FilterToolbarVM.Default.SelectedCategory != "All")
        {
            groupsToDisplay = (from grp in groupsToDisplay where grp.Category == FilterToolbarVM.Default.SelectedCategory select grp).OrderBy(row => row.Name).ToList();
        }
        if (FilterToolbarVM.Default.SelectedGroup != "All")
        {
            groupsToDisplay = (from grp in groupsToDisplay where grp.Name == FilterToolbarVM.Default.SelectedGroup select grp).OrderBy(row => row.Name).ToList();
        }

        foreach (RinceDCSGroup grp in groupsToDisplay)
        {
            dynamic dynGroup = new ExpandoObject();
            dynGroup.Name = grp.Name;
            IDictionary<String, Object> dynGroupMembers = (IDictionary<String, Object>)dynGroup;
            for (int j = 0; j < sticks.Count(); j++)
            {
                string bindingName = "Stick" + j.ToString();

                RinceDCSGroupJoystick grpStick = grp.Joysticks.Find(row => row.Joystick == sticks[j]);
                if (grpStick.Buttons.Count > 0)
                {
                    dynGroupMembers.TryAdd(bindingName + "Buttons", grpStick.GetButtonsLabel());
                    dynGroupMembers.TryAdd(bindingName + "Visible", Visibility.Visible);
                }
                else
                {
                    dynGroupMembers.TryAdd(bindingName + "Buttons", null);
                    dynGroupMembers.TryAdd(bindingName + "Visible", Visibility.Collapsed);
                }
            }
            newGroupsData.Groups.Add(dynGroup);
        }

        GroupsData = newGroupsData;
    }
}
