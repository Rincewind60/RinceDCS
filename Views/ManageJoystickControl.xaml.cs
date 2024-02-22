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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using static System.Runtime.InteropServices.JavaScript.JSType;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RinceDCS.Views
{
    public sealed partial class ManageJoystickControl : UserControl
    {
        public ManageJoystickControl(RinceDCSJoystick stick, RinceDCSGroups groups, DCSData dcsData, RinceDCSAircraft currentAircraft)
        {
            this.InitializeComponent();

            this.DataContext = new ManageJoystickViewModel(stick, groups, dcsData, currentAircraft);
        }

        public ManageJoystickViewModel ViewModel => (ManageJoystickViewModel)DataContext;

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

        private void ButtonsItemsControl_LayoutUpdated(object sender, object e)
        {
            if (JoystickScrollViewer.ZoomFactor != ScaleVMHelper.Default.ZoomFactors[ScaleVMHelper.Default.CurrentScale])
            {
                JoystickScrollViewer.ChangeView(0, 0, ScaleVMHelper.Default.ZoomFactors[ScaleVMHelper.Default.CurrentScale]);
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ManagedButton button = ((ComboBox)sender).DataContext as ManagedButton;
            ViewModel.ButtonGroupChanged(button);
        }
    }
}
