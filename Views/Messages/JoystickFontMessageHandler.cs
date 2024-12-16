// Copyright 2023-2024 Paul Scobell. Subject to the GPL-3.0 license.

using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using RinceDCS.Models;

namespace RinceDCS.Views.Messages;

public class JoystickFontMessageHandler
{
    public static void Register(Page page, RinceDCSJoystick stick)
    {
        WeakReferenceMessenger.Default.Register<PropertyChangedMessage<RinceDCSJoystick>>(page, (r, m) =>
        {
            if ((r is EditLayoutsPage) && stick != null)
            {
                stick.ButtonFont = stick.ButtonFont == null ? FontFamily.XamlAutoFontFamily.Source : stick.ButtonFont;
                stick.ButtonFontSize = stick.ButtonFontSize == 0 ? (int)page.FontSize : stick.ButtonFontSize;
            }
        });

    }
}
