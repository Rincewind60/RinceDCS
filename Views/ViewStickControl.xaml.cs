// Copyright 2023-2024 Paul Scobell. Subject to the GPL-3.0 license.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using RinceDCS.Models;
using RinceDCS.Services;
using RinceDCS.ViewModels;
using RinceDCS.ViewModels.Helpers;
using RinceDCS.Views.Utilities;
using System;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

///TODO: Try Viewbox to scale stick image?

namespace RinceDCS.Views
{
    public sealed partial class ViewStickControl : UserControl
    {
        public ViewStickControl(string instanceName, string savedGamesFolder, RinceDCSJoystick stick, DCSData dcsData)
        {
            this.InitializeComponent();

            this.DataContext = new ViewStickVM(instanceName, savedGamesFolder, stick, dcsData);
        }

        public ViewStickVM ViewModel => (ViewStickVM)DataContext;

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
            JoystickUtil.ExportKneeboard(ViewModel.Stick.Image, ViewModel.AssignedButtons.ToList(), FilterToolbarVM.Default.SelectedAircraft, ViewModel.AttachedStick.DCSName, ViewModel.SavedGamesFolder, ViewModel.Stick.ButtonHeight, ViewModel.Stick.ButtonWidth, ViewModel.Stick.ButtonFont, ViewModel.Stick.ButtonFontSize);
        }

        private async void ExportImage_Click(object sender, RoutedEventArgs e)
        {
            string savePath = await DialogService.Default.OpenPickSaveFile("JoystickLabels.png", "PNG", ".png");

            JoystickUtil.ExportAssignedButtonsImage(ViewModel.Stick.Image, ViewModel.AssignedButtons.ToList(), ViewModel.Stick.ButtonHeight, ViewModel.Stick.ButtonWidth, ViewModel.Stick.ButtonFont, ViewModel.Stick.ButtonFontSize, savePath);
        }

        private void PrintImage_Click(object sender, RoutedEventArgs e)
        {
            JoystickUtil.PrintAssigedButtonsImage(ViewModel.Stick.Image, ViewModel.AssignedButtons.ToList(), ViewModel.Stick.ButtonHeight, ViewModel.Stick.ButtonWidth, ViewModel.Stick.ButtonFont, ViewModel.Stick.ButtonFontSize);
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
