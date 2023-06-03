// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.DependencyInjection;
using RinceDCS.Models;
using RinceDCS.ServiceModels;
using RinceDCS.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.EnterpriseData;
using System.Threading.Tasks;
using RinceDCS.Views.Utilities;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RinceDCS.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GamePage : Page
    {
        public GamePage()
        {
            this.InitializeComponent();

            this.DataContext = new GameViewModel();
        }

        public GameViewModel ViewModel => (GameViewModel)DataContext;

        private async void UpdateInstances_Click(object sender, RoutedEventArgs e)
        {
            GameInstancesPage page = new(ViewModel.CurrentGame.Instances.ToList());
            Binding SaveButtonBinding = new Binding
            {
                Source = page.ViewModel,
                Path = new PropertyPath("IsValid"),
                Mode = BindingMode.OneWay
            };
            ContentDialogResult result = await Ioc.Default.GetRequiredService<IDialogService>().OpenResponsePageDialog("Manage Game Instances", page, "Save", SaveButtonBinding, null, "Cancel");

            if (result == ContentDialogResult.Primary)
            {
                page.ViewModel.Save();
            }
        }

        private async void About_Click(object sender, RoutedEventArgs e)
        {
            AboutPage about = new();
            await Ioc.Default.GetRequiredService<IDialogService>().OpenInfoPageDialog("About", about);
        }

        private void InstancesCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.CurrentInstanceChanged();
        }

        private void AircraftCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.CurrentAircraftChanged();
        }

        /// <summary>
        /// As each Joystick tab is loaded we create its view and pass in the appropriate JosStick model.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewJoystickLayouts_Loaded(object sender, RoutedEventArgs e)
        {
            TabView tabView = sender as TabView;
            GameAircraft currentAircraft = ViewModel.CurrentAircraft == null ? null : ViewModel.CurrentAircraft;

            foreach (AttachedJoystick stick in ViewModel.AttachedJoysticks)
            {
                TabViewItem newItem = new TabViewItem();
                newItem.Header = stick.Name;
                newItem.IsClosable = false;

                Frame frame = new Frame();
                frame.Navigate(typeof(ViewJoystickLayoutPage), Tuple.Create(ViewModel.CurrentGame, stick, ViewModel.CurrentInstanceBindingsData, currentAircraft));

                newItem.Content = frame;
                tabView.TabItems.Add(newItem);
            }
        }

        /// <summary>
        /// As each Joystick tab is loaded we create its view and pass in the appropriate JosStick model.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ManageJoystickLayouts_Loaded(object sender, RoutedEventArgs e)
        {
            TabView tabView = sender as TabView;

            foreach (AttachedJoystick stick in ViewModel.AttachedJoysticks)
            {
                TabViewItem newItem = new TabViewItem();
                newItem.Header = stick.Name;
                newItem.IsClosable = false;

                Frame frame = new Frame();
                frame.Navigate(typeof(ManageJoystickLayoutPage), Tuple.Create(ViewModel.CurrentGame, stick));

                newItem.Content = frame;
                tabView.TabItems.Add(newItem);
            }
        }

        /// <summary>
        /// As each Joystick tab is loaded we create its view and pass in the appropriate JosStick model.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditJoystickLayouts_Loaded(object sender, RoutedEventArgs e)
        {
            TabView tabView = sender as TabView;

            foreach (AttachedJoystick stick in ViewModel.AttachedJoysticks)
            {
                TabViewItem newItem = new TabViewItem();
                newItem.Header = stick.Name;
                newItem.IsClosable = false;

                Frame frame = new Frame();
                frame.Navigate(typeof(EditJoystickLayoutPage), Tuple.Create(ViewModel.CurrentGame, stick));

                newItem.Content = frame;
                tabView.TabItems.Add(newItem);
            }
        }

        private void BindingsTable_Loaded(object sender, RoutedEventArgs e)
        {
            TabView tabView = sender as TabView;
            GameAircraft currentAircraft = ViewModel.CurrentAircraft == null ? null : ViewModel.CurrentAircraft;

            BindingsTableFrame.Navigate(typeof(BindingsTablePage), Tuple.Create(ViewModel.CurrentInstanceBindingsData, currentAircraft));
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            var helpWindow = WindowHelper.CreateWindow();
            HelpPage helpPage = new();
            helpPage.RequestedTheme = this.ActualTheme;
            helpWindow.Content = helpPage;
            helpWindow.Activate();
        }
    }

    public class ModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value.ToString() == parameter.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            bool isChecked = (bool)value;

            if(isChecked)
            {
                DisplayMode mode;
                Enum.TryParse(parameter.ToString(), true, out mode);
                return mode;
            }
            else
            {
                return null;
            }
        }
    }

    public class ModeVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value.ToString() == parameter.ToString() ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class AircraftVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value.ToString() == DisplayMode.Edit.ToString() ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
