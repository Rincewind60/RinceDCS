// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.DependencyInjection;
using RinceDCS.Models;
using RinceDCS.ServiceModels;
using RinceDCS.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RinceDCS.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GameInstancesPage : Page
    {
        public GameInstancesPage(List<GameInstance> instances)
        {
            this.InitializeComponent();

            this.DataContext = new GameInstancesViewModel(instances);
        }

        public GameInstancesViewModel ViewModel => (GameInstancesViewModel)DataContext;

        private async void SelectGameExePath_Click(object sender, RoutedEventArgs e)
        {
            InstanceData instance = (InstanceData)((Button)sender).DataContext;

            string newPath = await Ioc.Default.GetRequiredService<IDialogService>().OpenPickFile(".exe");

            ViewModel.UpdateGameExePath(instance, newPath);
        }

        private async void SelectSavedGameFolderPath_Click(object sender, RoutedEventArgs e)
        {
            InstanceData instance = (InstanceData)((Button)sender).DataContext;

            string newPath = await Ioc.Default.GetRequiredService<IDialogService>().OpenPickFolder();

            ViewModel.UpdateSavedGameFolderPathh(instance, newPath);
        }

        private void DeleteInstance_Click(object sender, RoutedEventArgs e)
        {
            InstanceData instance = (InstanceData)((Button)sender).DataContext;

            ViewModel.DeleteInstance(instance);
        }

        public static Visibility AreButtonsVisible(bool isHeading)
        {
            if (isHeading)
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }
    }

    public class HideIfHeadingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool isHeading = (bool)value;

            if (isHeading)
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class ShowIfHeadingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool isHeading = (bool)value;

            if (isHeading)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
