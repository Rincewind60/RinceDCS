// Copyright 2023-2024 Paul Scobell. Subject to the GPL-3.0 license.

using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using RinceDCS.ViewModels;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RinceDCS.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HelpPage : Page
    {
        public HelpPage()
        {
            this.InitializeComponent();

            this.DataContext = new HelpVM();
        }

        public HelpVM ViewModel => (HelpVM)DataContext;

        private void MarkdownText_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            ViewModel.LinkToPage(e.Link);
        }

        private void MarkdownTextBlock_ImageResolving(object sender, ImageResolvingEventArgs e)
        {
            e.Image = new BitmapImage(new Uri(ViewModel.GetAbsolutePath(e.Url)));
            e.Handled = true;
        }
    }
}
