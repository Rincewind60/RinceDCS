using CommunityToolkit.Mvvm.Messaging.Messages;
using CommunityToolkit.Mvvm.Messaging;
using RinceDCS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace RinceDCS.Views.Messages;

public class JoystickFontMessageHandler
{
    public static void Register(Page page, RinceDCSJoystick stick)
    {
        WeakReferenceMessenger.Default.Register<PropertyChangedMessage<RinceDCSJoystick>>(page, (r, m) => {
            if ((r is EditLayoutsPage) && stick != null)
            {
                stick.Font = stick.Font == null ? FontFamily.XamlAutoFontFamily.Source : stick.Font;
                stick.FontSize = stick.FontSize == 0 ? (int)page.FontSize : stick.FontSize;
            }
        });

    }
}
