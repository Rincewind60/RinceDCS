// Copyright 2023-2024 Paul Scobell. Subject to the GPL-3.0 license.

using System.Collections.Generic;

namespace RinceDCS.Models;

public record AssignedButtonKey(string ButtonName, bool IsModifier);

public class AssignedButton
{
    public bool IsAxisButton { get; set; }
    public RinceDCSJoystickButton JoystickButton { get; }
    public List<string> Modifiers { get; } = new();
    public List<AssignedAction> Actions { get; } = new();
    public AxisFilter AxisFilter { get; set; }

    public bool IsModifier { get { return Modifiers.Count > 0; } }

    public string Action
    {
        get
        {
            if (Actions.Count == 0) { return ""; }

            string action = Actions[0].ActionName;
            for (int i = 1; i < Actions.Count; i++)
            {
                action += "|" + Actions[i].ActionName;
            }
            return action;
        }
    }

    public string Category
    {
        get
        {
            if (Actions.Count == 0) { return ""; }

            string category = Actions[0].CategoryName;
            for (int i = 1; i < Actions.Count; i++)
            {
                category += "|" + Actions[i].CategoryName;
            }
            return category;
        }
    }

    public string ID
    {
        get
        {
            if (Actions.Count == 0) { return ""; }

            string id = Actions[0].ActionId;
            for (int i = 1; i < Actions.Count; i++)
            {
                id += "|" + Actions[i].ActionId;
            }
            return id;
        }
    }

    public string Modifier
    {
        get
        {
            string modifier = IsModifier ? Modifiers[0] : "";
            for (int i = 1; i < Modifiers.Count; i++)
            {
                modifier += "," + Modifiers[i];
            }
            return modifier;
        }
    }

    public bool IsValid { get { return Actions.Count < 2; } }

    public AssignedButton(RinceDCSJoystickButton joyButton)
    {
        JoystickButton = joyButton;
    }
}

public class AssignedAction
{
    public string ActionId { get; }
    public string ActionName { get; }
    public string CategoryName { get; }

    public AssignedAction(string actionId, string actionName, string categoryName)
    {
        ActionId = actionId;
        ActionName = actionName;
        CategoryName = categoryName;
    }
}