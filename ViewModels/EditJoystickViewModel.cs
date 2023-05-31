using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using RinceDCS.Models;
using RinceDCS.ServiceModels;
using System;
using System.IO;
using System.Linq;

namespace RinceDCS.ViewModels;

public partial class EditJoystickViewModel : ObservableRecipient,
                                             IRecipient<PropertyChangedMessage<Game>>
{
    [ObservableProperty]
    private AttachedJoystick attachedStick;

    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    private GameJoystick stick;

    [ObservableProperty]
    private GameJoystickButton currentJoystickButton;

    [ObservableProperty]
    private string currentScale;

    public string[] Scales = { "400%", "200%", "100%", "75%", "50%", "25%" };

    public EditJoystickViewModel(Game game, AttachedJoystick attachedStick)
    {
        IsActive = true;

        AttachedStick = attachedStick;
        CurrentJoystickButton = null;
        CurrentScale = Scales[2];

        Stick = (from gameStick in game.Joysticks
                 where gameStick.AttachedJoystick == AttachedStick
                 select gameStick).First();
    }

    public void Receive(PropertyChangedMessage<Game> message)
    {
        Stick = (from gameStick in message.NewValue.Joysticks
                    where gameStick.AttachedJoystick == AttachedStick
                    select gameStick).First();
        CurrentJoystickButton = null;
    }

    public void UpdateImage(string path)
    {
        Stick.Image = Ioc.Default.GetRequiredService<IFileService>().ReadImageFile(path);
    }
}
