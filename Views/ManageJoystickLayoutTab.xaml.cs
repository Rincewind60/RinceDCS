// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using RinceDCS.Models;
using RinceDCS.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RinceDCS.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ManageJoystickLayoutTab : Page
    {
        public ManageJoystickLayoutTab()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            Tuple<RinceDCSJoystick, DCSData, RinceDCSAircraft> data = e.Parameter as Tuple<RinceDCSJoystick, DCSData, RinceDCSAircraft>;

            RinceDCSJoystick stick = data.Item1;
            DCSData dcsData = data.Item2;
            RinceDCSAircraft currentAircraft = data.Item3;

            this.DataContext = new ManageJoystickViewModel(stick, dcsData, currentAircraft);
        }

        public ManageJoystickViewModel ViewModel => (ManageJoystickViewModel)DataContext;
    }
}
