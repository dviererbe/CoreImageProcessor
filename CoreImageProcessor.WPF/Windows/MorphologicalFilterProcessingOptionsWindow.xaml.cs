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
    /// Interaktionslogik für MorphologicalFilterProcessingOptionsWindow.xaml
    /// </summary>
    public partial class MorphologicalFilterProcessingOptionsWindow : Window
    {
        public MorphologicalFilterProcessingOptionsWindow()
        {
            MorphologicalOperationOptions = Enum.GetValues(typeof(MorphologicalOperation)).Cast<MorphologicalOperation>();
            KernelShapeOptions = Enum.GetValues(typeof(KernelShape)).Cast<KernelShape>();
            EdgeHandlingOptions = Enum.GetValues(typeof(EdgeHandling)).Cast<EdgeHandling>();
            KernelSizeOptions = new[] { KernelSize.KERNEL_SIZE_3_X_3, KernelSize.KERNEL_SIZE_5_X_5, KernelSize.KERNEL_SIZE_7_X_7, KernelSize.KERNEL_SIZE_9_X_9, KernelSize.KERNEL_SIZE_11_X_11, KernelSize.KERNEL_SIZE_13_X_13, KernelSize.KERNEL_SIZE_15_X_15 };

            SelectedMorphologicalOperation = MorphologicalOperation.Dilation;
            SelectedKernelSize = KernelSize.KERNEL_SIZE_3_X_3;
            SelectedKernelShape = KernelShape.Cross;
            SelectedEdgeHandling = EdgeHandling.Wrap;
            Constant = null;
            Threshold = 128;

            DataContext = this;
            InitializeComponent();
        }

        public IEnumerable<MorphologicalOperation> MorphologicalOperationOptions
        {
            get;
        }

        public IEnumerable<KernelSize> KernelSizeOptions
        {
            get;
        }

        public IEnumerable<KernelShape> KernelShapeOptions
        {
            get;
        }

        public IEnumerable<EdgeHandling> EdgeHandlingOptions
        {
            get;
        }

        public MorphologicalOperation SelectedMorphologicalOperation
        {
            get;
            set;
        }

        public KernelSize SelectedKernelSize
        {
            get;
            set;
        }

        public KernelShape SelectedKernelShape
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
