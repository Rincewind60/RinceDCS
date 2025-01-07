// Copyright 2023-2024 Paul Scobell. Subject to the GPL-3.0 license.

using RinceDCS.Models;
using System.Collections.Generic;

namespace RinceDCS.ViewModels.Messages;

public class ExportKneeboardMessage
{
    public RinceDCSJoystick Stick { get; set; }
    public RinceDCSJoystickImage StickImage { get; set; }
    public List<AssignedButton> AssignedButtons { get; set; }
    public string AircraftName { get; set; }

    public ExportKneeboardMessage(RinceDCSJoystick stick, RinceDCSJoystickImage stickImage, List<AssignedButton> assignedButtons, string aircraftName)
    {
        Stick = stick;
        StickImage = stickImage;
        AssignedButtons = assignedButtons;
        AircraftName = aircraftName;
    }
}
