// Copyright 2023-2024 Paul Scobell. Subject to the GPL-3.0 license.

using Microsoft.UI.Xaml.Controls;
using RinceDCS.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RinceDCS.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class JoystickSettingsDialog : Page
    {
        public JoystickSettingsDialog(string title, int height, int width)
        {
            this.InitializeComponent();

            this.DataContext = new JoystickSettingsVM(title, height, width);
        }

        public JoystickSettingsVM ViewModel => (JoystickSettingsVM)DataContext;

        private void Title_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            ViewModel.TitleChanged(Title.Text);
        }
    }
}
