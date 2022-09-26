using CoreImageProcessor.Processing;
using System;
using System.Collections.Generic;
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
    /// Interaktionslogik für FilterProcessingOptionsWindow.xaml
    /// </summary>
    public partial class FilterProcessingOptionsWindow : Window
    {
        public static readonly IEnumerable<KernelSize> AllFilters = Enum.GetValues(typeof(KernelSize)).Cast<KernelSize>();

        public FilterProcessingOptionsWindow(string filterName) : this(filterName, AllFilters)
        {
        }

        public FilterProcessingOptionsWindow(string filterName, IEnumerable<KernelSize> allowedKernelSizes)
        {
            FilterName = filterName;
            
            AllowedKernelSizes = allowedKernelSizes;
            EdgeHandlingOptions = Enum.GetValues(typeof(EdgeHandling)).Cast<EdgeHandling>();

            SelectedKernelSize = AllowedKernelSizes.First();
            SelectedEdgeHandling = EdgeHandlingOptions.First();

            Constant = null;

            InitializeComponent();
            DataContext = this;
        }

        public string FilterName
        {
            get;
        }

        public IEnumerable<KernelSize> AllowedKernelSizes
        {
            get;
        }

        public IEnumerable<EdgeHandling> EdgeHandlingOptions
        {
            get;
        }

        public KernelSize SelectedKernelSize
        {
            get;
            set;
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
