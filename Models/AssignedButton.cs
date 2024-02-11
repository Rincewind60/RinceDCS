﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RinceDCS.Models;

public record AssignedButtonKey(string ButtonName, bool IsModifier);

public class AssignedButton
{
    public bool IsAxisButton { get; set; }
    public RinceDCSJoystickButton JoystickButton { get; }
    public List<string> Modifiers { get; } = new();
    public List<AssignedCommand> Commands { get; } = new();
    public AxisFilter AxisFilter { get; set; }

    public string Command
    {
        get
        {
            string command = Commands[0].CommandName;
            for (int i = 1; i < Commands.Count; i++)
            {
                command += "|" + Commands[i].CommandName;
            }
            return command;
        }
    }

    public string Category
    {
        get
        {
            string category = Commands[0].CategoryName;
            for (int i = 1; i < Commands.Count; i++)
            {
                category += "|" + Commands[i].CategoryName;
            }
            return category;
        }
    }

    public string ID
    {
        get
        {
            string id = Commands[0].BindID;
            for (int i = 1; i < Commands.Count; i++)
            {
                id += "|" + Commands[i].BindID;
            }
            return id;
        }
    }

    public string Modifier
    {
        get
        {
            string modifier = Modifiers.Count > 0 ? Modifiers[0] : "";
            for (int i = 1; i < Modifiers.Count; i++)
            {
                modifier += "," + Modifiers[i];
            }
            return modifier;
        }
    }

    public bool IsValid { get { return Commands.Count == 1; } }

    public AssignedButton(RinceDCSJoystickButton joyButton)
    {
        JoystickButton = joyButton;
    }
}

public class AssignedCommand
{
    public string BindID { get; }
    public string CommandName { get; }
    public string CategoryName { get; }

    public AssignedCommand(string bindID, string commandName, string categoryName)
    {
        BindID = bindID;
        CommandName = commandName;
        CategoryName = categoryName;
    }
}