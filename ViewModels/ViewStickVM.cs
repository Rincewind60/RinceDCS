// Copyright 2023-2024 Paul Scobell. Subject to the GPL-3.0 license.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using RinceDCS.Models;
using RinceDCS.ViewModels.Helpers;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RinceDCS.ViewModels;

public partial class ViewStickVM : ObservableRecipient,
                                   IRecipient<PropertyChangedMessage<string>>
{
    [ObservableProperty]
    public ObservableCollection<AssignedButton> assignedButtons = new();

    [ObservableProperty]
    private RinceDCSJoystick stick;

    [ObservableProperty]
    private AttachedJoystick attachedStick;
    public DCSData BindingsData { get; set; }

    public string SavedGamesFolder { get; set; }

    public string InstanceName { get; set; }

    public ScaleVMHelper ScaleHelper { get { return ScaleVMHelper.Default; } }

    public ViewStickVM(string instanceName, string savedGamesFolder, RinceDCSJoystick stick, DCSData data)
    {
        Stick = stick;
        AttachedStick = Stick.AttachedJoystick;
        BindingsData = data;
        InstanceName = instanceName;
        SavedGamesFolder = savedGamesFolder;

        ReBuildViewButtons();
    }

    public void Receive(PropertyChangedMessage<string> message)
    {
        if (message.Sender is FilterToolbarVM && message.PropertyName == "SelectedAircraft")
        {
            ReBuildViewButtons();
        }
    }

    private void ReBuildViewButtons()
    {
        AssignedButtons.Clear();

        if (FilterToolbarVM.Default.SelectedAircraft != "All")
        {
            JoystickVMHelper helper = new(BindingsData);
            Dictionary<AssignedButtonKey, RinceDCSJoystickButton> buttonsOnLayout = helper.GetJoystickButtonsOnLayout(Stick);
            List<AssignedButton> buttons = helper.GetAssignedButtons(Stick, buttonsOnLayout, InstanceName, FilterToolbarVM.Default.SelectedAircraft);
            foreach (AssignedButton button in buttons)
            {
                AssignedButtons.Add(button);
            }
        }
    }
}
