// Copyright 2023-2024 Paul Scobell. Subject to the GPL-3.0 license.

using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using RinceDCS.Services;
using RinceDCS.ViewModels;
using RinceDCS.ViewModels.Messages;
using RinceDCS.Views.Utilities;
using System;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RinceDCS.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AppPage : Page
    {
        public AppPage()
        {
            this.InitializeComponent();

            AppVM vm = new();
            this.DataContext = vm;
            vm.IsActive = true;

            WeakReferenceMessenger.Default.Register<ExportAssignedButtonsImageMessage>(this, (r, m) =>
            {
                JoystickUtil.ExportAssignedButtonsImage(m.Stick.Image, m.AssignedButtons, m.Stick.ButtonHeight, m.Stick.ButtonWidth, m.Stick.ButtonFont, m.Stick.ButtonFontSize, m.SaveFilePath);
            });

            WeakReferenceMessenger.Default.Register<ExportKneeboardMessage>(this, (r, m) =>
            {
                JoystickUtil.ExportKneeboard(m.Stick.Image, m.AssignedButtons, m.AircraftName, m.Stick.AttachedJoystick.DCSName, ViewModel.CurrentInstance.SavedGamesPath, m.Stick.ButtonHeight, m.Stick.ButtonWidth, m.Stick.ButtonFont, m.Stick.ButtonFontSize);
            });
        }

        public AppVM ViewModel => (AppVM)DataContext;

        private void ViewSticks_Click(object sender, RoutedEventArgs e)
        {
            NavigateToViewSticksPage();
        }

        private void ViewActions_Click(object sender, RoutedEventArgs e)
        {
            DetailsViewFrame.Navigate(typeof(ViewActionsPage), ViewModel.CurrentInstanceDCSData);
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

        private void EditSticks_Click(object sender, RoutedEventArgs e)
        {
            DetailsViewFrame.Navigate(typeof(EditSticksPage),
                Tuple.Create(ViewModel.CurrentFile.Joysticks.ToList(), ViewModel.CurrentInstance, ViewModel.CurrentInstanceDCSData));
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
                             ViewModel.CurrentInstanceDCSData));
        }

        private async void EditModifiers_Click(object sender, RoutedEventArgs e)
        {
            ModifiersDialog page = new(ViewModel.CurrentInstanceGroups.Modifiers, ViewModel.CurrentInstanceGroups.DefaultModifierName);
            ContentDialogResult result = await DialogService.Default.OpenResponsePageDialog("Edit Modifiers", page, "Save", null, null, null);
            if (result == ContentDialogResult.Primary)
            {
                ViewModel.CurrentInstanceGroups.Modifiers = page.ViewModel.GetUpdatedModifiers();
                ViewModel.CurrentInstanceGroups.DefaultModifierName = page.ViewModel.GetUpdatedDefaultModifierName();
            }
        }
    }
}
