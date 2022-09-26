using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaktionslogik für BlackAndWhiteDialog.xaml
    /// </summary>
    public partial class BlackAndWhiteWindow : Window, INotifyPropertyChanged
    {
        private bool _ConvertToGrayscale = true;
        private double _RedValue = 21.26;
        private double _GreenValue = 71.52;
        private double _BlueValue = 7.22;

        public event PropertyChangedEventHandler? PropertyChanged;
        //(byte)(0.2126 * source[k] + 0.7152 * source[k + 1] + 0.0722 * source[k + 2]);
        public BlackAndWhiteWindow()
        {
            DataContext = this;

            InitializeComponent();
        }

        public double RedValue
        {
            get => _RedValue;
            set
            {
                if (value != _RedValue)
                {
                    _RedValue = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RedValue)));
                }
            }
        }

        public double GreenValue
        {
            get => _GreenValue;
            set
            {
                if (value != _GreenValue)
                {
                    _GreenValue = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GreenValue)));
                }
            }
        }

        public double BlueValue
        {
            get => _BlueValue;
            set
            {
                if (value != _BlueValue)
                {
                    _BlueValue = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BlueValue)));
                }
            }
        }

        public bool ConvertToGrayscale
        {
            get => _ConvertToGrayscale;
            set
            {
                if (value != _ConvertToGrayscale)
                {
                    _ConvertToGrayscale = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ConvertToGrayscale)));
                }
            }
        }

        private void OkButtonClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButtonClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
