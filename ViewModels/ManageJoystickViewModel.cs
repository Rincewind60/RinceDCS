using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging.Messages;
using CommunityToolkit.Mvvm.Messaging;
using RinceDCS.Models;
using RinceDCS.ViewModels.Helpers;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace RinceDCS.ViewModels;

public partial class ManageJoystickViewModel : ObservableRecipient,
                                               IRecipient<PropertyChangedMessage<RinceDCSAircraft>>
{
    [ObservableProperty]
    public ObservableCollection<ManagedButton> buttons;

    [ObservableProperty]
    private RinceDCSJoystick stick;

    [ObservableProperty]
    private AttachedJoystick attachedStick;

    public DCSData BindingsData { get; set; }

    private RinceDCSGroups Groups { get; set; }
    public string CurrentAircraftName { get; set; }

    public ScaleVMHelper ScaleHelper { get { return ScaleVMHelper.Default; } }

    public ManageJoystickViewModel(RinceDCSJoystick stick, RinceDCSGroups groups, DCSData data, RinceDCSAircraft currentAircraft)
    {
        Stick = stick;
        Groups = groups;
        AttachedStick = Stick.AttachedJoystick;
        BindingsData = data;
        CurrentAircraftName = currentAircraft == null ? string.Empty : currentAircraft.Name;

        ReBuildViewButtons();
    }

    public void Receive(PropertyChangedMessage<RinceDCSAircraft> message)
    {
        CurrentAircraftName = message.NewValue == null ? string.Empty : new(message.NewValue.Name);
        ReBuildViewButtons();
    }

    public void ButtonGroupChanged(ManagedButton button, RinceDCSGroup group)
    {
        //  Remove this button from any other groups


        //  Add to this group if not alread a member
    }

    private void ReBuildViewButtons()
    {
        JoystickVMHelper helper = new(BindingsData);
        Dictionary<AssignedButtonKey, RinceDCSJoystickButton> buttonsOnLayout = helper.GetJoystickButtonsOnLayout(Stick);
        List<ManagedButton> buttons = helper.GetManagedButtons(Stick, Groups, buttonsOnLayout, CurrentAircraftName);
        Buttons = buttons == null ? new() : new(buttons);
    }
}
