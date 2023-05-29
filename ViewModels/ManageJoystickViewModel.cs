using CommunityToolkit.Mvvm.ComponentModel;
using RinceDCS.Models;
using SharpDX.DirectInput;

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
