using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging.Messages;
using CommunityToolkit.Mvvm.Messaging;
using RinceDCS.Models;
using RinceDCS.ViewModels.Helpers;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml.Controls;

namespace RinceDCS.ViewModels;

public partial class ManageJoystickViewModel : ObservableRecipient,
                                               IRecipient<PropertyChangedMessage<RinceDCSAircraft>>
{
    [ObservableProperty]
    public ObservableCollection<ManagedButton> buttons;

    [ObservableProperty]
    private RinceDCSJoystick stick;

//    [ObservableProperty]
//    private AttachedJoystick attachedStick;

    public DCSData BindingsData { get; set; }

    private RinceDCSGroups Groups { get; set; }
    public string CurrentAircraftName { get; set; }

    public ScaleVMHelper ScaleHelper { get { return ScaleVMHelper.Default; } }

    public ManageJoystickViewModel(RinceDCSJoystick stick, RinceDCSGroups groups, DCSData data, RinceDCSAircraft currentAircraft)
    {
        Stick = stick;
        Groups = groups;
//        AttachedStick = Stick.AttachedJoystick;
        BindingsData = data;
        CurrentAircraftName = currentAircraft == null ? string.Empty : currentAircraft.Name;

        ReBuildViewButtons();
    }

    public void Receive(PropertyChangedMessage<RinceDCSAircraft> message)
    {
        CurrentAircraftName = message.NewValue == null ? string.Empty : new(message.NewValue.Name);
        ReBuildViewButtons();
    }

    public void ButtonGroupChanged(ManagedButton managedButton)
    {
        RinceDCSGroupJoystick newGroupJoystick = managedButton.Group.Joysticks.Find(row => row.Joystick == Stick.AttachedJoystick);

        //  Check if already assigned
        if(newGroupJoystick.Buttons.Any(row => row.Name == managedButton.JoystickButton.ButtonName &&
                                        row.IsModifier == managedButton.JoystickButton.IsModifier)) return;

        //  Remove this button from any other groups
        var query = (from grp in managedButton.Groups
                    from joystick in grp.Joysticks
                    from button in joystick.Buttons
                    where grp != managedButton.Group &&
                          joystick.Joystick == Stick.AttachedJoystick &&
                          button.Name == managedButton.JoystickButton.ButtonName &&
                          button.IsModifier == managedButton.JoystickButton.IsModifier
                    select new { joystick, button }).ToList();
        foreach(var stickButton in query)
        {
            stickButton.joystick.Buttons.Remove(stickButton.button);
        }

        //  Add to the new group
        RinceDCSGroupButton newButton = new();
        newButton.Name = managedButton.JoystickButton.ButtonName;
        if(managedButton.JoystickButton.IsModifier)
        {
            newButton.Modifiers.Add(Groups.DefaultModifierName);
        }
        newGroupJoystick.Buttons.Add(newButton);
    }

    private void ReBuildViewButtons()
    {
        JoystickVMHelper helper = new(BindingsData);
        Dictionary<AssignedButtonKey, RinceDCSJoystickButton> buttonsOnLayout = helper.GetJoystickButtonsOnLayout(Stick);
        List<ManagedButton> buttons = helper.GetManagedButtons(Stick, Groups, buttonsOnLayout, CurrentAircraftName);
        Buttons = buttons == null ? new() : new(buttons);
    }
}
