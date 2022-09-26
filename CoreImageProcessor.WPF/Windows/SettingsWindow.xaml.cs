using CoreImageProcessor.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CoreImageProcessor.Windows
{
    /// <summary>
    /// Interaktionslogik für SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            DataContext = new AppSettingsViewModel(App.Settings);
        }

        private void Save()
        {
            AppSettingsViewModel viewModel = (AppSettingsViewModel)DataContext;
            viewModel.Save();
        }

        private void OnOkClicked(object sender, RoutedEventArgs e)
        {
            Save();
            Close();
        }

        private void OnApplyClicked(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void OnCancelCicked(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
