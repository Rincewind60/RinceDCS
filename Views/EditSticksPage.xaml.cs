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
    public sealed partial class EditSticksPage : Page
    {
        public EditSticksPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            Tuple<List<RinceDCSJoystick>, RinceDCSInstance, DCSData, RinceDCSAircraft> data = e.Parameter as Tuple<List<RinceDCSJoystick>, RinceDCSInstance, DCSData, RinceDCSAircraft>;

            List<RinceDCSJoystick> joysticks = data.Item1;
            RinceDCSInstance rinceDCSInstance = data.Item2;
            DCSData dcsData = data.Item3;
            RinceDCSAircraft currentAircraft = data.Item4;

            foreach (RinceDCSJoystick stick in joysticks)
            {
                TabViewItem newItem = new TabViewItem();
                newItem.Header = stick.AttachedJoystick.Name;
                newItem.IsClosable = false;

                EditStickControl ctrl = new(stick, rinceDCSInstance.Groups, dcsData, currentAircraft);
                ctrl.ViewModel.IsActive = true;

                newItem.Content = ctrl;
                ManageJoysticks.TabItems.Add(newItem);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            foreach (TabViewItem item in ManageJoysticks.TabItems)
            {
                ViewStickControl ctrl = item.Content as ViewStickControl;
                if (ctrl != null)
                {
                    ctrl.ViewModel.IsActive = false;
                }
            }
        }
    }
}
