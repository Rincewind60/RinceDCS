using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using RinceDCS.Models;
using RinceDCS.ServiceModels;
using RinceDCS.ViewModels.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RinceDCS.ViewModels;

public partial class ViewJoystickViewModel : ObservableRecipient,
                                             IRecipient<PropertyChangedMessage<GameAircraft>>
{
    [ObservableProperty]
    public ObservableCollection<GameAssignedButton> assignedButtons;

    [ObservableProperty]
    private GameJoystick stick;

    [ObservableProperty]
    private AttachedJoystick attachedStick;
    public DCSData BindingsData { get; set; }
    public DCSAircraftKey CurrentAircraftKey { get; set; }

    public ScaleVMHelper ScaleHelper { get; }

    public string SavedGamesFolder { get; set; }

    public string InstanceName { get; set; }

    public ViewJoystickViewModel(string instanceName, string savedGamesFolder, GameJoystick stick, DCSData data, GameAircraft currentAircraft)
    {
        Stick = stick;
        AttachedStick = Stick.AttachedJoystick;
        BindingsData = data;
        CurrentAircraftKey = currentAircraft == null ? null : new(currentAircraft.Name);
        ScaleHelper = Ioc.Default.GetRequiredService<ScaleVMHelper>();
        InstanceName = instanceName;
        SavedGamesFolder = savedGamesFolder;

        ReBuildViewButtons();
    }

    public void Receive(PropertyChangedMessage<GameAircraft> message)
    {
        CurrentAircraftKey = message.NewValue == null ? null : new(message.NewValue.Name);
        ReBuildViewButtons();
    }

    private void ReBuildViewButtons()
    {
        JoystickVMHelper helper = new(BindingsData);
        Dictionary<GameAssignedButtonKey, GameJoystickButton>  buttonsOnLayout = helper.GetJoystickButtonsOnLayout(Stick);
        AssignedButtons = new(helper.GetAssignedButtons(Stick, buttonsOnLayout, InstanceName, CurrentAircraftKey.Name));
    }
}
