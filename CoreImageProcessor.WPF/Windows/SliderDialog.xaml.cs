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
    /// Interaktionslogik für ThresholdWindow.xaml
    /// </summary>
    public partial class SliderDialog : Window, INotifyPropertyChanged
    {
        private int _Value;

        public event PropertyChangedEventHandler? PropertyChanged;

        public SliderDialog(string propertyName, int minValue = 0, int maxValue = 255, int initialValue = 0)
        {
            _Value = initialValue;

            MinValue = minValue;
            MaxValue = maxValue;
            PropertyName = propertyName;

            DataContext = this;

            InitializeComponent();
        }

        public string PropertyName
        {
            get;
        }

        public int MinValue
        {
            get;
        }

        public int MaxValue
        {
            get;
        }

        public int Value
        {
            get => _Value;
            set
            {
                if (value != _Value)
                {
                    _Value = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
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
