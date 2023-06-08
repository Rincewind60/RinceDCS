using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using RinceDCS.Models;
using RinceDCS.ViewModels.Helper;
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

    [ObservableProperty]
    private int currentScale;

    public string[] Scales = { "400%", "200%", "100%", "75%", "50%", "25%" };

    public string SavedGamesFolder { get; set; }

    public string InstanceName { get; set; }

    public ViewJoystickViewModel(string instanceName, string savedGamesFolder, GameJoystick stick, DCSData data, GameAircraft currentAircraft)
    {
        Stick = stick;
        AttachedStick = Stick.AttachedJoystick;
        BindingsData = data;
        CurrentAircraftKey = currentAircraft == null ? null : new(currentAircraft.Name);
        CurrentScale = 2;
        InstanceName = instanceName;
        SavedGamesFolder = savedGamesFolder;

        ReBuildViewButtons();

        IsActive = true;
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
        AssignedButtons = new(helper.GetAssignedButtons(buttonsOnLayout, InstanceName, CurrentAircraftKey.Name, AttachedStick));
    }
}
