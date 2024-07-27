// Copyright 2023-2024 Paul Scobell. Subject to the GPL-3.0 license.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RinceDCS.Models;
using RinceDCS.ViewModels;
using System.Collections.Generic;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RinceDCS.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ModifiersDialog : Page
    {
        public ModifiersDialog(List<RinceDCSGroupModifier> modifiers, string defaultModifierName)
        {
            this.InitializeComponent();
            this.DataContext = new ModifiersVM(modifiers, defaultModifierName);
        }

        public ModifiersVM ViewModel => (ModifiersVM)DataContext;

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
