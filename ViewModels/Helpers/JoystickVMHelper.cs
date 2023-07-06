using Microsoft.UI.Xaml.Data;
using RinceDCS.Models;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RinceDCS.ViewModels.Helpers;

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

    public List<GameAssignedButton> GetAssignedButtons(GameJoystick stick, Dictionary<GameAssignedButtonKey, GameJoystickButton> buttonsOnLayout, string instanceName, string aircraftName)
    {
        List<GameAssignedButton> assignedButtons = new();

        if (string.IsNullOrWhiteSpace(aircraftName)) return null;

        DCSAircraftKey aircraftKey = new(aircraftName);
        DCSAircraft dcsAircraft = Data.Aircraft[aircraftKey];

        BuildAppButtons(stick, assignedButtons, buttonsOnLayout, "Game", instanceName);
        BuildAppButtons(stick, assignedButtons, buttonsOnLayout, "Plane", aircraftName);
        BuildAppButtons(stick, assignedButtons, buttonsOnLayout, "Joystick", stick.AttachedJoystick.Name);

        foreach (DCSBinding binding in dcsAircraft.Bindings.Values)
        {
            DCSAircraftBinding aircraftBinding = binding.AircraftWithBinding[aircraftKey];
            string commandName = aircraftBinding.CommandName;
            string categoryName = aircraftBinding.CategoryName;

            DCSAircraftJoystickKey key = new(aircraftKey.Name, stick.AttachedJoystick.JoystickGuid);

            if (binding.AircraftJoystickBindings.ContainsKey(key))
            {
                DCSAircraftJoystickBinding bindingButtons = binding.AircraftJoystickBindings[key];

                BuildAssignedButtons(stick, assignedButtons, buttonsOnLayout, binding.Key.Id, commandName, categoryName, bindingButtons);
            }
        }

        return assignedButtons;
    }

    private void BuildAppButtons(
        GameJoystick stick,
        List<GameAssignedButton> assignedButtons,
        Dictionary< GameAssignedButtonKey, GameJoystickButton > buttonsOnLayout,
        string buttonName, 
        string commandName
        )
    {
        GameAssignedButtonKey key = new(buttonName, false);
        if (buttonsOnLayout.ContainsKey(key))
        {
            GameAssignedButton vwButton = new(buttonsOnLayout[key]);
            vwButton.Commands.Add(new("", commandName, ""));
            vwButton.DetailsMarkDown = commandName;
            assignedButtons.Add(vwButton);
        }
    }

    private void BuildAssignedButtons(
        GameJoystick stick,
        List<GameAssignedButton> assignedButtons,
        Dictionary<GameAssignedButtonKey, GameJoystickButton> buttonsOnLayout,
        string bindingId,
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
                GameAssignedButton vwButton = GetAssignedButtons(assignedButtons, buttonsOnLayout[key]);
                vwButton.Commands.Add(new(bindingId, commandName, categoryName));
                vwButton.DetailsMarkDown += GetButtonDetailsMarkDown(bindingId, commandName, categoryName, button);
            }
        }
    }

    private string GetButtonDetailsMarkDown(string bindingId, string commandName, string categoryName, DCSButton button)
    {
        StringBuilder markDown = new();

        markDown.AppendLine("**Command:** " + commandName);
        markDown.AppendLine();
        markDown.AppendLine("**Category:** " + categoryName);
        markDown.AppendLine();
        markDown.AppendLine("**ID:** " + bindingId);
        markDown.AppendLine();
        if (button is DCSAxisButton)
        {
            DCSAxisButton axisButton = button as DCSAxisButton;
            markDown.AppendLine("**Filter**");
            markDown.AppendLine();
            markDown.Append("- Curvature: ");
            markDown.AppendJoin(',', axisButton.Filter.Curvature);
            markDown.AppendLine();
            markDown.AppendLine("- Deadzone: " + axisButton.Filter.Deadzone);
            markDown.AppendLine();
            markDown.Append("- Hardware Detent: ");
            markDown.AppendJoin(',', axisButton.Filter.Curvature);
            markDown.AppendLine();
            markDown.Append("- Hardware Detent AB: ");
            markDown.AppendJoin(',', axisButton.Filter.Curvature);
            markDown.AppendLine();
            markDown.Append("- Hardware Detent Max: ");
            markDown.AppendJoin(',', axisButton.Filter.Curvature);
            markDown.AppendLine();
            markDown.AppendLine("- Invert: " + axisButton.Filter.Invert);
            markDown.AppendLine();
            markDown.AppendLine("- Saturation X: " + axisButton.Filter.SaturationX);
            markDown.AppendLine();
            markDown.AppendLine("- Saturation Y: " + axisButton.Filter.SaturationY);
            markDown.AppendLine();
            markDown.Append("- Slider: ");
            markDown.AppendJoin(',', axisButton.Filter.Curvature);
            markDown.AppendLine();
        }
        else
        {
            DCSKeyButton keyButton = button as DCSKeyButton;
            markDown.Append("**Modifiers:** ");
            markDown.AppendJoin(',', keyButton.Modifiers);
            markDown.AppendLine();
        }
        markDown.AppendLine();

        return markDown.ToString();
    }

    private GameAssignedButton GetAssignedButtons(List<GameAssignedButton> assignedButtons, GameJoystickButton gameJoystickButton)
    {
        foreach(GameAssignedButton button in assignedButtons)
        {
            if(button.JoystickButton.ButtonName == gameJoystickButton.ButtonName &&
                button.JoystickButton.IsModifier == gameJoystickButton.IsModifier) {
                return button;
            }
        }

        GameAssignedButton assigned = new(gameJoystickButton);
        assignedButtons.Add(assigned); 
        return assigned;
    }
}
