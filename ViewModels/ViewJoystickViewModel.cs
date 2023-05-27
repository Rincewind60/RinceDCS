using CommunityToolkit.Mvvm.ComponentModel;
using RinceDCS.Models;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RinceDCS.ViewModels;

public partial class ViewJoystickViewModel : ObservableObject
{
    [ObservableProperty]
    private GameJoystick joystick;

    public ViewJoystickViewModel(GameJoystick joystick)
    {
        Joystick = joystick;
    }
}
