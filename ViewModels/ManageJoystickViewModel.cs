using CommunityToolkit.Mvvm.ComponentModel;
using RinceDCS.Models;

namespace RinceDCS.ViewModels;

public partial class ManageJoystickViewModel : ObservableObject
{
    [ObservableProperty]
    private GameJoystick joystick;

    public ManageJoystickViewModel(GameJoystick stick, DCSData data, GameAircraft currentAircraft)
    {
        Joystick = stick;
    }
}
