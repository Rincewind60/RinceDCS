using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using RinceDCS.Models;
using RinceDCS.Services;
using RinceDCS.ViewModels;
using RinceDCS.ViewModels.Helpers;
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
    public sealed partial class ViewStickControl : UserControl
    {
        public ViewStickControl(string instanceName, string savedGamesFolder, RinceDCSJoystick stick, DCSData dcsData, RinceDCSAircraft currentAircraft)
        {
            this.InitializeComponent();

            this.DataContext = new ViewStickViewModel(instanceName, savedGamesFolder, stick, dcsData, currentAircraft);
        }

        public ViewStickViewModel ViewModel => (ViewStickViewModel)DataContext;

        private async void JoystickImage_Loaded(object sender, RoutedEventArgs e)
        {
            JoystickImage.Source = await JoystickUtil.GetImageSource(ViewModel.Stick);
            ButtonsItemsControl.Width = (JoystickImage.Source as BitmapSource).PixelWidth;
        }

        private void Expand_Click(object sender, RoutedEventArgs e)
        {
            ScaleVMHelper.Default.CurrentScale = Math.Max(ScaleVMHelper.Default.CurrentScale - 1, 0);
        }

        private void ScaleCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            JoystickScrollViewer.ChangeView(0, 0, ScaleVMHelper.Default.ZoomFactors[ScaleVMHelper.Default.CurrentScale]);
        }

        private void Shrink_Click(object sender, RoutedEventArgs e)
        {
            ScaleVMHelper.Default.CurrentScale = Math.Min(ScaleVMHelper.Default.CurrentScale + 1, ScaleVMHelper.Default.Scales.Count - 1);
        }

        private void ExportKneeboard_Click(object sender, RoutedEventArgs e)
        {
            JoystickUtil.ExportKneeboard(ViewModel.Stick.Image, ViewModel.AssignedButtons.ToList(), ViewModel.CurrentAircraftName, ViewModel.AttachedStick.DCSName, ViewModel.SavedGamesFolder, ViewModel.Stick.Font, ViewModel.Stick.FontSize);
        }

        private async void ExportImage_Click(object sender, RoutedEventArgs e)
        {
            string savePath = await DialogService.Default.OpenPickSaveFile("JoystickLabels.png", "PNG", ".png");

            JoystickUtil.ExportAssignedButtonsImage(ViewModel.Stick.Image, ViewModel.AssignedButtons.ToList(), ViewModel.Stick.Font, ViewModel.Stick.FontSize, savePath);
        }

        private void PrintImage_Click(object sender, RoutedEventArgs e)
        {
            JoystickUtil.PrintAssigedButtonsImage(ViewModel.Stick.Image, ViewModel.AssignedButtons.ToList(), ViewModel.Stick.Font, ViewModel.Stick.FontSize);
        }

        private void ButtonsItemsControl_LayoutUpdated(object sender, object e)
        {
            if (JoystickScrollViewer.ZoomFactor != ScaleVMHelper.Default.ZoomFactors[ScaleVMHelper.Default.CurrentScale])
            {
                JoystickScrollViewer.ChangeView(0, 0, ScaleVMHelper.Default.ZoomFactors[ScaleVMHelper.Default.CurrentScale]);
            }
        }
    }
}
