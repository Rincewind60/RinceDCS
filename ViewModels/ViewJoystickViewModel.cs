using CommunityToolkit.Mvvm.ComponentModel;
using RinceDCS.Models;
using SharpDX.DirectInput;

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
