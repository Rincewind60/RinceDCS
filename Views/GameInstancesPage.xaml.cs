// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using RinceDCS.Models;
using RinceDCS.Services;
using RinceDCS.ViewModels;
using RinceDCS.Views.Utilities;
using System;
using System.Collections.Generic;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RinceDCS.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GameInstancesPage : Page
    {
        public GameInstancesPage(List<RinceDCSInstance> instances)
        {
            this.InitializeComponent();

            this.DataContext = new GameInstancesViewModel(instances);
        }

        public GameInstancesViewModel ViewModel => (GameInstancesViewModel)DataContext;

        private async void SelectGameExePath_Click(object sender, RoutedEventArgs e)
        {
            InstanceData instance = (InstanceData)((Button)sender).DataContext;

            string newPath = await DialogService.Default.OpenPickFile(".exe");

            ViewModel.UpdateGameExePath(instance, newPath);
        }

        private async void SelectSavedGameFolderPath_Click(object sender, RoutedEventArgs e)
        {
            InstanceData instance = (InstanceData)((Button)sender).DataContext;

            string newPath = await DialogService.Default.OpenPickFolder();

            ViewModel.UpdateSavedGameFolderPathh(instance, newPath);
        }

        private void DeleteInstance_Click(object sender, RoutedEventArgs e)
        {
            InstanceData instance = (InstanceData)((Button)sender).DataContext;

            ViewModel.DeleteInstance(instance);
        }

        public static Visibility AreButtonsVisible(bool isHeading)
        {
            return isHeading ? Visibility.Collapsed : Visibility.Visible;
        }

        private void InstanceName_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            TextBoxHandler.EnterKey_SaveBindAndUnfocus(sender, e);
        }
    }

    public class HideIfHeadingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (bool)value == true ? Visibility.Collapsed : Visibility.Visible;
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
            return (bool)value == true ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class StyleRowFontWeight : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (bool)value == true ? FontWeights.Bold : FontWeights.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

}
