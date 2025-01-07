// Copyright 2023-2024 Paul Scobell. Subject to the GPL-3.0 license.

using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using RinceDCS.Models;
using RinceDCS.Services;
using RinceDCS.ViewModels;
using RinceDCS.ViewModels.Messages;
using RinceDCS.Views.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using Windows.System;
using Windows.UI.Core;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RinceDCS.Views
{
    public sealed partial class EditLayoutsControl : UserControl
    {
        private bool isAddButtonMode = false;
        private bool isMovingButtonMode = false;
        private int movingYOffset = 0;
        private int movingXOffset = 0;

        public EditLayoutsControl(RinceDCSJoystick joystick, RinceDCSJoystickImage stickImage)
        {
            this.InitializeComponent();

            InstalledFontCollection fonts = new InstalledFontCollection();
            List<string> fontNames = new();
            foreach (var font in fonts.Families)
            {
                fontNames.Add(font.Name);
            }

            this.DataContext = new EditLayoutVM(joystick, stickImage, fontNames);

            ColorButton.Background = new SolidColorBrush(CommunityToolkit.WinUI.Helpers.ColorHelper.ToColor(joystick.ButtonFontColor));
        }

        public EditLayoutVM ViewModel => (EditLayoutVM)DataContext;

        private void JoystickImage_Loaded(object sender, RoutedEventArgs e)
        {
            SetJoystickImageSource();
        }

        private async void SelectImage_Click(object sender, RoutedEventArgs e)
        {
            string newImageFile = await DialogService.Default.OpenPickFile(".png");
            if (!string.IsNullOrEmpty(newImageFile))
            {
                ViewModel.UpdateImage(newImageFile);
                SetJoystickImageSource();
            }
        }

        private async void SetJoystickImageSource()
        {
            JoystickImage.Source = await JoystickUtil.GetImageSource(ViewModel.Stick, ViewModel.StickImage);
            ButtonsItemsControl.Width = (JoystickImage.Source as BitmapSource).PixelWidth;
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

        private PointerPoint GetMousePoint(object sender, PointerRoutedEventArgs e)
        {
            Pointer pointer = e.Pointer;

            if (pointer.PointerDeviceType != PointerDeviceType.Mouse)
            {
                return null;
            }

            return e.GetCurrentPoint(sender as UIElement);
        }

        private void ExportImage_Click(object sender, RoutedEventArgs e)
        {
            JoystickUtil.ExportButtonsImage(ViewModel.StickImage.Image, ViewModel.StickImage.Buttons.ToList(), ViewModel.Stick.ButtonHeight, ViewModel.Stick.ButtonWidth, ViewModel.Stick.ButtonFont, ViewModel.Stick.ButtonFontSize);
        }

        private void PrintImage_Click(object sender, RoutedEventArgs e)
        {
            JoystickUtil.PrintButtonsImage(ViewModel.StickImage.Image, ViewModel.StickImage.Buttons.ToList(), ViewModel.Stick.ButtonHeight, ViewModel.Stick.ButtonWidth, ViewModel.Stick.ButtonFont, ViewModel.Stick.ButtonFontSize);
        }


        private void ApplyColor_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Stick.ButtonFontColor = ColorPicker.Color.ToHex();
            ColorButton.Background = new SolidColorBrush(ColorPicker.Color);
            ColorPickerFlyout.Hide();
        }

        private void CancelColor_Click(object sender, RoutedEventArgs e)
        {
            ColorPickerFlyout.Hide();
        }

        private async void Settings_Click(object sender, RoutedEventArgs e)
        {
            JoystickSettingsDialog page = new(ViewModel.StickImage.Title, ViewModel.Stick.ButtonHeight, ViewModel.Stick.ButtonWidth);
            string stickName = ViewModel.Stick.AttachedJoystick.Name;
            Binding settingsBinding = new Binding
            {
                Source = page.ViewModel,
                Path = new PropertyPath("IsValid"),
                Mode = BindingMode.OneWay
            };
            ContentDialogResult result = await DialogService.Default.OpenResponsePageDialog(stickName + " edit Settings", page, "Save", settingsBinding, null, "Cancel");
            if (result == ContentDialogResult.Primary)
            {
                ViewModel.UpdateSettings(page.ViewModel.Title, page.ViewModel.Height, page.ViewModel.Width);
                ///TODO: Update tab title now image title has changed
            }
        }

        private void JoystickImage_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            PointerPoint point = GetMousePoint(sender, e);

            if (isAddButtonMode && point.Properties.IsLeftButtonPressed)
            {
                //  Add button to layout and make current
                ViewModel.CurrentButton = JoystickButtons.SelectedItem as RinceDCSJoystickButton;
                ViewModel.PlaceButtonOnJoystick(ViewModel.CurrentButton, (int)point.Position.X, (int)point.Position.Y);
                JoystickButtons.SelectedItem = null;
            }
            else if(ViewModel.CurrentButton != null && point.Properties.IsRightButtonPressed)
            {
                //  Add a line from current cutton to click location
                ViewModel.DrawLineFromButtonToLocation(ViewModel.CurrentButton, (int)point.Position.X, (int)point.Position.Y);
            }
            else
            {
                ViewModel.CurrentButton = null;
                JoystickImage.Focus(FocusState.Pointer);
            }

            e.Handled = true;
        }

        private void JoystickImage_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!isMovingButtonMode) { return; }

            PointerPoint point = GetMousePoint(sender, e);
            ViewModel.CurrentButton.TopY = (int)point.Position.Y - movingYOffset;
            ViewModel.CurrentButton.TopX = (int)point.Position.X - movingXOffset;
        }

        private void JoystickImage_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            isMovingButtonMode = false;
        }

        private void JoystickImage_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            isMovingButtonMode = false;
        }

        private void JoystickButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(JoystickButtons.SelectedItem == null)
            {
                isAddButtonMode = false;
            }
            else
            {
                isAddButtonMode = true;
            }
        }

        private void JoystickButtons_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            //if (ViewModel.CurrentButton == null) return;

            //switch (e.Key)
            //{
            //    case Windows.System.VirtualKey.Delete:
            //        ViewModel.CurrentButton.OnLayout = false;
            //        e.Handled = true;
            //        break;
            //}
        }

        private void ButtonsItemsControl_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            //if (ViewModel.CurrentButton == null) return;

            //switch (e.Key)
            //{
            //    case Windows.System.VirtualKey.Up:
            //        ViewModel.CurrentButton.TopY = Math.Max(ViewModel.CurrentButton.TopY - 1, 0);
            //        e.Handled = true;
            //        break;
            //    case Windows.System.VirtualKey.Right:
            //        ViewModel.CurrentButton.TopX = ViewModel.CurrentButton.TopX + 1;
            //        e.Handled = true;
            //        break;
            //    case Windows.System.VirtualKey.Down:
            //        ViewModel.CurrentButton.TopY = ViewModel.CurrentButton.TopY + 1;
            //        e.Handled = true;
            //        break;
            //    case Windows.System.VirtualKey.Left:
            //        ViewModel.CurrentButton.TopX = Math.Max(ViewModel.CurrentButton.TopX - 1, 0);
            //        e.Handled = true;
            //        break;
            //    case Windows.System.VirtualKey.Delete:
            //        ViewModel.CurrentButton.OnLayout = false;
            //        e.Handled = true;
            //        break;
            //}
        }

        private void ButtonsItemsControl_LayoutUpdated(object sender, object e)
        {
            if (JoystickScrollViewer.ZoomFactor != ViewModel.ScaleHelper.ZoomFactors[ViewModel.ScaleHelper.CurrentScale])
            {
                JoystickScrollViewer.ChangeView(0, 0, ViewModel.ScaleHelper.ZoomFactors[ViewModel.ScaleHelper.CurrentScale]);
            }
        }

        private void JoysticButton_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            PointerPoint point = GetMousePoint(sender, e);
            Border border = (Border)sender;

            if (ViewModel.CurrentButton == (RinceDCSJoystickButton)border.DataContext)
            {
                if (point.Properties.IsLeftButtonPressed &&
                   InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.LeftControl) == CoreVirtualKeyStates.Down)
                {
                    ViewModel.CurrentButton = null;
                    JoystickImage.Focus(FocusState.Pointer);
                }
                else if (point.Properties.IsRightButtonPressed)
                {
                    ViewModel.HideButtonLine(ViewModel.CurrentButton);
                }
            }
            else
            {
                isMovingButtonMode = true;
                border.Focus(FocusState.Pointer);
                movingXOffset = (int)point.Position.X;
                movingYOffset = (int)point.Position.Y;
                JoystickImage.CapturePointer(e.Pointer);
            }
        }

        private void JoysticButton_GotFocus(object sender, RoutedEventArgs e)
        {
            RinceDCSJoystickButton button = ((Border)sender).DataContext as RinceDCSJoystickButton;
            ViewModel.CurrentButton = button;
        }

        private void JoysticButton_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
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
}
