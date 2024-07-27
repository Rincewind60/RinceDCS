// Copyright 2023-2024 Paul Scobell. Subject to the GPL-3.0 license.

using RinceDCS.Models;
using System.Collections.Generic;

namespace RinceDCS.ViewModels.Messages;

public class ExportKneeboardMessage
{
    public RinceDCSJoystick Stick { get; set; }
    public List<AssignedButton> AssignedButtons { get; set; }
    public string AircraftName { get; set; }

    public ExportKneeboardMessage(RinceDCSJoystick stick, List<AssignedButton> assignedButtons, string aircraftName)
    {
        Stick = stick;
        AssignedButtons = assignedButtons;
        AircraftName = aircraftName;
    }
}
