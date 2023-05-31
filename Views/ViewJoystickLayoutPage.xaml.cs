// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.Messaging.Messages;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using RinceDCS.Models;
using RinceDCS.ViewModels;
using RinceDCS.Views.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using RinceDCS.Views.Messages;
using System.Numerics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RinceDCS.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ViewJoystickLayoutPage : Page
    {
        private Vector3[] ScaleValues = { new Vector3(4.0F), new Vector3(2.0F), new Vector3(1.0F), new Vector3(0.75F), new Vector3(0.50F), new Vector3(0.25F) };

        private double imageActualHeight = 0;
        private double imageActualWidth = 0;

        public ViewJoystickLayoutPage()
        {
            this.InitializeComponent();

            JoystickFontMessageHandler.Register(this, ViewModel == null ? null : ViewModel.Stick);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            Tuple<Game, AttachedJoystick, DCSData, GameAircraft> data = e.Parameter as Tuple<Game, AttachedJoystick, DCSData, GameAircraft>;

            Game game = data.Item1;
            AttachedJoystick attachedStick = data.Item2;
            DCSData dcsData = data.Item3;
            GameAircraft currentAircraft = data.Item4;

            this.DataContext = new ViewJoystickViewModel(game, attachedStick, dcsData, currentAircraft);
        }

        public ViewJoystickViewModel ViewModel => (ViewJoystickViewModel)DataContext;

        private async void JoystickImage_Loaded(object sender, RoutedEventArgs e)
        {
            await ImageSourceUtil.SetSourceFromGameJoystick(JoystickImage, ViewModel.Stick);
        }

        private void Expand_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.CurrentScale = ViewModel.Scales[Math.Max(CurrentScaleIndex() - 1, 0)];
        }

        private void ScaleCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Vector3 newScale = ScaleValues[CurrentScaleIndex()];
            if (imageActualWidth > 0)
            {
                JoystickScaleGrid.RowDefinitions[0].Height = new GridLength(imageActualHeight * newScale.X, GridUnitType.Pixel);
                JoystickScaleGrid.ColumnDefinitions[0].Width = new GridLength(imageActualWidth * newScale.X, GridUnitType.Pixel);
            }
            else
            {
                //  Set to a default
                JoystickScaleGrid.RowDefinitions[0].Height = new GridLength();
                JoystickScaleGrid.ColumnDefinitions[0].Width = new GridLength();
            }
            JoystickImage.Scale = newScale;
            //            JoystickCanvas.Scale = newScale;
        }

        private void Shrink_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.CurrentScale = ViewModel.Scales[Math.Min(CurrentScaleIndex() + 1, ViewModel.Scales.Length - 1)];
        }

        private int CurrentScaleIndex()
        {
            for (var i = 0; i < ViewModel.Scales.Length; i++)
            {
                if (ViewModel.Scales[i] == ViewModel.CurrentScale)
                {
                    return i;
                }
            }

            return 2;   //  Default to 100%
        }

        private void JoystickImage_ImageOpened(object sender, RoutedEventArgs e)
        {
            imageActualHeight = ((Image)sender).ActualHeight;
            imageActualWidth = ((Image)sender).ActualWidth;

            Vector3 newScale = ScaleValues[CurrentScaleIndex()];
            JoystickScaleGrid.RowDefinitions[0].Height = new GridLength(imageActualHeight * newScale.X, GridUnitType.Pixel);
            JoystickScaleGrid.ColumnDefinitions[0].Width = new GridLength(imageActualWidth * newScale.X, GridUnitType.Pixel);
        }


        private void Print_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}
