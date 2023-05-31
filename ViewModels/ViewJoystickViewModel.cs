using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using RinceDCS.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RinceDCS.ViewModels;

using ButtonName = String;

internal record ViewButtonKey(ButtonName buttonName, bool isModifier);

public class ViewJoystickButton
{
    public string CommandName { get; set; }
    public string CategoryName { get; set; }

    public GameJoystickButton BoundButton { get; set; }
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
    [NotifyPropertyChangedRecipients]
    private GameJoystick stick;

    private DCSData BindingsData { get; set; }
    private DCSAircraftKey CurrentAircraftKey { get; set; }

    [ObservableProperty]
    private string currentScale;

    public string[] Scales = { "400%", "200%", "100%", "75%", "50%", "25%" };

    public ViewJoystickViewModel(Game game, AttachedJoystick attachedStick, DCSData data, GameAircraft currentAircraft)
    {
        AttachedStick = attachedStick;
        CurrentScale = Scales[2];

        BindingsData = data;
        CurrentAircraftKey = currentAircraft == null ? null : new(currentAircraft.Name);
        Stick = (from gameStick in game.Joysticks
                 where gameStick.AttachedJoystick == AttachedStick
                 select gameStick).First();

        ReBuildViewButtons();

        IsActive = true;
    }

    public void Receive(PropertyChangedMessage<Game> message)
    {
        Game game = message.NewValue;
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

        Dictionary<ViewButtonKey, GameJoystickButton> aircraftButtons = GetJoystickButtonsOnLayout();

        DCSAircraft dcsAircraft = BindingsData.Aircraft[CurrentAircraftKey];

        foreach(DCSBinding binding in dcsAircraft.Bindings.Values)
        {
            DCSAircraftBinding aircraftBinding = binding.AircraftWithBinding[CurrentAircraftKey];
            string commandName = aircraftBinding.CommandName;
            string categoryName = aircraftBinding.CategoryName;

            DCSAircraftJoystickKey key = new DCSAircraftJoystickKey(CurrentAircraftKey.Name, AttachedStick.JoystickGuid);
            DCSAircraftJoystickBinding bindingButtons = binding.AircraftJoystickBindings[key];

            BuildAssignedButtons(aircraftButtons, commandName, categoryName, bindingButtons);
        }
    }

    private Dictionary<ViewButtonKey, GameJoystickButton> GetJoystickButtonsOnLayout()
    {
        Dictionary<ViewButtonKey, GameJoystickButton> buttons = new();

        foreach (GameJoystickButton button in Stick.Buttons)
        {
            if (button.OnLayout)
            {
                ViewButtonKey key = new(button.ButtonName, button.IsModifier);
                buttons[key] = button;
            }
        }

        return buttons;
    }

    private void BuildAssignedButtons(
        Dictionary<ViewButtonKey, GameJoystickButton> aircraftButtons, 
        string commandName, 
        string categoryName, 
        DCSAircraftJoystickBinding bindingButtons)
    {
        foreach (DCSButton button in bindingButtons.AssignedButtons.Values)
        {
            ViewButtonKey key;
            if (button is DCSAxisButton)
            {
                key = new(button.Key.Name, false);
            }
            else
            {
                bool isModifer = ((DCSKeyButton)button).Modifiers.Count > 0;
                key = new(button.Key.Name, isModifer);
            }
            if (aircraftButtons.ContainsKey(key))
            {
                ViewJoystickButton vwButton = new()
                {
                    CommandName = commandName,
                    CategoryName = categoryName,
                    BoundButton = aircraftButtons[key]
                };
                ViewButtons.Add(vwButton);
            }
        }
    }
}
