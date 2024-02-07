using Microsoft.UI.Xaml.Data;
using RinceDCS.Models;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Security.Cryptography;
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

    public Dictionary<AssignedButtonKey, RinceDCSJoystickButton> GetJoystickButtonsOnLayout(RinceDCSJoystick stick)
    {
        Dictionary<AssignedButtonKey, RinceDCSJoystickButton> buttons = new();
        foreach (RinceDCSJoystickButton button in stick.Buttons)
        {
            if (button.OnLayout)
            {
                AssignedButtonKey key = new(button.ButtonName, button.IsModifier);
                buttons[key] = button;
            }
        }
        return buttons;
    }

    public List<AssignedButton> GetAssignedButtons(RinceDCSJoystick stick, Dictionary<AssignedButtonKey, RinceDCSJoystickButton> buttonsOnLayout, string instanceName, string aircraftName)
    {
        List<AssignedButton> assignedButtons = new();

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
        RinceDCSJoystick stick,
        List<AssignedButton> assignedButtons,
        Dictionary< AssignedButtonKey, RinceDCSJoystickButton > buttonsOnLayout,
        string buttonName, 
        string commandName
        )
    {
        AssignedButtonKey key = new(buttonName, false);
        if (buttonsOnLayout.ContainsKey(key))
        {
            AssignedButton vwButton = new(buttonsOnLayout[key]);
            vwButton.Commands.Add(new("", commandName, ""));
            assignedButtons.Add(vwButton);
        }
    }

    private void BuildAssignedButtons(
        RinceDCSJoystick stick,
        List<AssignedButton> assignedButtons,
        Dictionary<AssignedButtonKey, RinceDCSJoystickButton> buttonsOnLayout,
        string bindingId,
        string commandName,
        string categoryName,
        DCSAircraftJoystickBinding bindingButtons)
    {
        foreach (IDCSButton button in bindingButtons.AssignedButtons.Values)
        {
            AssignedButtonKey key;
            GameAssignedButtonKey key;
            bool isModifer = button.Modifiers.Count > 0;
            if (button is DCSAxisButton)
            {
                key = new(button.Key.Name, isModifer);
            }
            else
            {
                key = new(button.Key.Name, isModifer);
            }
            if (buttonsOnLayout.ContainsKey(key))
            {
                AssignedButton vwButton = GetAssignedButtons(assignedButtons, buttonsOnLayout[key]);
                vwButton.Commands.Add(new(bindingId, commandName, categoryName));
                AddButtonConfiguration(button, vwButton);
            }
        }
    }

    private void AddButtonConfiguration(DCSButton button, GameAssignedButton vwButton)
    {
        vwButton.Modifiers.AddRange(button.Modifiers);
        if (button is DCSAxisButton)
        {
            vwButton.IsAxisButton = true;
            DCSAxisButton axisButton = button as DCSAxisButton;
            if (axisButton.Filter != null)
            {
                foreach (double curve in axisButton.Filter.Curvature)
                {
                    vwButton.Filter.Curvature.Add((int)(curve * 100));
                }
                vwButton.Filter.Deadzone = (int)axisButton.Filter.Deadzone;
                vwButton.Filter.HardwareDetent = axisButton.Filter.HardwareDetent;
                vwButton.Filter.HardwareDetentAB = (int)axisButton.Filter.HardwareDetentAB;
                vwButton.Filter.HardwareDetentMax = (int)axisButton.Filter.HardwareDetentMax;
                vwButton.Filter.Invert = axisButton.Filter.Invert;
                vwButton.Filter.SaturationX = (int)(axisButton.Filter.SaturationX * 100);
                vwButton.Filter.SaturationY = (int)(axisButton.Filter.SaturationY * 100);
                vwButton.Filter.Slider = axisButton.Filter.Slider;
            }
            else
            {
                //  Set to default values
                vwButton.Filter.Curvature.Add(0);
                vwButton.Filter.Deadzone = 0;
                vwButton.Filter.HardwareDetent = false;
                vwButton.Filter.HardwareDetentAB = 0;
                vwButton.Filter.HardwareDetentMax = 0;
                vwButton.Filter.Invert = false;
                vwButton.Filter.SaturationX = 100;
                vwButton.Filter.SaturationY = 100;
                vwButton.Filter.Slider = false;
            }
        }
        else 
        {
            vwButton.IsAxisButton = false;
        }
    }

    private AssignedButton GetAssignedButtons(List<AssignedButton> assignedButtons, RinceDCSJoystickButton gameJoystickButton)
    {
        foreach(AssignedButton button in assignedButtons)
        {
            if(button.JoystickButton.ButtonName == gameJoystickButton.ButtonName &&
                button.JoystickButton.IsModifier == gameJoystickButton.IsModifier) {
                return button;
            }
        }

        AssignedButton assigned = new(gameJoystickButton);
        assignedButtons.Add(assigned); 
        return assigned;
    }
}
