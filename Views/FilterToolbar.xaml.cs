using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using RinceDCS.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using static System.Runtime.InteropServices.JavaScript.JSType;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.



namespace RinceDCS.Views
{
    /// <summary>
    /// Provides an application wide standard Filter tool bar.
    /// </summary>
    /// 
    /// <usage>
    /// Add FilterControl to a Page or userControl.
    /// 
    /// Set which filter controls are visible by using the Visibility properties: AircraftVisible, CategoriesVisible, GroupsVisible, ShowWithButtons.
    /// 
    /// To respond to a change in a Filter your class should watch the Property Changed notifications raised by the FilterViewModel.
    /// </usage>
    public sealed partial class FilterToolbar : UserControl
    {
        public Visibility AircraftVisible { get { return (Visibility)GetValue(AircraftVisibleProperty); } set { SetValue(AircraftVisibleProperty, value); } }
        public Visibility CategoriesVisible { get { return (Visibility)GetValue(CategoriesVisibleProperty); } set { SetValue(CategoriesVisibleProperty, value); } }
        public Visibility GroupsVisible { get { return (Visibility)GetValue(GroupsVisibleProperty); } set { SetValue(GroupsVisibleProperty, value); } }
        public Visibility WithButtonsVisible { get { return (Visibility)GetValue(WithButtonsVisibleProperty); } set { SetValue(WithButtonsVisibleProperty, value); } }

        public static readonly DependencyProperty AircraftVisibleProperty = DependencyProperty.Register("AircraftVisible", typeof(Visibility), typeof(FilterToolbar), new PropertyMetadata(Visibility.Collapsed));
        public static readonly DependencyProperty CategoriesVisibleProperty = DependencyProperty.Register("CategoriesVisible", typeof(Visibility), typeof(FilterToolbar), new PropertyMetadata(Visibility.Collapsed));
        public static readonly DependencyProperty GroupsVisibleProperty = DependencyProperty.Register("GroupsVisible", typeof(Visibility), typeof(FilterToolbar), new PropertyMetadata(Visibility.Collapsed));
        public static readonly DependencyProperty WithButtonsVisibleProperty = DependencyProperty.Register("WithButtonsVisible", typeof(Visibility), typeof(FilterToolbar), new PropertyMetadata(Visibility.Collapsed));

        public FilterToolbar()
        {
            this.InitializeComponent();

            DataContext = FilterToolbarVM.Default;
        }

        public FilterToolbarVM ViewModel => (FilterToolbarVM)DataContext;
    }
}
