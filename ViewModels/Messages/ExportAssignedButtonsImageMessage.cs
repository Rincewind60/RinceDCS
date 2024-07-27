// Copyright 2023-2024 Paul Scobell. Subject to the GPL-3.0 license.

using RinceDCS.Models;
using System.Collections.Generic;

namespace RinceDCS.ViewModels.Messages;

public class ExportAssignedButtonsImageMessage
{
    public RinceDCSJoystick Stick { get; set; }
    public List<AssignedButton> AssignedButtons { get; set; }
    public string SaveFilePath { get; set; }

    public ExportAssignedButtonsImageMessage(RinceDCSJoystick stick, List<AssignedButton> assignedButtons, string saveFilePath)
    {
        Stick = stick;
        AssignedButtons = assignedButtons;
        SaveFilePath = saveFilePath;
    }
}
