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
                JoystickUtil.ExportKneeboard(m.Stick.Image, m.AssignedButtons, m.AircraftName, m.Stick.AttachedJoystick.DCSName, ViewModel.CurrentInstance.SavedGamesPath, m.Stick.Font, m.Stick.FontSize);
            });
        }

        public GameViewModel ViewModel => (GameViewModel)DataContext;

        private void ViewSticks_Click(object sender, RoutedEventArgs e)
        {
            NavigateToViewSticksPage();
        }

        private void ViewActions_Click(object sender, RoutedEventArgs e)
        {
            DetailsViewFrame.Navigate(typeof(ViewActionsPage),
                Tuple.Create(ViewModel.CurrentInstanceDCSData, ViewModel.CurrentAircraft));
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

        private async void About_Click(object sender, RoutedEventArgs e)
        {
            AboutDialog about = new();
            await DialogService.Default.OpenInfoPageDialog("About", about);
        }

        private void InstancesCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.CurrentInstanceChanged();
        }

        private async void UpdateInstances_Click(object sender, RoutedEventArgs e)
        {
            GameInstancesDialog page = new(ViewModel.CurrentFile.Instances.ToList());
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

        private void AircraftCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.CurrentAircraftChanged();
            if(ViewModel.JoystickMode == DetailsDisplayMode.View)
            {
                NavigateToViewSticksPage();
            }
        }

        private void EditSticks_Click(object sender, RoutedEventArgs e)
        {
            DetailsViewFrame.Navigate(typeof(EditSticksPage),
                Tuple.Create(ViewModel.CurrentFile.Joysticks.ToList(), ViewModel.CurrentInstance, ViewModel.CurrentInstanceDCSData, ViewModel.CurrentAircraft));
        }

        private void EditGroups_Click(object sender, RoutedEventArgs e)
        {
            DetailsViewFrame.Navigate(typeof(EditGroupsPage),
                Tuple.Create(ViewModel.CurrentInstance.Groups, ViewModel.CurrentInstanceDCSData));
        }

        private void EditLayouts_Click(object sender, RoutedEventArgs e)
        {
            DetailsViewFrame.Navigate(typeof(EditLayoutsPage), ViewModel.CurrentFile);
        }

        private void NavigateToViewSticksPage()
        {
            DetailsViewFrame.Navigate(typeof(ViewSticksPage),
                Tuple.Create(ViewModel.CurrentInstance.Name,
                             ViewModel.CurrentInstance.SavedGamesPath,
                             ViewModel.CurrentFile,
                             ViewModel.CurrentInstanceDCSData,
                             ViewModel.CurrentAircraft));
        }

        private async void EditModifiers_Click(object sender, RoutedEventArgs e)
        {
            EditModifiersPage page = new();
            ContentDialogResult result = await DialogService.Default.OpenResponsePageDialog("Edit Modifiers", page, "Save", null, null, null);
            if(result == ContentDialogResult.Primary)
            {
                ///TODO: Handle Modifiers save
            }
        }
    }
}
