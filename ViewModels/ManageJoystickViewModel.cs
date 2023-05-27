using CommunityToolkit.Mvvm.ComponentModel;
using RinceDCS.Models;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RinceDCS.ViewModels;

public partial class ManageJoystickViewModel : ObservableObject
{
    [ObservableProperty]
    private GameJoystick joystick;

    public ManageJoystickViewModel(GameJoystick joystick)
    {
        Joystick = joystick;
    }
}
