using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using RinceDCS.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
    public sealed partial class SelectAircraftDialog : Page
    {
        public SelectAircraftDialog(List<string> aircraftNames)
        {
            this.InitializeComponent();

            this.DataContext = new SelectAircraftViewModel(aircraftNames);
        }

        public SelectAircraftViewModel ViewModel => (SelectAircraftViewModel)DataContext;

        private void SelectAircraft_Checked(object sender, RoutedEventArgs e)
        {
            bool allChecked = true;
            foreach(var item in SelectAircraft.Items)
            {
                ListViewItem listViewItem = SelectAircraft.ContainerFromItem(item) as ListViewItem;
                if(listViewItem != null)
                {
                    CheckBox checkbox = listViewItem.FindDescendant<CheckBox>();
                    allChecked &= checkbox.IsChecked.Value;
                }
                else
                {
                    return;
                }
            }

            if(allChecked)
            {
                SelectAll.Checked -= SelectAll_Checked;
                ViewModel.SelectAll = true;
                SelectAll.Checked += SelectAll_Checked;
            }
            else
            {
                SelectAll.Unchecked -= SelectAll_Unchecked;
                ViewModel.SelectAll = false;
                SelectAll.Unchecked += SelectAll_Unchecked;
            }
        }

        private void SelectAircraft_Unchecked(object sender, RoutedEventArgs e)
        {
            bool anyUnchecked = false;
            foreach (var item in SelectAircraft.Items)
            {
                ListViewItem listViewItem = SelectAircraft.ContainerFromItem(item) as ListViewItem;
                if (listViewItem != null)
                {
                    CheckBox checkbox = listViewItem.FindDescendant<CheckBox>();
                    anyUnchecked |= !checkbox.IsChecked.Value;
                }
                else
                {
                    return;
                }
            }

            if (anyUnchecked)
            {
                SelectAll.Unchecked -= SelectAll_Unchecked;
                ViewModel.SelectAll = false;
                SelectAll.Unchecked += SelectAll_Unchecked;
            }
            else
            {
                SelectAll.Checked -= SelectAll_Checked;
                ViewModel.SelectAll = true;
                SelectAll.Checked += SelectAll_Checked;
            }
        }

        private void SelectAll_Checked(object sender, RoutedEventArgs e)
        {
            ViewModel.SelectAllChecked();
        }

        private void SelectAll_Unchecked(object sender, RoutedEventArgs e)
        {
            ViewModel.SelectAllUnchecked();
        }
    }
}
