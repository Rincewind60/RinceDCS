using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using RinceDCS.Models;
using RinceDCS.Services;
using RinceDCS.ViewModels.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RinceDCS.ViewModels;

public partial class ViewJoystickViewModel : ObservableRecipient,
                                             IRecipient<PropertyChangedMessage<RinceDCSAircraft>>
{
    [ObservableProperty]
    public ObservableCollection<AssignedButton> assignedButtons;

    [ObservableProperty]
    private RinceDCSJoystick stick;

    [ObservableProperty]
    private AttachedJoystick attachedStick;
    public DCSData BindingsData { get; set; }
    public string CurrentAircraftName { get; set; }

    public string SavedGamesFolder { get; set; }

    public string InstanceName { get; set; }

    public ScaleVMHelper ScaleHelper { get { return ScaleVMHelper.Default; } }

    public ViewJoystickViewModel(string instanceName, string savedGamesFolder, RinceDCSJoystick stick, DCSData data, RinceDCSAircraft currentAircraft)
    {
        Stick = stick;
        AttachedStick = Stick.AttachedJoystick;
        BindingsData = data;
        CurrentAircraftName = currentAircraft == null ? string.Empty : currentAircraft.Name;
        InstanceName = instanceName;
        SavedGamesFolder = savedGamesFolder;

        ReBuildViewButtons();
    }

    public void Receive(PropertyChangedMessage<RinceDCSAircraft> message)
    {
        CurrentAircraftName = message.NewValue == null ? string.Empty : new(message.NewValue.Name);
        ReBuildViewButtons();
    }

    private void ReBuildViewButtons()
    {
        JoystickVMHelper helper = new(BindingsData);
        Dictionary<AssignedButtonKey, RinceDCSJoystickButton>  buttonsOnLayout = helper.GetJoystickButtonsOnLayout(Stick);
        List<AssignedButton> buttons = helper.GetAssignedButtons(Stick, buttonsOnLayout, InstanceName, CurrentAircraftName);
        AssignedButtons = buttons == null ? new() : new(buttons);
    }
}
