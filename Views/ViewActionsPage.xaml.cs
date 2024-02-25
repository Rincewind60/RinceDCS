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
using Microsoft.UI.Xaml.Markup;
using CommunityToolkit.Mvvm.Messaging;
using RinceDCS.ViewModels.Messages;
using static System.Runtime.InteropServices.JavaScript.JSType;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RinceDCS.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ViewActionsPage : Page
    {
        public ViewActionsPage()
        {
            this.InitializeComponent();
            ViewActionsViewModel vm = new();
            this.DataContext = vm;

            WeakReferenceMessenger.Default.Register<BindingsDataUpdatedMessage>(this, (r, m) =>
            {
                BindingsDataUpdated();
            });
        }

        public ViewActionsViewModel ViewModel => (ViewActionsViewModel)DataContext;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            Tuple<DCSData,RinceDCSAircraft> data = e.Parameter as Tuple<DCSData, RinceDCSAircraft>;

            ViewModel.Initialize(data.Item1, data.Item2);
            ViewModel.IsActive = true;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            ViewModel.IsActive = false;
        }

        private void actionsDataGrid_Sorting(object sender, CommunityToolkit.WinUI.UI.Controls.DataGridColumnEventArgs e)
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

            foreach(var col in actionsDataGrid.Columns)
            {
                if(col.Tag.ToString() != sortColumn)
                {
                    col.SortDirection = null;
                }
            }
        }

        private void BindingsDataUpdated()
        {
            actionsDataGrid.Columns.Clear();

            if (ViewModel.CurrentCategory == null) return;

            actionsDataGrid.Columns.Add(new CommunityToolkit.WinUI.UI.Controls.DataGridTextColumn()
            {
                Header = "Action",
                Binding = new Microsoft.UI.Xaml.Data.Binding { Path = new PropertyPath("ActionName"), Mode = BindingMode.OneWay },
                Tag = "ActionName"
            });

            int joystickIndex = 0;
            foreach (string joystickName in ViewModel.ActionData.JoystickHeadings)
            {
                string bindingName = "Joystick" + joystickIndex.ToString();

                DataGridTemplateColumn column = new() { Header = joystickName, Tag = bindingName  + "Buttons" };

                string Xaml = "<DataTemplate xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" " +
                        "xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">" +
                        "<StackPanel " +
                            "Orientation=\"Horizontal\" " +
                            "Visibility=\"{Binding " + bindingName + "Visible}\">" +
                            "<TextBlock " +
                                "Margin=\"12,0,12,0\" " +
                                "Text=\"{Binding " + bindingName + "Buttons}\">" +
                            "</TextBlock>" +
                        "</StackPanel>" +
                    "</DataTemplate>";

                DataTemplate cellTemplate = XamlReader.Load(Xaml) as DataTemplate;
                column.CellTemplate = cellTemplate;

                actionsDataGrid.Columns.Add(column);
                joystickIndex++;
            }
        }
    }
}