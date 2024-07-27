// Copyright 2023-2024 Paul Scobell. Subject to the GPL-3.0 license.

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using RinceDCS.Models;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RinceDCS.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ViewSticksPage : Page
    {
        public ViewSticksPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            Tuple<string, string, RinceDCSFile, DCSData> data = e.Parameter as Tuple<string, string, RinceDCSFile, DCSData>;

            string instanceName = data.Item1;
            string savedGamesFolder = data.Item2;
            RinceDCSFile rinceDCSFile = data.Item3;
            DCSData dcsData = data.Item4;

            foreach (RinceDCSJoystick stick in rinceDCSFile.Joysticks)
            {
                TabViewItem newItem = new TabViewItem();
                newItem.Header = stick.AttachedJoystick.Name;
                newItem.IsClosable = false;

                ViewStickControl ctrl = new(instanceName, savedGamesFolder, stick, dcsData);
                ctrl.ViewModel.IsActive = true;

                newItem.Content = ctrl;
                ViewSticks.TabItems.Add(newItem);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            foreach (TabViewItem item in ViewSticks.TabItems)
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
