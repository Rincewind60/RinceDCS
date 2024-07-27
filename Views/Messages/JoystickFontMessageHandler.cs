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
                stick.Font = stick.Font == null ? FontFamily.XamlAutoFontFamily.Source : stick.Font;
                stick.FontSize = stick.FontSize == 0 ? (int)page.FontSize : stick.FontSize;
            }
        });

    }
}
