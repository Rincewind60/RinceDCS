using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using RinceDCS.Models;
using RinceDCS.ServiceModels;
using RinceDCS.ViewModels;
using RinceDCS.Views.Utilities;
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
    public sealed partial class ViewJoystickLayoutControl : UserControl
    {
        public ViewJoystickLayoutControl(string instanceName, string savedGamesFolder, RinceDCSJoystick stick, DCSData dcsData, RinceDCSAircraft currentAircraft)
        {
            this.InitializeComponent();

            this.DataContext = new ViewJoystickViewModel(instanceName, savedGamesFolder, stick, dcsData, currentAircraft);
        }

        public ViewJoystickViewModel ViewModel => (ViewJoystickViewModel)DataContext;

        private async void JoystickImage_Loaded(object sender, RoutedEventArgs e)
        {
            JoystickImage.Source = await JoystickUtil.GetImageSource(ViewModel.Stick);
        }

        private void Expand_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ScaleHelper.CurrentScale = Math.Max(ViewModel.ScaleHelper.CurrentScale - 1, 0);
        }

        private void ScaleCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            JoystickScrollViewer.ChangeView(0, 0, ViewModel.ScaleHelper.ZoomFactors[ViewModel.ScaleHelper.CurrentScale]);
        }

        private void Shrink_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ScaleHelper.CurrentScale = Math.Min(ViewModel.ScaleHelper.CurrentScale + 1, ViewModel.ScaleHelper.Scales.Count - 1);
        }

        private void ExportKneeboard_Click(object sender, RoutedEventArgs e)
        {
            JoystickUtil.ExportKneeboard(ViewModel.Stick.Image, ViewModel.AssignedButtons.ToList(), ViewModel.CurrentAircraftName, ViewModel.AttachedStick.DCSName, ViewModel.SavedGamesFolder, ViewModel.Stick.Font, ViewModel.Stick.FontSize);
        }

        private async void ExportImage_Click(object sender, RoutedEventArgs e)
        {
            string savePath = await Ioc.Default.GetRequiredService<IDialogService>().OpenPickSaveFile("JoystickLabels.png", "PNG", ".png");

            JoystickUtil.ExportAssignedButtonsImage(ViewModel.Stick.Image, ViewModel.AssignedButtons.ToList(), ViewModel.Stick.Font, ViewModel.Stick.FontSize, savePath);
        }

        private void PrintImage_Click(object sender, RoutedEventArgs e)
        {
            JoystickUtil.PrintAssigedButtonsImage(ViewModel.Stick.Image, ViewModel.AssignedButtons.ToList(), ViewModel.Stick.Font, ViewModel.Stick.FontSize);
        }

        private void ButtonsItemsControl_LayoutUpdated(object sender, object e)
        {
            if (JoystickScrollViewer.ZoomFactor != ViewModel.ScaleHelper.ZoomFactors[ViewModel.ScaleHelper.CurrentScale])
            {
                JoystickScrollViewer.ChangeView(0, 0, ViewModel.ScaleHelper.ZoomFactors[ViewModel.ScaleHelper.CurrentScale]);
            }
        }
    }
}
