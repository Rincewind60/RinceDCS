// Copyright 2023-2024 Paul Scobell. Subject to the GPL-3.0 license.

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using RinceDCS.Models;
using System;
using System.Collections.Generic;

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

            Tuple<List<RinceDCSJoystick>, RinceDCSInstance, DCSData> data = e.Parameter as Tuple<List<RinceDCSJoystick>, RinceDCSInstance, DCSData>;

            List<RinceDCSJoystick> joysticks = data.Item1;
            RinceDCSInstance rinceDCSInstance = data.Item2;
            DCSData dcsData = data.Item3;

            foreach (RinceDCSJoystick stick in joysticks)
            {
                TabViewItem newItem = new TabViewItem();
                newItem.Header = stick.AttachedJoystick.Name;
                newItem.IsClosable = false;

                EditStickControl ctrl = new(stick, rinceDCSInstance.Groups, dcsData);
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
