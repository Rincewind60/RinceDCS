// Copyright 2023-2024 Paul Scobell. Subject to the GPL-3.0 license.

using RinceDCS.Models;
using System;
using System.Collections.Generic;
using System.Drawing.Design;
using System.Linq;

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

        //  First Create an assigned button for each button on layout, this way all button labels will appear, even if no action assigned to them
        foreach (RinceDCSJoystickButton layoutButton in buttonsOnLayout.Values)
        {
            assignedButtons.Add(new AssignedButton(layoutButton));
        }

        DCSAircraftKey aircraftKey = new(aircraftName);
        DCSAircraft dcsAircraft = Data.Aircraft[aircraftKey];

        BuildAppButtons(assignedButtons, buttonsOnLayout, "Game", instanceName);
        BuildAppButtons(assignedButtons, buttonsOnLayout, "Plane", aircraftName);
        BuildAppButtons(assignedButtons, buttonsOnLayout, "Joystick", stick.AttachedJoystick.Name);

        foreach (DCSAction action in dcsAircraft.Actions.Values)
        {
            DCSAircraftAction aircraftBinding = action.Aircraft[aircraftKey];
            string commandName = aircraftBinding.Action;
            string categoryName = aircraftBinding.Category;

            DCSAircraftJoystickKey key = new(aircraftKey.Name, stick.AttachedJoystick.JoystickGuid);

            if (action.AircraftJoysticks.ContainsKey(key))
            {
                DCSAircraftJoystickAction bindingButtons = action.AircraftJoysticks[key];

                BuildAssignedButtons(action.IsAxis, stick, assignedButtons, buttonsOnLayout, action.Key.Id, commandName, categoryName, bindingButtons);
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

        foreach (RinceDCSJoystickButton button in buttonsOnLayout.Values)
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
                                             button.IsModifier == grpButton.IsModifier
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
        List<AssignedButton> assignedButtons,
        Dictionary<AssignedButtonKey, RinceDCSJoystickButton> buttonsOnLayout,
        string buttonName,
        string commandName
        )
    {
        AssignedButtonKey key = new(buttonName, false);
        if (buttonsOnLayout.ContainsKey(key))
        {
            AssignedButton vwButton = GetAssignedButton(assignedButtons, buttonsOnLayout[key]);
            vwButton.Actions.Add(new("", commandName, ""));
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
        DCSAircraftJoystickAction bindingButtons)
    {
        foreach (DCSButton button in bindingButtons.Buttons.Values)
        {
            AssignedButtonKey key = new(button.Name, button.IsModifier);
            if (buttonsOnLayout.ContainsKey(key))
            {
                AssignedButton vwButton = GetAssignedButton(assignedButtons, buttonsOnLayout[key]);
                vwButton.Actions.Add(new(bindingId, commandName, categoryName));
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

    private AssignedButton GetAssignedButton(List<AssignedButton> assignedButtons, RinceDCSJoystickButton gameJoystickButton)
    {
        foreach (AssignedButton button in assignedButtons)
        {
            if (button.JoystickButton.ButtonName == gameJoystickButton.ButtonName &&
                button.JoystickButton.IsModifier == gameJoystickButton.IsModifier)
            {
                return button;
            }
        }

        throw new Exception("Joystick button not in assignedButtons");
    }
}
