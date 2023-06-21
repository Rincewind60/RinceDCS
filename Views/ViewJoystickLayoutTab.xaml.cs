// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using RinceDCS.Models;
using RinceDCS.ServiceModels;
using RinceDCS.ViewModels;
using RinceDCS.Views.Utilities;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Linq;
using System.Numerics;
using Windows.Networking;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RinceDCS.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ViewJoystickLayoutTab : Page
    {
        private float[] ZoomFactors = { 4F, 2F, 1F, 0.75F, 0.5F, 0.25F };

        public ViewJoystickLayoutTab()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            Tuple<string, string, GameJoystick, DCSData, GameAircraft> data = e.Parameter as Tuple<string, string, GameJoystick, DCSData, GameAircraft>;

            string instanceName = data.Item1;
            string savedGamesFolder = data.Item2;
            GameJoystick stick = data.Item3;
            DCSData dcsData = data.Item4;
            GameAircraft currentAircraft = data.Item5;

            this.DataContext = new ViewJoystickViewModel(instanceName, savedGamesFolder, stick, dcsData, currentAircraft);
        }

        public ViewJoystickViewModel ViewModel => (ViewJoystickViewModel)DataContext;

        private async void JoystickImage_Loaded(object sender, RoutedEventArgs e)
        {
            JoystickImage.Source = await JoystickUtil.GetImageSource(ViewModel.Stick);
        }

        private void Expand_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.CurrentScale = Math.Max(ViewModel.CurrentScale - 1, 0);
        }

        private void ScaleCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            JoystickScrollViewer.ChangeView(0, 0, ZoomFactors[ViewModel.CurrentScale]);
        }

        private void Shrink_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.CurrentScale = Math.Min(ViewModel.CurrentScale + 1, ViewModel.Scales.Length - 1);
        }

        private void ExportKneeboard_Click(object sender, RoutedEventArgs e)
        {
            JoystickUtil.ExportKneeboard(ViewModel.Stick.Image, ViewModel.AssignedButtons.ToList(), ViewModel.CurrentAircraftKey.Name, ViewModel.AttachedStick.DCSName, ViewModel.SavedGamesFolder, ViewModel.Stick.Font, ViewModel.Stick.FontSize);
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
    }

    public class IsValidBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (bool)value == true ? null : new SolidColorBrush(Colors.LightPink);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
