using CoreImageProcessor.Processing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
    /// Interaktionslogik für CustomMorphologicalFilterWindow.xaml
    /// </summary>
    public partial class CustomMorphologicalFilterWindow : Window, INotifyPropertyChanged
    {
        private bool[,] _FilterMask = new bool[,]
        {
            { false, false, false, false, false },
            { false, false, false, false, false },
            { false, false, true , false, false },
            { false, false, false, false, false },
            { false, false, false, false, false }
        };

        public event PropertyChangedEventHandler? PropertyChanged;

        public CustomMorphologicalFilterWindow()
        {
            MorphologicalOperationOptions = Enum.GetValues(typeof(MorphologicalOperation)).Cast<MorphologicalOperation>();
            SelectedMorphologicalOperation = MorphologicalOperation.Dilation;
            EdgeHandlingOptions = Enum.GetValues(typeof(EdgeHandling)).Cast<EdgeHandling>();
            SelectedEdgeHandling = EdgeHandling.Extend;
            Constant = null;
            Threshold = 128;

            DataContext = this;
            InitializeComponent();
        }

        public bool[,] FilterMask => _FilterMask;

        #region ValueAccessors

        #region X = 0;

        public bool FilterMaskValue_0X0
        {
            get => _FilterMask[0, 0];
            set
            {
                if (value != _FilterMask[0, 0])
                {
                    _FilterMask[0, 0] = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterMaskValue_0X0)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterMask)));
                }
            }
        }

        public bool FilterMaskValue_0X1
        {
            get => _FilterMask[0, 1];
            set
            {
                if (value != _FilterMask[0, 1])
                {
                    _FilterMask[0, 1] = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterMaskValue_0X1)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterMask)));
                }
            }
        }

        public bool FilterMaskValue_0X2
        {
            get => _FilterMask[0, 2];
            set
            {
                if (value != _FilterMask[0, 2])
                {
                    _FilterMask[0, 2] = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterMaskValue_0X2)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterMask)));
                }
            }
        }

        public bool FilterMaskValue_0X3
        {
            get => _FilterMask[0, 3];
            set
            {
                if (value != _FilterMask[0, 3])
                {
                    _FilterMask[0, 3] = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterMaskValue_0X3)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterMask)));
                }
            }
        }

        public bool FilterMaskValue_0X4
        {
            get => _FilterMask[0, 4];
            set
            {
                if (value != _FilterMask[0, 4])
                {
                    _FilterMask[0, 4] = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterMaskValue_0X4)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterMask)));
                }
            }
        }

        #endregion

        #region X = 1;

        public bool FilterMaskValue_1X0
        {
            get => _FilterMask[1, 0];
            set
            {
                if (value != _FilterMask[1, 0])
                {
                    _FilterMask[1, 0] = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterMaskValue_1X0)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterMask)));
                }
            }
        }

        public bool FilterMaskValue_1X1
        {
            get => _FilterMask[1, 1];
            set
            {
                if (value != _FilterMask[1, 1])
                {
                    _FilterMask[1, 1] = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterMaskValue_1X1)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterMask)));
                }
            }
        }

        public bool FilterMaskValue_1X2
        {
            get => _FilterMask[1, 2];
            set
            {
                if (value != _FilterMask[1, 2])
                {
                    _FilterMask[1, 2] = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterMaskValue_1X2)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterMask)));
                }
            }
        }

        public bool FilterMaskValue_1X3
        {
            get => _FilterMask[1, 3];
            set
            {
                if (value != _FilterMask[1, 3])
                {
                    _FilterMask[1, 3] = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterMaskValue_1X3)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterMask)));
                }
            }
        }

        public bool FilterMaskValue_1X4
        {
            get => _FilterMask[1, 4];
            set
            {
                if (value != _FilterMask[1, 4])
                {
                    _FilterMask[1, 4] = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterMaskValue_1X4)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterMask)));
                }
            }
        }

        #endregion

        #region X = 2;

        public bool FilterMaskValue_2X0
        {
            get => _FilterMask[2, 0];
            set
            {
                if (value != _FilterMask[2, 0])
                {
                    _FilterMask[2, 0] = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterMaskValue_2X0)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterMask)));
                }
            }
        }

        public bool FilterMaskValue_2X1
        {
            get => _FilterMask[2, 1];
            set
            {
                if (value != _FilterMask[2, 1])
                {
                    _FilterMask[2, 1] = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterMaskValue_2X1)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterMask)));
                }
            }
        }

        public bool FilterMaskValue_2X2
        {
            get => _FilterMask[2, 2];
            set
            {
                if (value != _FilterMask[2, 2])
                {
                    _FilterMask[2, 2] = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterMaskValue_2X2)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterMask)));
                }
            }
        }

        public bool FilterMaskValue_2X3
        {
            get => _FilterMask[2, 3];
            set
            {
                if (value != _FilterMask[2, 3])
                {
                    _FilterMask[2, 3] = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterMaskValue_2X3)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterMask)));
                }
            }
        }

        public bool FilterMaskValue_2X4
        {
            get => _FilterMask[2, 4];
            set
            {
                if (value != _FilterMask[2, 4])
                {
                    _FilterMask[2, 4] = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterMaskValue_2X4)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterMask)));
                }
            }
        }

        #endregion

        #region X = 3;

        public bool FilterMaskValue_3X0
        {
            get => _FilterMask[3, 0];
            set
            {
                if (value != _FilterMask[3, 0])
                {
                    _FilterMask[3, 0] = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterMaskValue_3X0)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterMask)));
                }
            }
        }

        public bool FilterMaskValue_3X1
        {
            get => _FilterMask[3, 1];
            set
            {
                if (value != _FilterMask[3, 1])
                {
                    _FilterMask[3, 1] = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterMaskValue_3X1)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterMask)));
                }
            }
        }

        public bool FilterMaskValue_3X2
        {
            get => _FilterMask[3, 2];
            set
            {
                if (value != _FilterMask[3, 2])
                {
                    _FilterMask[3, 2] = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterMaskValue_3X2)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterMask)));
                }
            }
        }

        public bool FilterMaskValue_3X3
        {
            get => _FilterMask[3, 3];
            set
            {
                if (value != _FilterMask[3, 3])
                {
                    _FilterMask[3, 3] = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterMaskValue_3X3)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterMask)));
                }
            }
        }

        public bool FilterMaskValue_3X4
        {
            get => _FilterMask[3, 4];
            set
            {
                if (value != _FilterMask[3, 4])
                {
                    _FilterMask[3, 4] = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterMaskValue_3X4)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterMask)));
                }
            }
        }

        #endregion

        #region X = 4;

        public bool FilterMaskValue_4X0
        {
            get => _FilterMask[4, 0];
            set
            {
                if (value != _FilterMask[4, 0])
                {
                    _FilterMask[4, 0] = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterMaskValue_4X0)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterMask)));
                }
            }
        }

        public bool FilterMaskValue_4X1
        {
            get => _FilterMask[4, 1];
            set
            {
                if (value != _FilterMask[4, 1])
                {
                    _FilterMask[4, 1] = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterMaskValue_4X1)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterMask)));
                }
            }
        }

        public bool FilterMaskValue_4X2
        {
            get => _FilterMask[4, 2];
            set
            {
                if (value != _FilterMask[4, 2])
                {
                    _FilterMask[4, 2] = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterMaskValue_4X2)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterMask)));
                }
            }
        }

        public bool FilterMaskValue_4X3
        {
            get => _FilterMask[4, 3];
            set
            {
                if (value != _FilterMask[4, 3])
                {
                    _FilterMask[4, 3] = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterMaskValue_4X3)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterMask)));
                }
            }
        }

        public bool FilterMaskValue_4X4
        {
            get => _FilterMask[4, 4];
            set
            {
                if (value != _FilterMask[4, 4])
                {
                    _FilterMask[4, 4] = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterMaskValue_4X4)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterMask)));
                }
            }
        }

        #endregion

        #endregion

        public IEnumerable<MorphologicalOperation> MorphologicalOperationOptions
        {
            get;
        }

        public MorphologicalOperation SelectedMorphologicalOperation
        {
            get;
            set;
        }

        public IEnumerable<EdgeHandling> EdgeHandlingOptions
        {
            get;
        }

        public EdgeHandling SelectedEdgeHandling
        {
            get;
            set;
        }

        public byte? Constant
        {
            get;
            set;
        }

        public byte Threshold
        {
            get;
            set;
        }

        private void OkButtonClicked(object sender, RoutedEventArgs e)
        {
            if (SelectedEdgeHandling == EdgeHandling.Constant)
            {
                var dialog = new SliderDialog("Constant");

                if (dialog.ShowDialog() == true)
                {
                    if (dialog.Value > byte.MaxValue)
                        Constant = byte.MaxValue;
                    else if (dialog.Value < byte.MinValue)
                        Constant = byte.MinValue;
                    else
                        Constant = (byte)dialog.Value;

                    DialogResult = true;
                    Close();
                }
            }
            else
            {
                Constant = null;
                DialogResult = true;
                Close();
            }
        }

        private void CancelButtonClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
