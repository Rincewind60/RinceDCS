using RinceDCS.Models;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RinceDCS.ViewModels.Helper;

public class JoystickVMHelper
{
    private DCSData Data { get; set; }

    public JoystickVMHelper(DCSData data)
    {
        Data = data;
    }

    public Dictionary<GameAssignedButtonKey, GameJoystickButton> GetJoystickButtonsOnLayout(GameJoystick stick)
    {
        Dictionary<GameAssignedButtonKey, GameJoystickButton> buttons = new();
        foreach (GameJoystickButton button in stick.Buttons)
        {
            if (button.OnLayout)
            {
                GameAssignedButtonKey key = new(button.ButtonName, button.IsModifier);
                buttons[key] = button;
            }
        }
        return buttons;
    }

    public List<GameAssignedButton> GetAssignedButtons(Dictionary<GameAssignedButtonKey, GameJoystickButton> buttonsOnLayout, string instanceName, string aircraftName, AttachedJoystick stick)
    {
        List<GameAssignedButton> assignedButtons = new();

        if (string.IsNullOrWhiteSpace(aircraftName)) return null;

        DCSAircraftKey aircraftKey = new(aircraftName);
        DCSAircraft dcsAircraft = Data.Aircraft[aircraftKey];

        BuildAppButtons(assignedButtons, buttonsOnLayout, instanceName, aircraftName, stick.Name);

        foreach (DCSBinding binding in dcsAircraft.Bindings.Values)
        {
            DCSAircraftBinding aircraftBinding = binding.AircraftWithBinding[aircraftKey];
            string commandName = aircraftBinding.CommandName;
            string categoryName = aircraftBinding.CategoryName;

            DCSAircraftJoystickKey key = new DCSAircraftJoystickKey(aircraftKey.Name, stick.JoystickGuid);

            if (binding.AircraftJoystickBindings.ContainsKey(key))
            {
                DCSAircraftJoystickBinding bindingButtons = binding.AircraftJoystickBindings[key];

                BuildAssignedButtons(assignedButtons, buttonsOnLayout, commandName, categoryName, bindingButtons);
            }
        }

        return assignedButtons;
    }

    private void BuildAppButtons(
        List<GameAssignedButton> assignedButtons,
        Dictionary< GameAssignedButtonKey, GameJoystickButton > buttonsOnLayout,
        string instanceName, 
        string aircraftName,
        string stickName
        )
    {
        GameAssignedButtonKey key = new("Game", false);
        if (buttonsOnLayout.ContainsKey(key))
        {
            GameAssignedButton button = new()
            {
                CommandName = instanceName,
                BoundButton = buttonsOnLayout[key]
            };
            assignedButtons.Add(button);
        }
        key = new("Plane", false);
        if (buttonsOnLayout.ContainsKey(key))
        {
            GameAssignedButton button = new()
            {
                CommandName = aircraftName,
                BoundButton = buttonsOnLayout[key]
            };
            assignedButtons.Add(button);
        }
        key = new("Joystick", false);
        if (buttonsOnLayout.ContainsKey(key))
        {
            GameAssignedButton button = new()
            {
                CommandName = stickName,
                BoundButton = buttonsOnLayout[key]
            };
            assignedButtons.Add(button);
        }
    }

    private void BuildAssignedButtons(
        List<GameAssignedButton> assignedButtons,
        Dictionary<GameAssignedButtonKey, GameJoystickButton> buttonsOnLayout,
        string commandName,
        string categoryName,
        DCSAircraftJoystickBinding bindingButtons)
    {
        foreach (DCSButton button in bindingButtons.AssignedButtons.Values)
        {
            GameAssignedButtonKey key;
            if (button is DCSAxisButton)
            {
                key = new(button.Key.Name, false);
            }
            else
            {
                bool isModifer = ((DCSKeyButton)button).Modifiers.Count > 0;
                key = new(button.Key.Name, isModifer);
            }
            if (buttonsOnLayout.ContainsKey(key))
            {
                GameAssignedButton vwButton = new()
                {
                    CommandName = commandName,
                    CategoryName = categoryName,
                    BoundButton = buttonsOnLayout[key]
                };
                assignedButtons.Add(vwButton);
            }
        }
    }
}
