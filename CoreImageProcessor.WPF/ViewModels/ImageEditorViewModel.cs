using CoreImageProcessor.Processing;
using CoreImageProcessor.Windows;
using Microsoft.Win32;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace CoreImageProcessor.ViewModels
{
    internal class ImageEditorViewModel : INotifyPropertyChanged
    {
        private class CloseCommandHandler : ICommand
        {
            private readonly ImageEditorViewModel _editor;
            public event EventHandler? CanExecuteChanged;

            public CloseCommandHandler(ImageEditorViewModel editor)
            {
                _editor = editor;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                _editor.Close();
            }
        }

        private bool _Closed = false;
        private object _CloseLock = new object();

        private SemaphoreSlim _OperationWriteLock = new SemaphoreSlim(1,1);

        private (Task, CancellationTokenSource)? _OngoingOperation = null;

        private readonly Action<ImageEditorViewModel> _RemoveTabAction;
        private bool _Unsaved = false;
        private Stretch _ImageStretch;
        private ScrollBarVisibility _ScrollBarVisibility;
        private ImageAnalyticsViewModel _ImageAnalytics;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ImageEditorViewModel(ImageAnalytics analytics, Action<ImageEditorViewModel> removeTabAction)
        {
            _RemoveTabAction = removeTabAction;
            _ImageAnalytics = new ImageAnalyticsViewModel(analytics, true);
            
            _ImageStretch = Stretch.Uniform;
            _ScrollBarVisibility = ScrollBarVisibility.Disabled;

            FileName = analytics.FileName!;
            SourceFile = analytics.FilePath!;
            CloseCommand = new CloseCommandHandler(this);
        }

        public bool IsOperationOngoing { get; private set; } = false;

        public (Task Task, CancellationTokenSource CancellationToken)? OngoingOperation
        {
            get => _OngoingOperation;
            set
            {
                if (_OngoingOperation.HasValue)
                {
                    var operation = OngoingOperation!.Value;

                    if (!operation.Task.IsCompleted)
                        operation.CancellationToken.Cancel();
                }

                _OngoingOperation = value;

                bool val = _OngoingOperation.HasValue;

                if (val != IsOperationOngoing)
                {
                    IsOperationOngoing = val;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsOperationOngoing)));
                }   
            }
        }

        public string FileName
        {
            get;
            set;
        }

        public string SourceFile
        {
            get;
        }

        public bool Unsaved
        {
            get => _Unsaved;
            set
            {
                if (value != _Unsaved)
                {
                    _Unsaved = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Unsaved)));
                }
            }
        }

        public Stretch ImageStretchMode
        {
            get => _ImageStretch;
            set
            {
                if (value != _ImageStretch)
                {
                    _ImageStretch = value;

                    ScrollBarVisibility = value == Stretch.None ? ScrollBarVisibility.Auto : ScrollBarVisibility.Disabled;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageStretchMode)));
                }
            }
        }

        public ScrollBarVisibility ScrollBarVisibility
        {
            get => _ScrollBarVisibility;
            set
            {
                if (value != _ScrollBarVisibility)
                {
                    _ScrollBarVisibility = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ScrollBarVisibility)));
                }
            }
        }

        public ImageSource Image => ImageData.Image;

        public ImageSource Histogram => ImageData.Histogram;

        public ImageAnalyticsViewModel ImageData
        {
            get => _ImageAnalytics;
            set
            {
                _ImageAnalytics = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageData)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Image)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Histogram)));

                Unsaved = true;
            }
        }

        private ImageAnalytics Analytics => ImageData.AnalyticsData;

        public ICommand CloseCommand
        {
            get;
        }

        public bool Close()
        {
            lock (_CloseLock)
            {
                if (_Closed)
                    return true;

                if (IsOperationOngoing)
                {
                    var result = MessageBox.Show(
                        messageBoxText: "An operation is currently ongoing. Dou you want to cancel it?",
                        caption: "Core Image Processor (" + SourceFile + ')',
                        button: MessageBoxButton.YesNo,
                        icon: MessageBoxImage.Question,
                        defaultResult: MessageBoxResult.No);

                    if (result != MessageBoxResult.Yes)
                        return false;

                    OngoingOperation = null;
                }

                if (Unsaved)
                {
                    var result = MessageBox.Show(
                        messageBoxText: "Image is not saved. Dou you want to save it before closing it?",
                        caption: "Core Image Processor (" + SourceFile + ')',
                        button: MessageBoxButton.YesNo,
                        icon: MessageBoxImage.Question,
                        defaultResult: MessageBoxResult.No);

                    if (result == MessageBoxResult.Yes)
                    {
                        if (!Save())
                            return false;
                    }
                }

                _RemoveTabAction(this);
                return true;
            }
        }

        public bool Save(bool saveToSource = true)
        {
            if (saveToSource && !Unsaved)
                return true;

            string fileName;

            if (saveToSource && _ImageAnalytics.FilePath != null)
            {
                fileName = _ImageAnalytics.FilePath;
            }
            else
            {
                SaveFileDialog dialog = new SaveFileDialog()
                {
                    Title = "Select Image to load...",
                    Filter = "PNG (*.png)|*.png|JPEG (*.jpg)|*.jpg|Bitmap (*.bmp)|*.bmp",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
                };

                if (dialog.ShowDialog() != true)
                {
                    return false;   
                }

                fileName = dialog.FileName;
            }

            SixLabors.ImageSharp.Image? image = null;
            
            try
            {
                if (Analytics.PixelFormat == PixelFormats.Gray8)
                {
                    image = SixLabors.ImageSharp.Image.LoadPixelData<Gray8>(Analytics.PixelData, Analytics.Width, Analytics.Height);
                }
                else if (Analytics.PixelFormat == PixelFormats.Rgb24)
                {
                    image = SixLabors.ImageSharp.Image.LoadPixelData<Rgb24>(Analytics.PixelData, Analytics.Width, Analytics.Height);
                }
                else
                {
                    ShowPixelFormatNotSupportedMessageBox();
                    return false;
                }

                image.Save(fileName);

                if (saveToSource)
                    Unsaved = false;
                
                return true;
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);

                var window = new ErrorWindow(exception, "Something went wrong while saving image. :/");
                window.ShowDialog();
                return false;
            }
        }

        private class OperationResult
        {
            public OperationResult(byte[] pixelData, PixelFormat pixelFormat, int width, int height, string? fileName = null, string? filePath = null, long? fileSize = null)
            {
                PixelData = pixelData;
                PixelFormat = pixelFormat;
                Width = width;
                Height = height;
                FileName = fileName;
                FilePath = filePath;
                FileSize = fileSize;
            }

            public OperationResult(byte[] pixelData, ImageAnalytics analytics)
            {
                PixelData = pixelData;
                PixelFormat = analytics.PixelFormat;
                Width = analytics.Width;
                Height = analytics.Height;
                FileName = analytics.FileName;
                FilePath = analytics.FilePath;
                FileSize = analytics.FileSize;
            }

            public byte[] PixelData { get; set; }
            public PixelFormat PixelFormat { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public string? FileName { get; set; }
            public string? FilePath { get; set; }
            public long? FileSize { get; set; }
        }

        private void RunOperation<TParameter>(Func<TParameter, CancellationToken, Task<OperationResult>> operation, TParameter parameter)
        {
            async Task OperationKernel(TParameter parameter, CancellationToken cancellationToken)
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    OperationResult result = await operation(parameter, cancellationToken);

                    ImageAnalytics analytics = await ImageAnalytics.AnalyseImageAsync(
                        pixelData: result.PixelData,
                        pixelFormat: result.PixelFormat,
                        width: result.Width,
                        height: result.Height,
                        fileName: result.FileName,
                        filePath: result.FilePath,
                        fileSize: result.FileSize,
                        cancellationToken);

                    ImageData = new ImageAnalyticsViewModel(analytics);
                }
                catch (OperationCanceledException)
                {
                    //Do Nothing
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception);

                    var window = new ErrorWindow(exception, "Something went wrong while running the latest Operation. :/");
                    window.ShowDialog();
                }
                finally
                {
                    await _OperationWriteLock.WaitAsync();

                    try
                    {
                        OngoingOperation = null;
                    }
                    finally
                    {
                        _OperationWriteLock.Release();
                    }   
                }
            }

            _OperationWriteLock.Wait();
            
            try
            {
                if (!IsOperationOngoing)
                {
                    CancellationTokenSource cancellationToken = new CancellationTokenSource();
                    Task task = OperationKernel(parameter, cancellationToken.Token);

                    OngoingOperation = (task, cancellationToken);
                    return;
                }
            }
            finally
            {
                _OperationWriteLock.Release();
            }

            MessageBox.Show(
                messageBoxText: "Unable to manipulate image while an operation is ongoing.",
                caption: App.ApplicationName + " (" + SourceFile + ")",
                button: MessageBoxButton.OK,
                icon: MessageBoxImage.Error);
        }

        public void ShowPixelFormatNotSupportedMessageBox()
        {
            MessageBox.Show(
                    messageBoxText: "Operation is not supported for the pixel format of the current image.",
                    caption: App.ApplicationName + " (" + SourceFile + ")",
                    button: MessageBoxButton.OK,
                    icon: MessageBoxImage.Error);
        }

        public void ConvertToGray8()
        {
            async Task<OperationResult> Operation(object? parameter, CancellationToken cancellationToken)
            {
                byte[] grayscaleImage = await ImageManipulation.ConvertRgb24ToGray8Async(
                    source: Analytics.PixelData,
                    cancellationToken: cancellationToken);

                return new OperationResult(grayscaleImage, Analytics)
                    {
                        PixelFormat = PixelFormats.Gray8
                    };
            }

            if (Analytics.PixelFormat == PixelFormats.Gray8)
                return;

            if (Analytics.PixelFormat == PixelFormats.Rgb24)
            {
                RunOperation<object?>(Operation, null);
            }
            else
            {
                ShowPixelFormatNotSupportedMessageBox();
            }
        }

        public void ConvertToRgb24()
        {
            async Task<OperationResult> Operation(object? parameter, CancellationToken cancellationToken)
            {
                byte[] rgb24Image = await ImageManipulation.ConvertGray8ToRgb24Async(
                    source: Analytics.PixelData,
                    cancellationToken);

                return new OperationResult(rgb24Image, Analytics)
                    {
                        PixelFormat = PixelFormats.Rgb24
                    };
            }

            if (Analytics.PixelFormat == PixelFormats.Rgb24)
                return;

            if (Analytics.PixelFormat == PixelFormats.Gray8)
            {
                RunOperation<object?>(Operation, null);
            }
            else
            {
                ShowPixelFormatNotSupportedMessageBox();
            }
        }

        public void AdjustBrightness()
        {
            async Task<OperationResult> Operation(int value, CancellationToken cancellationToken)
            {
                byte[] image = await ImageManipulation.AdjustBrightnessAsync(
                    source: Analytics.PixelData,
                    value,
                    cancellationToken);

                return new OperationResult(image, Analytics);
            }

            if (Analytics.PixelFormat == PixelFormats.Gray8 ||
                Analytics.PixelFormat == PixelFormats.Rgb24)
            {
                var dialog = new SliderDialog("Brightness", -200, +200);
                bool? result = dialog.ShowDialog();

                if (result == true)
                {
                    RunOperation<int>(Operation, dialog.Value);
                }
            }
            else
            {
                ShowPixelFormatNotSupportedMessageBox();
            }
        }

        public void AdjustContrast()
        {
            async Task<OperationResult> Operation(int factor, CancellationToken cancellationToken)
            {
                byte[] image = await ImageManipulation.AdjustContrastAsync(
                    source: Analytics.PixelData,
                    factor,
                    cancellationToken);

                return new OperationResult(image, Analytics);
            }

            if (Analytics.PixelFormat == PixelFormats.Gray8 ||
                Analytics.PixelFormat == PixelFormats.Rgb24)
            {
                var dialog = new SliderDialog("Contrast", -200, +200);
                bool? result = dialog.ShowDialog();

                if (result == true)
                {
                    RunOperation(Operation, dialog.Value);
                }
            }
            else
            {
                ShowPixelFormatNotSupportedMessageBox();
            }
        }

        public void ConvertToBlackAndWhite()
        {
            async Task<OperationResult> Operation((double readPortion, double greenPortion, double bluePortion, bool convertToGrayscale) parameter, CancellationToken cancellationToken)
            {
                byte[] image = await ImageManipulation.ConvertRgb24ToGray8Async(
                    source: Analytics.PixelData,
                    parameter.readPortion,
                    parameter.greenPortion,
                    parameter.bluePortion,
                    cancellationToken);

                if (!parameter.convertToGrayscale)
                {
                    image = await ImageManipulation.ConvertGray8ToRgb24Async(
                    source: image,
                    cancellationToken);
                }

                return new OperationResult(image, Analytics)
                    {
                        PixelFormat = parameter.convertToGrayscale ? PixelFormats.Gray8 : PixelFormats.Rgb24
                    };
            }

            if (Analytics.PixelFormat == PixelFormats.Rgb24)
            {
                var dialog = new BlackAndWhiteWindow();
                bool? result = dialog.ShowDialog();

                if (result == true)
                {
                    RunOperation<(double, double, double, bool)>(Operation, (dialog.RedValue / 100, dialog.GreenValue / 100, dialog.BlueValue / 100, dialog.ConvertToGrayscale));
                }
            }
            else
            {
                ShowPixelFormatNotSupportedMessageBox();
            }
        }

        public void Invert()
        {
            async Task<OperationResult> Operation(object? parameter, CancellationToken cancellationToken)
            {
                byte[] invertedImage = await ImageManipulation.InvertAsync(
                    source: Analytics.PixelData,
                    cancellationToken);

                return new OperationResult(invertedImage, Analytics);
            }

            if (Analytics.PixelFormat == PixelFormats.Gray8 ||
                Analytics.PixelFormat == PixelFormats.Rgb24)
            {
                RunOperation<object?>(Operation, null);
            }
            else
            {
                ShowPixelFormatNotSupportedMessageBox();
            }
        }

        public void ApplyThreshold()
        {
            async Task<OperationResult> Operation(int threshold, CancellationToken cancellationToken)
            {
                byte[] image = await ImageManipulation.ApplyThresholdAsync(
                    source: Analytics.PixelData,
                    threshold,
                    cancellationToken);

                return new OperationResult(image, Analytics);
            }

            if (Analytics.PixelFormat == PixelFormats.Gray8 ||
                Analytics.PixelFormat == PixelFormats.Rgb24)
            {
                var dialog = new SliderDialog("Threshold");
                bool? result = dialog.ShowDialog();

                if (result == true)
                {
                    RunOperation<int>(Operation, dialog.Value);
                }
            }
            else
            {
                ShowPixelFormatNotSupportedMessageBox();
            }
        }

        public void ApplyToneSeparation()
        {
            async Task<OperationResult> Operation(byte[] lut, CancellationToken cancellationToken)
            {
                byte[] image = await ImageManipulation.ApplyU8LookupTablePerChannelAsync(Analytics.PixelData, lut, cancellationToken);

                return new OperationResult(image, Analytics);
            }

            if (Analytics.PixelFormat == PixelFormats.Gray8 ||
                Analytics.PixelFormat == PixelFormats.Rgb24)
            {
                var dialog = new ToneSeparationOptionsWindow();
                bool? result = dialog.ShowDialog();

                if (result == true)
                { 
                    RunOperation(Operation, dialog.CalculateLookupTable());
                }
            }
            else
            {
                ShowPixelFormatNotSupportedMessageBox();
            }
        }

        public void ApplyFilter(KernelType kernelType)
        {
            async Task<OperationResult> Operation((KernelSize kernelSize, EdgeHandling edgeHandling, byte? constant) parameter, CancellationToken cancellationToken)
            {
                Mapper2D<byte> mapper2D = Mapper2DFactory.CreateU8Instance(Analytics, parameter.edgeHandling, parameter.constant);

                byte[] image = await ImageManipulation.ApplyFilterKernelU8Async(mapper2D, kernelType, parameter.kernelSize, cancellationToken);

                return new OperationResult(image, Analytics);
            }

            if (Analytics.PixelFormat == PixelFormats.Rgb24 ||
                Analytics.PixelFormat == PixelFormats.Gray8)
            {
                var dialog = new FilterProcessingOptionsWindow(kernelType.ToString() + " Filter", ImageUtils.SupportedFilterKernels[kernelType].Keys);
                bool? result = dialog.ShowDialog();

                if (result == true)
                {
                    RunOperation<(KernelSize, EdgeHandling, byte?)>(Operation, (dialog.SelectedKernelSize, dialog.SelectedEdgeHandling, dialog.Constant));
                }
            }
            else
            {
                ShowPixelFormatNotSupportedMessageBox();
            }
        }

        public void ApplyCustomFilter()
        {
            async Task<OperationResult> Operation((float[,] filterKernel, EdgeHandling edgeHandling, byte? constant) parameter, CancellationToken cancellationToken)
            {
                Mapper2D<byte> mapper2D = Mapper2DFactory.CreateU8Instance(Analytics, parameter.edgeHandling, parameter.constant);

                byte[] image = await ImageManipulation.ApplyFilterKernelU8Async(mapper2D, parameter.filterKernel, cancellationToken);

                return new OperationResult(image, Analytics);
            }

            if (Analytics.PixelFormat == PixelFormats.Rgb24 ||
                Analytics.PixelFormat == PixelFormats.Gray8)
            {
                var dialog = new CustomFilterWindow();
                bool? result = dialog.ShowDialog();

                if (result == true)
                {
                    RunOperation<(float[,], EdgeHandling, byte?)>(Operation, (dialog.FilterMask, dialog.SelectedEdgeHandling, dialog.Constant));
                }
            }
            else
            {
                ShowPixelFormatNotSupportedMessageBox();
            }
        }

        public void ApplyRankOrderFilter(RankOrderFilter rankOrderFilter)
        {
            async Task<OperationResult> Operation((KernelSize kernelSize, KernelShape kernelShape, EdgeHandling edgeHandling, byte? constant) parameter, CancellationToken cancellationToken)
            {
                Mapper2D<byte> mapper2D = Mapper2DFactory.CreateU8Instance(Analytics, parameter.edgeHandling, parameter.constant);

                byte[] image = await ImageManipulation.ApplyRankOrderFilterU8Async(mapper2D, rankOrderFilter, parameter.kernelSize, parameter.kernelShape, cancellationToken);

                return new OperationResult(image, Analytics);
            }

            if (Analytics.PixelFormat == PixelFormats.Rgb24 ||
                Analytics.PixelFormat == PixelFormats.Gray8)
            {
                var dialog = new RankOrderFilterProcessingoptionsWindow(rankOrderFilter.ToString() + " Filter");
                bool? result = dialog.ShowDialog();

                if (result == true)
                {


                    RunOperation<(KernelSize, KernelShape, EdgeHandling, byte?)>(Operation, (dialog.SelectedKernelSize, dialog.SelectedKernelShape, dialog.SelectedEdgeHandling, dialog.Constant));
                }
            }
            else
            {
                ShowPixelFormatNotSupportedMessageBox();
            }
        }

        public void ApplyMorphologicalFilter()
        {
            async Task<OperationResult> Operation((MorphologicalOperation morphologicalOperation, KernelSize kernelSize, KernelShape kernelShape , EdgeHandling edgeHandling, byte? constant, byte threshold) parameter, CancellationToken cancellationToken)
            {
                var mapper = new U8C1Mapper2D(Analytics.PixelData, Analytics.Width, Analytics.Height, parameter.edgeHandling, parameter.constant);

                byte[] image = await ImageManipulation.ApplyMorphologicalFilterKernelU8C1Async(
                    mapper, parameter.morphologicalOperation, parameter.kernelShape, parameter.kernelSize, parameter.threshold, cancellationToken);

                return new OperationResult(image,Analytics);
            }

            if (Analytics.PixelFormat == PixelFormats.Gray8)
            {
                var dialog = new MorphologicalFilterProcessingOptionsWindow();
                bool? result = dialog.ShowDialog();

                if (result == true)
                {
                    RunOperation<(MorphologicalOperation, KernelSize, KernelShape, EdgeHandling, byte?, byte)>(Operation, (dialog.SelectedMorphologicalOperation, dialog.SelectedKernelSize, dialog.SelectedKernelShape, dialog.SelectedEdgeHandling, dialog.Constant, dialog.Threshold));
                }   
            }
            else
            {
                ShowPixelFormatNotSupportedMessageBox();
            }
        }

        public void ApplyCustomMorphologicalFilter()
        {
            async Task<OperationResult> Operation((MorphologicalOperation morphologicalOperation, bool[,] kernel, EdgeHandling edgeHandling, byte? constant, byte threshold) parameter, CancellationToken cancellationToken)
            {
                var mapper = new U8C1Mapper2D(Analytics.PixelData, Analytics.Width, Analytics.Height, parameter.edgeHandling, parameter.constant);

                byte[] image = await ImageManipulation.ApplyMorphologicalFilterKernelU8C1Async(
                    mapper, parameter.morphologicalOperation, parameter.kernel, parameter.threshold, cancellationToken);

                return new OperationResult(image, Analytics);
            }

            if (Analytics.PixelFormat == PixelFormats.Gray8)
            {
                var dialog = new CustomMorphologicalFilterWindow();
                bool? result = dialog.ShowDialog();

                if (result == true)
                {
                    RunOperation<(MorphologicalOperation, bool[,], EdgeHandling, byte?, byte)>(Operation, (dialog.SelectedMorphologicalOperation, dialog.FilterMask, dialog.SelectedEdgeHandling, dialog.Constant, dialog.Threshold));
                }
            }
            else
            {
                ShowPixelFormatNotSupportedMessageBox();
            }
        }

        public void CalculateSekelteton()
        {
            async Task<OperationResult> Operation(object? value, CancellationToken cancellationToken)
            {
                return await Task.Run<OperationResult>(() =>
                {
                    var mapper = new U8C1Mapper2D(Analytics.PixelData, Analytics.Width, Analytics.Height, EdgeHandling.Constant, 0);

                    byte[] image = ImageManipulation.CalculateSekelteton(mapper);

                    return new OperationResult(
                        image,
                        Analytics);
                });
            }

            if (Analytics.PixelFormat == PixelFormats.Gray8)
            {
                RunOperation<object?>(Operation, null);
            }
            else
            {
                ShowPixelFormatNotSupportedMessageBox();
            }
        }

        public void Rotate90Degrees()
        {
            async Task<OperationResult> Operation(object? value, CancellationToken cancellationToken)
            {
                byte[] image = await ImageManipulation.Rotate90DegreesU8Async(
                    Analytics.PixelData,
                    Analytics.Width,
                    Analytics.Height,
                    Analytics.ColorChannels.Count,
                    cancellationToken);

                return new OperationResult(
                    image,
                    Analytics.PixelFormat,
                    width: Analytics.Height,
                    height: Analytics.Width,
                    Analytics.FileName,
                    Analytics.FilePath,
                    Analytics.FileSize);
            }

            if (Analytics.PixelFormat == PixelFormats.Gray8 ||
                Analytics.PixelFormat == PixelFormats.Rgb24)
            {
                RunOperation<object?>(Operation, null);
            }
            else
            {
                ShowPixelFormatNotSupportedMessageBox();
            }
        }

        public void Rotate180Degrees()
        {
            async Task<OperationResult> Operation(object? value, CancellationToken cancellationToken)
            {
                byte[] image = await ImageManipulation.Rotate180DegreesU8Async(
                    Analytics.PixelData, 
                    Analytics.Width, 
                    Analytics.Height, 
                    Analytics.ColorChannels.Count,                    
                    cancellationToken);

                return new OperationResult(image, Analytics);
            }

            if (Analytics.PixelFormat == PixelFormats.Gray8 ||
                Analytics.PixelFormat == PixelFormats.Rgb24)
            {
                RunOperation<object?>(Operation, null);
            }
            else
            {
                ShowPixelFormatNotSupportedMessageBox();
            }
        }

        public void Rotate270Degrees()
        {
            async Task<OperationResult> Operation(object? value, CancellationToken cancellationToken)
            {
                byte[] image = await ImageManipulation.Rotate270DegreesU8Async(
                    Analytics.PixelData,
                    Analytics.Width,
                    Analytics.Height,
                    Analytics.ColorChannels.Count,
                    cancellationToken);

                return new OperationResult(
                    image,
                    Analytics.PixelFormat,
                    width: Analytics.Height,
                    height: Analytics.Width,
                    Analytics.FileName,
                    Analytics.FilePath,
                    Analytics.FileSize);
            }

            if (Analytics.PixelFormat == PixelFormats.Gray8 ||
                Analytics.PixelFormat == PixelFormats.Rgb24)
            {
                RunOperation<object?>(Operation, null);
            }
            else
            {
                ShowPixelFormatNotSupportedMessageBox();
            }
        }

        public void MirrorHorizontal()
        {
            async Task<OperationResult> Operation(object? value, CancellationToken cancellationToken)
            {
                byte[] image = await ImageManipulation.MirrorHorizontalU8Async(
                    Analytics.PixelData,
                    Analytics.Width,
                    Analytics.Height,
                    Analytics.ColorChannels.Count,
                    cancellationToken);

                return new OperationResult(image, Analytics);
            }

            if (Analytics.PixelFormat == PixelFormats.Gray8 ||
                Analytics.PixelFormat == PixelFormats.Rgb24)
            {
                RunOperation<object?>(Operation, null);
            }
            else
            {
                ShowPixelFormatNotSupportedMessageBox();
            }
        }

        public void MirrorVertical()
        {
            async Task<OperationResult> Operation(object? value, CancellationToken cancellationToken)
            {
                byte[] image = await ImageManipulation.MirrorVerticalU8Async(
                    Analytics.PixelData,
                    Analytics.Width,
                    Analytics.Height,
                    Analytics.ColorChannels.Count,
                    cancellationToken);

                return new OperationResult(image, Analytics);
            }

            if (Analytics.PixelFormat == PixelFormats.Gray8 ||
                Analytics.PixelFormat == PixelFormats.Rgb24)
            {
                RunOperation<object?>(Operation, null);
            }
            else
            {
                ShowPixelFormatNotSupportedMessageBox();
            }
        }

        public void Scale()
        {
            async Task<OperationResult> Operation((int Width, int Height) newSize, CancellationToken cancellationToken)
            {
                byte[] image = await ImageManipulation.ScaleU8Async(
                    Analytics.PixelData,
                    Analytics.Width,
                    Analytics.Height,
                    newSize.Width,
                    newSize.Height,
                    Analytics.ColorChannels.Count,
                    cancellationToken);

                return new OperationResult(
                    image, 
                    Analytics.PixelFormat, 
                    newSize.Width, 
                    newSize.Height, 
                    Analytics.FileName, 
                    Analytics.FilePath, 
                    Analytics.FileSize);
            }

            if (Analytics.PixelFormat == PixelFormats.Gray8 ||
                Analytics.PixelFormat == PixelFormats.Rgb24)
            {
                var dialog = new ScaleImageWindow(Analytics.Width, Analytics.Height);
                bool? result = dialog.ShowDialog();

                if (result == true)
                {
                    Debug.WriteLine("Width: " + dialog.NewWidth);
                    
                    RunOperation(Operation, (dialog.NewWidth, dialog.NewHeight));
                }
            }
            else
            {
                ShowPixelFormatNotSupportedMessageBox();
            }
        }
    }
}