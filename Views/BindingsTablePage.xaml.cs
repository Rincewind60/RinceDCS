// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using RinceDCS.Models;
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
    public sealed partial class BindingsTablePage : Page
    {
        public BindingsTablePage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            Tuple<DCSData,GameAircraft> data = e.Parameter as Tuple<DCSData, GameAircraft>;

            this.DataContext = new BindingsTableViewModel(data.Item1, data.Item2);
        }
        public BindingsTableViewModel ViewModel => (BindingsTableViewModel)DataContext;

        public static Visibility IsJoystickColumnVisible(string heading)
        {
            return string.IsNullOrEmpty(heading) ? Visibility.Collapsed : Visibility.Visible;
        }

        private void CategoriesCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}