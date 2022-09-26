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
    /// Interaktionslogik für ScaleImageWindow.xaml
    /// </summary>
    public partial class ScaleImageWindow : Window, INotifyPropertyChanged
    {
        private double _Ratio;
        private int _NewWidth;
        private int _NewHeight;
        private bool _NewWidthAndHeightLinked = true;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ScaleImageWindow(int width, int height)
        {
            OldWidth = _NewWidth = width;
            OldHeight = _NewHeight = height;
            _Ratio = width / (double)height;

            InitializeComponent();
            DataContext = this;
        }

        public int OldWidth
        {
            get;
        }

        public int OldHeight
        {
            get;
        }

        public int NewWidth
        {
            get => _NewWidth;
            set
            {
                if (value < 1)
                    value = 1;

                if (value != _NewWidth)
                {
                    if (NewWidthAndHeightLinked)
                    {
                        _NewHeight = (int)Math.Round(value / _Ratio);

                        if (_NewHeight < 1)
                            _NewHeight = 1;

                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NewHeight)));
                    }

                    _NewWidth = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NewWidth)));
                }
            }
        }

        public int NewHeight
        {
            get => _NewHeight;
            set
            {
                if (value < 1)
                    value = 1;

                if (value != _NewHeight)
                {
                    if (NewWidthAndHeightLinked)
                    {
                        _NewWidth = (int)Math.Round(value * _Ratio);

                        if (_NewWidth < 1)
                            _NewWidth = 1;

                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NewWidth)));
                    }

                    _NewHeight = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NewHeight)));
                }
            }
        }

        public bool NewWidthAndHeightLinked

        {
            get => _NewWidthAndHeightLinked; 
            set
            {
                if (value != _NewWidthAndHeightLinked)
                {
                    if (value)
                        _Ratio = NewWidth / (double)NewHeight;

                    _NewWidthAndHeightLinked = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NewWidthAndHeightLinked)));
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
