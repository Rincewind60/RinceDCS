// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.DependencyInjection;
using RinceDCS.Models;
using RinceDCS.Services;
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
using CommunityToolkit.Mvvm.Messaging.Messages;
using CommunityToolkit.Mvvm.Messaging;
using RinceDCS.ViewModels.Helpers;
using RinceDCS.ViewModels.Messages;
using Windows.ApplicationModel;

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

            GameViewModel vm = new();
            this.DataContext = vm;
            vm.IsActive = true;

            WeakReferenceMessenger.Default.Register<ExportAssignedButtonsImageMessage>(this, (r, m) =>
            {
                JoystickUtil.ExportAssignedButtonsImage(m.Stick.Image, m.AssignedButtons, m.Stick.Font, m.Stick.FontSize, m.SaveFilePath);
            });

            WeakReferenceMessenger.Default.Register<ExportKneeboardMessage>(this, (r, m) =>
            {
                JoystickUtil.ExportKneeboard(m.Stick.Image, m.AssignedButtons, m.AircraftName, m.Stick.AttachedJoystick.DCSName, ViewModel.CurrentInstance.SavedGameFolderPath, m.Stick.Font, m.Stick.FontSize);
            });
        }

        public GameViewModel ViewModel => (GameViewModel)DataContext;

        private async void UpdateInstances_Click(object sender, RoutedEventArgs e)
        {
            GameInstancesPage page = new(ViewModel.CurrentFile.Instances.ToList());
            Binding SaveButtonBinding = new Binding
            {
                Source = page.ViewModel,
                Path = new PropertyPath("IsValid"),
                Mode = BindingMode.OneWay
            };
            ContentDialogResult result = await DialogService.Default.OpenResponsePageDialog("Manage Game Instances", page, "Save", SaveButtonBinding, null, "Cancel");

            if (result == ContentDialogResult.Primary)
            {
                page.ViewModel.Save();
            }
        }

        private async void About_Click(object sender, RoutedEventArgs e)
        {
            AboutPage about = new();
            await DialogService.Default.OpenInfoPageDialog("About", about);
        }

        private void InstancesCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.CurrentInstanceChanged();
        }

        private void AircraftCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.CurrentAircraftChanged();
            if(ViewModel.JoystickMode == DetailsDisplayMode.View)
            {
                NavigateToViewPage();
            }
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            var helpWindow = WindowHelper.CreateWindow();
            helpWindow.Title = "Rince DCS - Help";
            HelpPage helpPage = new();
            helpPage.RequestedTheme = this.ActualTheme;
            helpWindow.Content = helpPage;
            helpWindow.Activate();
        }

        private void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToViewPage();
        }

        private void BindingsButton_Click(object sender, RoutedEventArgs e)
        {
            DetailsViewFrame.Navigate(typeof(BindingsTablePage), 
                Tuple.Create(ViewModel.CurrentInstanceDCSData, ViewModel.CurrentAircraft));
        }

        private void ManageButton_Click(object sender, RoutedEventArgs e)
        {
            DetailsViewFrame.Navigate(typeof(ManageJoystickPage),
                Tuple.Create(ViewModel.CurrentFile.Joysticks.ToList(), ViewModel.CurrentInstance, ViewModel.CurrentInstanceDCSData, ViewModel.CurrentAircraft));
        }

        private void GroupsButton_Click(object sender, RoutedEventArgs e)
        {
            DetailsViewFrame.Navigate(typeof(EditGroupsPage),
                Tuple.Create(ViewModel.CurrentInstance.BindingGroups, ViewModel.CurrentInstanceDCSData));
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            DetailsViewFrame.Navigate(typeof(EditJoystickLayoutPage), ViewModel.CurrentFile);
        }

        private void NavigateToViewPage()
        {
            DetailsViewFrame.Navigate(typeof(ViewJoystickPage),
                Tuple.Create(ViewModel.CurrentInstance.Name,
                             ViewModel.CurrentInstance.SavedGameFolderPath,
                             ViewModel.CurrentFile,
                             ViewModel.CurrentInstanceDCSData,
                             ViewModel.CurrentAircraft));
        }
    }
}
