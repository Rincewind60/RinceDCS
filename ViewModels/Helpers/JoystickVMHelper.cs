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
        if (string.IsNullOrWhiteSpace(aircraftName)) return null;

        List<AssignedButton> assignedButtons = new();

        DCSAircraftKey aircraftKey = new(aircraftName);
        DCSAircraft dcsAircraft = Data.Aircraft[aircraftKey];

        BuildAppButtons(stick, assignedButtons, buttonsOnLayout, "Game", instanceName);
        BuildAppButtons(stick, assignedButtons, buttonsOnLayout, "Plane", aircraftName);
        BuildAppButtons(stick, assignedButtons, buttonsOnLayout, "Joystick", stick.AttachedJoystick.Name);

        foreach (DCSBinding binding in dcsAircraft.Bindings.Values)
        {
            DCSAircraftBinding aircraftBinding = binding.Aircraft[aircraftKey];
            string commandName = aircraftBinding.Command;
            string categoryName = aircraftBinding.Category;

            DCSAircraftJoystickKey key = new(aircraftKey.Name, stick.AttachedJoystick.JoystickGuid);

            if (binding.AircraftJoysticks.ContainsKey(key))
            {
                DCSAircraftJoystickBinding bindingButtons = binding.AircraftJoysticks[key];

                BuildAssignedButtons(binding.IsAxis, stick, assignedButtons, buttonsOnLayout, binding.Key.Id, commandName, categoryName, bindingButtons);
            }
        }

        return assignedButtons;
    }

    public List<ManagedButton> GetManagedButtons(RinceDCSJoystick stick, RinceDCSGroups groups, Dictionary<AssignedButtonKey, RinceDCSJoystickButton> buttonsOnLayout, string aircraftName)
    {
        if (string.IsNullOrWhiteSpace(aircraftName)) return null;

        List<ManagedButton> buttons = new();

        List<RinceDCSGroup> axisGroups = (from grp in groups.Groups
                                          where grp.IsAxis == true &&
                                                grp.AircraftNames.Contains(aircraftName)
                                          select grp).OrderBy(row => row.Name).ToList();

        List<RinceDCSGroup> keyGroups = (from grp in groups.Groups
                                         where grp.IsAxis != true &&
                                               grp.AircraftNames.Contains(aircraftName)
                                         select grp).OrderBy(row => row.Name).ToList();

        foreach(RinceDCSJoystickButton button in buttonsOnLayout.Values)
        {
            if (button.ButtonName != "Game" && button.ButtonName != "Plane" && button.ButtonName != "Joystick")
            {
                ManagedButton newButton;
                if (button.IsKeyButton)
                {
                    newButton = new(button, keyGroups);
                }
                else
                {
                    newButton = new(button, axisGroups);
                }
                var groupsWithButton = from grp in groups.Groups
                                      from gj in grp.Joysticks
                                      from grpButton in gj.Buttons
                                      where gj.Joystick == stick.AttachedJoystick &&
                                            grpButton.Name == button.ButtonName &&
                                            ((button.IsModifier == true && grpButton.Modifiers.Count > 0) ||
                                            (button.IsModifier == false && grpButton.Modifiers.Count == 0))
                                      select grp;
                if (groupsWithButton.Count() > 0)
                {
                    newButton.Group = groupsWithButton.First();
                }
                buttons.Add(newButton);
            }
        }

        return buttons;
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
        bool IsAxis,
        RinceDCSJoystick stick,
        List<AssignedButton> assignedButtons,
        Dictionary<AssignedButtonKey, RinceDCSJoystickButton> buttonsOnLayout,
        string bindingId,
        string commandName,
        string categoryName,
        DCSAircraftJoystickBinding bindingButtons)
    {
        foreach (DCSButton button in bindingButtons.Buttons.Values)
        {
            AssignedButtonKey key;
            bool isModifer = button.Modifiers.Count > 0;
            key = new(button.Name, isModifer);
            if (buttonsOnLayout.ContainsKey(key))
            {
                AssignedButton vwButton = GetAssignedButtons(assignedButtons, buttonsOnLayout[key]);
                vwButton.Commands.Add(new(bindingId, commandName, categoryName));
                AddButtonConfiguration(IsAxis, button, vwButton);
            }
        }
    }

    private void AddButtonConfiguration(bool IsAxis, DCSButton button, AssignedButton vwButton)
    {
        vwButton.IsAxisButton = IsAxis;
        vwButton.Modifiers.AddRange(button.Modifiers);
        if (button.AxisFilter != null)
        {
            vwButton.AxisFilter = new(button.AxisFilter);
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
