using CoreImageProcessor.Processing;
using CoreImageProcessor.ViewModels;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace CoreImageProcessor.Windows
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _ViewModel;
        private CancellationTokenSource? _OngoingOperation;

        public MainWindow()
        {
            _ViewModel = new MainWindowViewModel();

            InitializeComponent();
            DataContext = _ViewModel;
        }

        //Path
        //Manipulated image data
        //analytics
        //operations

        #region Event Handler

        //User clicked on the "Exit" menu item
        private void OnOpenFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Title = "Select Image to load...",
                Filter = "Images (*.BMP;*.JPG;*.PNG)|*.BMP;*.JPG;*.PNG|All files (*.*)|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                Multiselect = false,
                CheckFileExists = true,
                CheckPathExists = true
            };

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                LoadFile(dialog.FileName);    
            }
        }

        private async void LoadFile(string path)
        {
            try
            {
                _OngoingOperation = new CancellationTokenSource();

                ImageAnalytics analytics = await ImageAnalytics.AnalyseImageAsync(path, _OngoingOperation.Token);
                ImageEditorViewModel viewModel = new ImageEditorViewModel(analytics, _ViewModel.RemoveImageEditor);

                _ViewModel.ImageEditors.Add(viewModel);
                _ViewModel.SelectedItem = viewModel;
            }
            catch (OperationCanceledException)
            {
                //Do nothing
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show(this, "Selected file couldn't be found :/", App.ApplicationName, MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
                var window = new ErrorWindow(exception, "An error occured while loading the file :/");
                window.ShowDialog();
            }
            finally
            {
                _OngoingOperation?.Dispose();
                _OngoingOperation = null;
            }
        }

        private void ShowNotImplementedWarning(object sender, RoutedEventArgs e) => MessageBox.Show(this, "This feature is not implemented yet :/", "Core Image Processor", MessageBoxButton.OK,
                    MessageBoxImage.Warning);

        private void OnSaveCurrenTab(object sender, RoutedEventArgs e) => _ViewModel.SelectedItem?.Save();

        private void OnSaveCurrenTabAsExternelFile(object sender, RoutedEventArgs e) => _ViewModel.SelectedItem?.Save(false);

        private void OnCloseCurrentTab(object sender, RoutedEventArgs e)
        {
            if (_ViewModel.SelectedItem != null)
                _ViewModel.RemoveImageEditor(_ViewModel.SelectedItem);
        }

        private void OnCloseAllTabs(object sender, RoutedEventArgs e)
        {
            var imageEditors = _ViewModel.ImageEditors.ToArray(); 

            foreach (ImageEditorViewModel imageEditor in imageEditors)
            {
                imageEditor.Close();
            }
        }

        //User clicked on the "About" menu item
        private void OpenAboutWindow(object sender, RoutedEventArgs e) => (new AboutWindow()).ShowDialog();

        //User clicked on the "Settings" menu item
        private void OpenSettings(object sender, RoutedEventArgs e) => (new SettingsWindow()).ShowDialog();

        //User clicked on the "Exit" menu item
        private void ExitApplication(object sender, RoutedEventArgs e) => Close();

        //Main Window is closing
        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var imageEditors = _ViewModel.ImageEditors.ToArray();

            foreach (ImageEditorViewModel imageEditor in imageEditors)
            {
                if (imageEditor.Close())
                    return;
            }
        }

        private void ShowImageInOriginalSize(object sender, RoutedEventArgs e)
        {
            if (_ViewModel.SelectedItem != null)
            {
                _ViewModel.SelectedItem.ImageStretchMode = Stretch.None;
            }
        }

        private void ShowImageInUniformFilledSize(object sender, RoutedEventArgs e)
        {
            if (_ViewModel.SelectedItem != null)
            {
                _ViewModel.SelectedItem.ImageStretchMode = Stretch.Uniform;
            }
        }

        private void ConvertSelectedImageToGray8PixelFormat(object sender, RoutedEventArgs e) => _ViewModel.SelectedItem?.ConvertToGray8();

        private void ConvertSelectedImageToRgb24PixelFormat(object sender, RoutedEventArgs e) => _ViewModel.SelectedItem?.ConvertToRgb24();

        private void AdjustBrightness(object sender, RoutedEventArgs e) => _ViewModel.SelectedItem?.AdjustBrightness();

        private void AdjustContrast(object sender, RoutedEventArgs e) => _ViewModel.SelectedItem?.AdjustContrast();

        private void ConvertToBlackAndWhiteImage(object sender, RoutedEventArgs e) => _ViewModel.SelectedItem?.ConvertToBlackAndWhite();

        private void InvertImage(object sender, RoutedEventArgs e) => _ViewModel.SelectedItem?.Invert();

        private void ApplyToneSeparation(object sender, RoutedEventArgs e) => _ViewModel.SelectedItem?.ApplyToneSeparation();

        private void ApplyMinimumFilter(object sender, RoutedEventArgs e) => _ViewModel.SelectedItem?.ApplyRankOrderFilter(RankOrderFilter.Minimum);

        private void ApplyMaximumFilter(object sender, RoutedEventArgs e) => _ViewModel.SelectedItem?.ApplyRankOrderFilter(RankOrderFilter.Maximum);

        private void ApplyMedianFilter(object sender, RoutedEventArgs e) => _ViewModel.SelectedItem?.ApplyRankOrderFilter(RankOrderFilter.Median);

        private void ApplyLocalHistogramOperation(object sender, RoutedEventArgs e) => _ViewModel.SelectedItem?.ApplyRankOrderFilter(RankOrderFilter.LocalHistogramOperation);

        private void ApplyThreshold(object sender, RoutedEventArgs e) => _ViewModel.SelectedItem?.ApplyThreshold();

        private void ApplyMeanFilter(object sender, RoutedEventArgs e) => _ViewModel.SelectedItem?.ApplyFilter(KernelType.Mean);

        private void ApplyGaussianFilter(object sender, RoutedEventArgs e) => _ViewModel.SelectedItem?.ApplyFilter(KernelType.Gaussian);

        private void ApplyLaplaceFilter(object sender, RoutedEventArgs e) => _ViewModel.SelectedItem?.ApplyFilter(KernelType.Laplace);

        private void ApplySobelBottomFilter(object sender, RoutedEventArgs e) => _ViewModel.SelectedItem?.ApplyFilter(KernelType.Sobel_Bottom);

        private void ApplySobelTopFilter(object sender, RoutedEventArgs e) => _ViewModel.SelectedItem?.ApplyFilter(KernelType.Sobel_Top);

        private void ApplySobelLeftFilter(object sender, RoutedEventArgs e) => _ViewModel.SelectedItem?.ApplyFilter(KernelType.Sobel_Left);

        private void ApplySobelRightFilter(object sender, RoutedEventArgs e) => _ViewModel.SelectedItem?.ApplyFilter(KernelType.Sobel_Right);

        private void ApplyEmbossFilter(object sender, RoutedEventArgs e) => _ViewModel.SelectedItem?.ApplyFilter(KernelType.Emboss);

        private void ApplyOutlineFilter(object sender, RoutedEventArgs e) => _ViewModel.SelectedItem?.ApplyFilter(KernelType.Outline);

        private void ApplySharpenFilter(object sender, RoutedEventArgs e) => _ViewModel.SelectedItem?.ApplyFilter(KernelType.Sharpen);

        private void ApplyCustomFilter(object sender, RoutedEventArgs e) => _ViewModel.SelectedItem?.ApplyCustomFilter();

        private void ApplyMorphologicalFilter(object sender, RoutedEventArgs e) => _ViewModel.SelectedItem?.ApplyMorphologicalFilter();

        private void ApplyCustomMorphologicalFilter(object sender, RoutedEventArgs e) => _ViewModel.SelectedItem?.ApplyCustomMorphologicalFilter();

        private void CalculateSekelteton(object sender, RoutedEventArgs e) => _ViewModel.SelectedItem?.CalculateSekelteton();

        private void Rotate90Degrees(object sender, RoutedEventArgs e) => _ViewModel.SelectedItem?.Rotate90Degrees();

        private void Rotate180Degrees(object sender, RoutedEventArgs e) => _ViewModel.SelectedItem?.Rotate180Degrees();

        private void Rotate270Degrees(object sender, RoutedEventArgs e) => _ViewModel.SelectedItem?.Rotate270Degrees();

        private void MirrorHorizontal(object sender, RoutedEventArgs e) => _ViewModel.SelectedItem?.MirrorHorizontal();

        private void MirrorVertical(object sender, RoutedEventArgs e) => _ViewModel.SelectedItem?.MirrorVertical();

        private void Scale(object sender, RoutedEventArgs e) => _ViewModel.SelectedItem?.Scale();

        #endregion
    }
}