using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using RinceDCS.Models;
using RinceDCS.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

/// TODO: When IsActive changes it is not reflected on other vies as it is not an observable property, should it be, or use messages?

namespace RinceDCS.Views
{
    public sealed partial class EditGroupAircraftControl : UserControl
    {
        public EditGroupAircraftControl(List<string> aircraftNames, Dictionary<string, RinceDCSGroup> groups)
        {
            this.InitializeComponent();

            EditGroupAircraftViewModel vm = new(aircraftNames, groups);

            DataContext = vm;
        }

        public EditGroupAircraftViewModel ViewModel => (EditGroupAircraftViewModel)DataContext;

        private void dataGrid_Sorting(object sender, CommunityToolkit.WinUI.UI.Controls.DataGridColumnEventArgs e)
        {
            /*
            string sortColumn = e.Column.Tag.ToString();
            if (e.Column.SortDirection == null || e.Column.SortDirection == DataGridSortDirection.Descending)
            {
                ViewModel.UpdateSortColumn(sortColumn, true);
                e.Column.SortDirection = DataGridSortDirection.Ascending;
            }
            else
            {
                ViewModel.UpdateSortColumn(sortColumn, false);
                e.Column.SortDirection = DataGridSortDirection.Descending;
            }

            foreach (var col in dataGrid.Columns)
            {
                if (col.Tag.ToString() != sortColumn)
                {
                    col.SortDirection = null;
                }
            }
            */
        }

        private void AircraftCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.CurrentAircraftChanged();

            dataGrid.Columns.Clear();

            if (!ViewModel.IsAircraftSelected) return;

            dataGrid.Columns.Add(new CommunityToolkit.WinUI.UI.Controls.DataGridTextColumn()
            {
                Header = "Groups",
                Binding = new Microsoft.UI.Xaml.Data.Binding { Path = new PropertyPath("Group.Name"), Mode = BindingMode.OneWay }
            });

            DataGridTemplateColumn column = new() { Header = "Status" };
            string Xaml = "<DataTemplate xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" " +
                    "xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">" +
                    "<StackPanel Orientation=\"Horizontal\">" +
                        "<CheckBox " +
                            "Margin=\"12,0,12,0\" " +
                            "Content=\"{Binding Aircraft.CommandName}\" " +
                            "IsChecked=\"{Binding Aircraft.IsActive, Mode=TwoWay}\">" +
                        "</CheckBox>" +
                    "</StackPanel>" +
                "</DataTemplate>";
            DataTemplate cellTemplate = XamlReader.Load(Xaml) as DataTemplate;
            column.CellTemplate = cellTemplate;
            dataGrid.Columns.Add(column);
        }
    }
}
