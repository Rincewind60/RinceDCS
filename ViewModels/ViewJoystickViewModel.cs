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
                                             IRecipient<PropertyChangedMessage<GameAircraft>>
{
    public ObservableCollection<ViewJoystickButton> ViewButtons { get; set; } = new();

    [ObservableProperty]
    private GameJoystick stick;

    [ObservableProperty]
    private AttachedJoystick attachedStick;
    public DCSData BindingsData { get; set; }
    public DCSAircraftKey CurrentAircraftKey { get; set; }

    [ObservableProperty]
    private string currentScale;

    public string[] Scales = { "400%", "200%", "100%", "75%", "50%", "25%" };

    public string InstanceFolderName { get; set; }

    public ViewJoystickViewModel(string instanceFolderName, GameJoystick stick, DCSData data, GameAircraft currentAircraft)
    {
        Stick = stick;
        AttachedStick = Stick.AttachedJoystick;
        BindingsData = data;
        CurrentAircraftKey = currentAircraft == null ? null : new(currentAircraft.Name);
        CurrentScale = Scales[2];
        InstanceFolderName = instanceFolderName;

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

            if(binding.AircraftJoystickBindings.ContainsKey(key))
            {
                DCSAircraftJoystickBinding bindingButtons = binding.AircraftJoystickBindings[key];

                BuildAssignedButtons(aircraftButtons, commandName, categoryName, bindingButtons);
            }
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
