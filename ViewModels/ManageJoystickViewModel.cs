using CommunityToolkit.Mvvm.ComponentModel;
using RinceDCS.Models;

namespace RinceDCS.ViewModels;

public partial class ManageJoystickViewModel : ObservableObject
{
    [ObservableProperty]
    private RinceDCSJoystick joystick;

    public ManageJoystickViewModel(RinceDCSJoystick stick, DCSData data, RinceDCSAircraft currentAircraft)
    {
        Joystick = stick;
    }
}
