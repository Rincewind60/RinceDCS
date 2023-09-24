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
using CommunityToolkit.WinUI.UI.Controls;

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

            Tuple<DCSData,RinceDCSAircraft> data = e.Parameter as Tuple<DCSData, RinceDCSAircraft>;

            BindingsTableViewModel vm = new(data.Item1, data.Item2);
            this.DataContext = vm;
            vm.IsActive = true;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            ViewModel.IsActive = false;
        }

        public BindingsTableViewModel ViewModel => (BindingsTableViewModel)DataContext;

        public static Visibility IsJoystickColumnVisible(string heading)
        {
            return string.IsNullOrEmpty(heading) ? Visibility.Collapsed : Visibility.Visible;
        }

        private void CategoriesCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void dataGrid_Sorting(object sender, CommunityToolkit.WinUI.UI.Controls.DataGridColumnEventArgs e)
        {
            string sortColumn = e.Column.Tag.ToString();
            if(e.Column.SortDirection == null || e.Column.SortDirection == DataGridSortDirection.Descending)
            {
                ViewModel.UpdateSortColumn(sortColumn, true);
                e.Column.SortDirection = DataGridSortDirection.Ascending;
            }
            else
            {
                ViewModel.UpdateSortColumn(sortColumn, false);
                e.Column.SortDirection= DataGridSortDirection.Descending;
            }

            foreach(var col in dataGrid.Columns)
            {
                if(col.Tag.ToString() != sortColumn)
                {
                    col.SortDirection = null;
                }
            }
        }
    }
}