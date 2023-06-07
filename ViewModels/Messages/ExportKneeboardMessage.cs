using RinceDCS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RinceDCS.ViewModels.Messages;

public class ExportKneeboardMessage
{
    public GameJoystick Stick { get; set; }
    public List<GameAssignedButton> AssignedButtons { get; set; }
    public string AircraftName { get; set; }

    public ExportKneeboardMessage(GameJoystick stick, List<GameAssignedButton> assignedButtons, string aircraftName)
    {
        Stick = stick;
        AssignedButtons = assignedButtons;
        AircraftName = aircraftName;
    }
}
