using RinceDCS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RinceDCS.ViewModels.Messages;

public class ExportAssignedButtonsImageMessage
{
    public RinceDCSJoystick Stick {  get; set; }
    public List<AssignedButton> AssignedButtons { get; set; }
    public string SaveFilePath { get; set; }

    public ExportAssignedButtonsImageMessage(RinceDCSJoystick stick, List<AssignedButton> assignedButtons, string saveFilePath)
    {
        Stick = stick;
        AssignedButtons = assignedButtons;
        SaveFilePath = saveFilePath;
    }
}
