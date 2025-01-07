// Copyright 2023-2024 Paul Scobell. Subject to the GPL-3.0 license.

using RinceDCS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RinceDCS.ViewModels.Messages;

public class AddNewJoystickImageMessage
{
    public RinceDCSJoystick Stick { get; set; }
    public RinceDCSJoystickImage StickImage { get; set; }

    public AddNewJoystickImageMessage(RinceDCSJoystick stick, RinceDCSJoystickImage stickImage)
    {
        Stick = stick;
        StickImage = stickImage;
    }
}
