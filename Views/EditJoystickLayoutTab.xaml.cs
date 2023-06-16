// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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
using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Drawing.Text;
using System.Linq;
using System.Numerics;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RinceDCS.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EditJoystickLayoutTab : Page
    {
        private float[] ZoomFactors = { 4F, 2F, 1F, 0.75F, 0.5F, 0.25F };
        private bool isDrawing = false;

        public EditJoystickLayoutTab()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            GameJoystick joystick = (GameJoystick)e.Parameter;

            InstalledFontCollection fonts = new InstalledFontCollection();
            List<string> fontNames = new();
            foreach (var font in fonts.Families)
            {
                fontNames.Add(font.Name);
            }

            this.DataContext = new EditJoystickViewModel(joystick, fontNames);

            ColorButton.Background = new SolidColorBrush(CommunityToolkit.WinUI.Helpers.ColorHelper.ToColor(joystick.FontColor));
        }

        public EditJoystickViewModel ViewModel => (EditJoystickViewModel)DataContext;

        private void JoystickImage_Loaded(object sender, RoutedEventArgs e)
        {
            SetJoystickImageSource();
        }

        private async void EditImage_Click(object sender, RoutedEventArgs e)
        {
            string newImageFile = await Ioc.Default.GetRequiredService<IDialogService>().OpenPickFile(".png");
            if (!string.IsNullOrEmpty(newImageFile))
            {
                ViewModel.UpdateImage(newImageFile);
                SetJoystickImageSource();
            }
        }

        private async void SetJoystickImageSource()
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
            ViewModel.CurrentScale = Math.Min(ViewModel.CurrentScale + 1, ViewModel.Scales.Count - 1);
        }

        private PointerPoint GetImageMousePoint(object sender, PointerRoutedEventArgs e)
        {
            Pointer pointer = e.Pointer;

            if (pointer.PointerDeviceType != PointerDeviceType.Mouse)
            {
                return null;
            }

            return e.GetCurrentPoint(JoystickImage);
        }

        private void ExportImage_Click(object sender, RoutedEventArgs e)
        {
            JoystickUtil.ExportButtonsImage(ViewModel.Stick.Image, ViewModel.Stick.Buttons.ToList(), ViewModel.Stick.Font, ViewModel.Stick.FontSize);
        }

        private void PrintImage_Click(object sender, RoutedEventArgs e)
        {
            JoystickUtil.PrintButtonsImage(ViewModel.Stick.Image, ViewModel.Stick.Buttons.ToList(), ViewModel.Stick.Font, ViewModel.Stick.FontSize);
        }


        private void ApplyColor_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Stick.FontColor = ColorPicker.Color.ToHex();
            ColorButton.Background = new SolidColorBrush(ColorPicker.Color);
            ColorPickerFlyout.Hide();
        }

        private void CancelColor_Click(object sender, RoutedEventArgs e)
        {
            ColorPickerFlyout.Hide();
        }

        private async void Settings_Click(object sender, RoutedEventArgs e)
        {
            JoystickSettingsPage page = new(ViewModel.Stick.DefaultLabelHeight, ViewModel.Stick.DefaultLabelWidth);
            string stickName = ViewModel.Stick.AttachedJoystick.Name;
            ContentDialogResult result = await Ioc.Default.GetRequiredService<IDialogService>().OpenResponsePageDialog(stickName + " edit Settings", page,"Save",null,null,"Cancel");
            if(result == ContentDialogResult.Primary)
            {
                ViewModel.UpdateSettings(page.ViewModel.DefaultHeight, page.ViewModel.DefaultWidth);
            }
        }

        private void JoystickImage_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (ViewModel.CurrentButton == null) { isDrawing = false; return; }

            PointerPoint point = GetImageMousePoint(sender, e);

            ViewModel.PlaceButtonOnJoystick(ViewModel.CurrentButton, (int)point.Position.X, (int)point.Position.Y);

            isDrawing = true;

            e.Handled = true;
        }

        private void JoystickImage_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (ViewModel.CurrentButton == null || !isDrawing) { return; }

            PointerPoint point = GetImageMousePoint(sender, e);

            if (!point.IsInContact) { isDrawing = false; return; }

            ViewModel.UpdateButtonDimensions(ViewModel.CurrentButton, (int)point.Position.X, (int)point.Position.Y);

            e.Handled = true;
        }

        private void JoystickImage_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (ViewModel.CurrentButton == null || !isDrawing) { isDrawing = false; return; }

            PointerPoint point = GetImageMousePoint(sender, e);

            ViewModel.UpdateButtonDimensions(ViewModel.CurrentButton, (int)point.Position.X, (int)point.Position.Y);

            ViewModel.CurrentButton = null;
            isDrawing = false;
            e.Handled = true;
        }

        private void JoysticButton_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Border border = (Border)sender;

            if (ViewModel.CurrentButton == (GameJoystickButton)border.DataContext &&
                InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.LeftControl) == CoreVirtualKeyStates.Down)
            {
                ViewModel.CurrentButton = null;
            }
            else
            {
                border.Focus(FocusState.Pointer);
            }
        }

        private void JoysticButton_GotFocus(object sender, RoutedEventArgs e)
        {
            GameJoystickButton button = ((Border)sender).DataContext as GameJoystickButton;
            ViewModel.CurrentButton = button;
        }

        private void ButtonsItemsControl_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (ViewModel.CurrentButton == null) return;

            switch (e.Key)
            {
                case Windows.System.VirtualKey.Up:
                    ViewModel.CurrentButton.TopY = Math.Max(ViewModel.CurrentButton.TopY - 1, 0);
                    e.Handled = true;
                    break;
                case Windows.System.VirtualKey.Right:
                    ViewModel.CurrentButton.TopX = ViewModel.CurrentButton.TopX + 1;
                    e.Handled = true;
                    break;
                case Windows.System.VirtualKey.Down:
                    ViewModel.CurrentButton.TopY = ViewModel.CurrentButton.TopY + 1;
                    e.Handled = true;
                    break;
                case Windows.System.VirtualKey.Left:
                    ViewModel.CurrentButton.TopX = Math.Max(ViewModel.CurrentButton.TopX - 1, 0);
                    e.Handled = true;
                    break;
                case Windows.System.VirtualKey.Delete:
                    ViewModel.CurrentButton.OnLayout = false;
                    e.Handled = true;
                    break;
            }

        }
    }

    public class ButtonOnLayoutConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (bool)value == true ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class ButtonOnLayoutColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (bool)value == true ? new SolidColorBrush(Colors.Black) : new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class SelectedButtonBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if(value == null || (bool)value == false)
            {
                return null;
            }
            else
            {
                return new SolidColorBrush(Colors.AliceBlue);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class SelectedButtonBorderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null || (bool)value == false)
            {
                return new SolidColorBrush(Colors.Black);
            }
            else
            {
                return new SolidColorBrush(Colors.Blue);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class AlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value.ToString() == parameter.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            bool isChecked = (bool)value;

            if (isChecked)
            {
                return parameter.ToString();
            }
            else
            {
                return null;
            }
        }
    }
}
