using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging.Messages;
using CommunityToolkit.Mvvm.Messaging;
using RinceDCS.Models;
using SharpDX.DirectInput;
using System.Linq;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Collections;
using Microsoft.UI.Xaml.Data;

namespace RinceDCS.ViewModels;

public class ViewJoystickButton
{
    public string CommandName { get; set; }

    public GameJoystickButton GameJoystickButton { get; set; }
}

public partial class ViewJoystickViewModel : ObservableRecipient,
                                             IRecipient<PropertyChangedMessage<Game>>,
                                             IRecipient<PropertyChangedMessage<GameInstance>>,
                                             IRecipient<PropertyChangedMessage<GameAircraft>>,
                                             IRecipient<PropertyChangedMessage<DCSData>>
{
    public ObservableCollection<ViewJoystickButton> ViewButtons { get; set; } = new();

    [ObservableProperty]
    private AttachedJoystick attachedStick;

    [ObservableProperty]
    private GameJoystick stick;

    private DCSData BindingsData { get; set; }
    private DCSAircraftKey CurrentAircraftKey { get; set; }

    [ObservableProperty]
    private string currentScale;

    public string[] Scales = { "400%", "200%", "100%", "75%", "50%", "25%" };

    public ViewJoystickViewModel(AttachedJoystick attachedStick)
    {
        IsActive = true;

        AttachedStick = attachedStick;
        CurrentScale = Scales[2];
        ReBuildViewButtons();
    }

    public void Receive(PropertyChangedMessage<Game> message)
    {
        Game game = message.NewValue as Game;
        if(game != null)
        {
            Stick = (from joystick in game.Joysticks
                     where joystick.AttachedJoystick == AttachedStick
                     select joystick).First();
        }
        CurrentAircraftKey = null;
        BindingsData = null;
        ReBuildViewButtons();
    }

    public void Receive(PropertyChangedMessage<GameInstance> message)
    {
        CurrentAircraftKey = null;
        BindingsData = null;
        ReBuildViewButtons();
    }

    public void Receive(PropertyChangedMessage<GameAircraft> message)
    {
        CurrentAircraftKey = message.NewValue == null ? null : new(message.NewValue.Name);
        ReBuildViewButtons();
    }

    public void Receive(PropertyChangedMessage<DCSData> message)
    {
        BindingsData = message.NewValue;
        ReBuildViewButtons();
    }

    private void ReBuildViewButtons()
    {
        ViewButtons.Clear();

        if (CurrentAircraftKey == null) return;

        foreach (KeyValuePair<DCSBindingKey, DCSBinding> binding in BindingsData.Bindings)
        {
            if (binding.Value.AircraftWithBinding.ContainsKey(CurrentAircraftKey))
            {
                BuildAircraftButtons(binding.Value);
            }
        }
    }

    private void BuildAircraftButtons(DCSBinding binding)
    {
        string commandName = binding.AircraftWithBinding[CurrentAircraftKey].CommandName;
        ViewJoystickButton aircraftButton = new()
        {
            CommandName = commandName,
            GameJoystickButton = FindGameJoystickButton(commandName)
        };

        ViewButtons.Add(aircraftButton);
    }

    private GameJoystickButton FindGameJoystickButton(string commandName)
    {
        return null;

        //var query = from button in Stick.Buttons
        //            where button.CommandName == commandName
        //            select button;
    }
}
