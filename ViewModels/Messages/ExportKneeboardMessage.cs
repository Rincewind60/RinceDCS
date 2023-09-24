using RinceDCS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
