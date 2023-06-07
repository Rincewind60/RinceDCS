using RinceDCS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RinceDCS.ViewModels.Messages;

public class ExportAssignedButtonsImageMessage
{
    public GameJoystick Stick {  get; set; }
    public List<GameAssignedButton> AssignedButtons { get; set; }
    public string SaveFilePath { get; set; }

    public ExportAssignedButtonsImageMessage(GameJoystick stick, List<GameAssignedButton> assignedButtons, string saveFilePath)
    {
        Stick = stick;
        AssignedButtons = assignedButtons;
        SaveFilePath = saveFilePath;
    }
}
