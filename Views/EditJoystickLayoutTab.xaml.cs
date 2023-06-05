// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.DependencyInjection;
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
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Drawing.Text;
using System.Numerics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RinceDCS.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EditJoystickLayoutTab : Page
    {
        private Vector3[] ScaleValues = { new Vector3(4.0F), new Vector3(2.0F), new Vector3(1.0F), new Vector3(0.75F), new Vector3(0.50F), new Vector3(0.25F) };

        private double imageActualHeight = 0;
        private double imageActualWidth = 0;

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
            ViewModel.CurrentScale = ViewModel.Scales[Math.Min(CurrentScaleIndex() + 1, ViewModel.Scales.Count - 1)];
        }

        private int CurrentScaleIndex()
        {
            for (var i = 0; i < ViewModel.Scales.Count; i++)
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

        private void JoystickImage_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if(ViewModel.CurrentButton == null) { isDrawing = false; return; }

            PointerPoint point = GetImageMousePoint(sender, e);

            ViewModel.CurrentButton.TopX = point.Position.X;
            ViewModel.CurrentButton.TopY = point.Position.Y;
            ViewModel.CurrentButton.OnLayout = true;

            isDrawing = true;

            e.Handled = true;
        }

        private void JoystickImage_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (ViewModel.CurrentButton == null || !isDrawing) { return; }

            PointerPoint point = GetImageMousePoint(sender, e);

            if(!point.IsInContact) { isDrawing = false;  return; }

            SetButtonDimensions(ViewModel.CurrentButton, point.Position.X, point.Position.Y);

            e.Handled = true;
        }

        private void JoystickImage_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (ViewModel.CurrentButton == null || !isDrawing) { isDrawing = false; return; }

            PointerPoint point = GetImageMousePoint(sender, e);

            SetButtonDimensions(ViewModel.CurrentButton, point.Position.X, point.Position.Y);
            
            ViewModel.CurrentButton = null;
            isDrawing = false;
            e.Handled = true;
        }

        private void SetButtonDimensions(GameJoystickButton button, double newX, double newY)
        {
            button.Width = Math.Max(0, (int)(newX - button.TopX));
            button.Height = Math.Max(0, (int)(newY - button.TopY));
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

        private async void ExportImage_Click(object sender, RoutedEventArgs e)
        {
            string savePath = await Ioc.Default.GetRequiredService<IDialogService>().OpenPickSaveFile("Image.png", "PNG", ".png");
            if(string.IsNullOrWhiteSpace(savePath)) { return; }

            System.Drawing.Image image = GraphicsUtils.CreateJoystickButtonsImage(ViewModel.Stick.Image, ViewModel.Stick.Buttons, ViewModel.Stick.Font, ViewModel.Stick.FontSize);
            image.Save(savePath, ImageFormat.Png);
        }

        private void PrintImage_Click(object sender, RoutedEventArgs e)
        {
            using (System.Drawing.Printing.PrintDocument printDoc = new())
            {
                printDoc.PrintPage += PrintJoystick;
                printDoc.Print();
            }
        }

        private void PrintJoystick(object o, PrintPageEventArgs e)
        {
            System.Drawing.Image img = GraphicsUtils.CreateJoystickButtonsImage(ViewModel.Stick.Image, ViewModel.Stick.Buttons, ViewModel.Stick.Font, ViewModel.Stick.FontSize);
            System.Drawing.Point loc = new(0, 0);
            e.Graphics.DrawImage(img, loc);
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
}
