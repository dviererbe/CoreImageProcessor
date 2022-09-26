using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Windows.Media;

namespace CoreImageProcessor.ViewModels
{
    internal class MainWindowViewModel : INotifyPropertyChanged
    {
        private bool _IsGray8;
        private bool _IsRgb24;
        private bool _CanManipulateImage;
        private bool _CanSave;
        private ImageEditorViewModel? _SelectedItem;

        private bool _IsImageDisplayedNotStreteched;
        private bool _IsImageDisplayedUniformStreched;

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainWindowViewModel()
        {
            _IsGray8 = false;
            _IsRgb24 = false;
            _CanManipulateImage = false;
            _CanSave = false;
            _SelectedItem = null;
            _IsImageDisplayedNotStreteched = false;
            _IsImageDisplayedUniformStreched = false;

            ImageEditors = new ObservableCollection<ImageEditorViewModel>();
            ImageEditors.CollectionChanged += OnImageEditorsCollectionChanged;
        }

        public bool CanSave
        {
            get => _CanSave;
            set
            {
                if (value != _CanSave)
                {
                    _CanSave = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanSave)));
                }
            }
        }

        public bool IsGray8
        {
            get => _IsGray8;
            set
            {
                if (value != _IsGray8)
                {
                    _IsGray8 = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsGray8)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanManipulateGray8Image)));
                }
            }
        }

        public bool IsRgb24
        {
            get => _IsRgb24;
            set
            {
                if (value != _IsRgb24)
                {
                    _IsRgb24 = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRgb24)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanManipulateRgb24Image)));
                }
            }
        }

        /// <summary>
        /// Gets if the Image can be manipulated and is a grayscale image.
        /// </summary>
        public bool CanManipulateGray8Image => _CanManipulateImage && IsGray8;

        /// <summary>
        /// Gets if the Image can be manipulated and is a color image.
        /// </summary>
        public bool CanManipulateRgb24Image => _CanManipulateImage && IsRgb24;

        public bool CanManipulateImage
        {
            get => _CanManipulateImage;
            set
            {
                if (value != _CanManipulateImage)
                {
                    _CanManipulateImage = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanManipulateImage)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanManipulateGray8Image)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanManipulateRgb24Image)));
                }
            }
        }

        public bool IsImageDisplayedNotStreteched
        {
            get => _IsImageDisplayedNotStreteched;
            set
            {
                if (value != _IsImageDisplayedNotStreteched)
                {
                    _IsImageDisplayedNotStreteched = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsImageDisplayedNotStreteched)));
                }
            }
        }


        public bool IsImageDisplayedUniformStreched
        {
            get => _IsImageDisplayedUniformStreched;
            set
            {
                if (value != _IsImageDisplayedUniformStreched)
                {
                    _IsImageDisplayedUniformStreched = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsImageDisplayedUniformStreched)));
                }
            }
        }

        public bool IsItemSelected => _SelectedItem != null;

        public ImageEditorViewModel? SelectedItem
        {
            get => _SelectedItem;
            set
            {
                bool wasItemSelected = IsItemSelected;

                _SelectedItem = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedItem)));

                if (wasItemSelected != IsItemSelected)
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsItemSelected)));

                CanSave = value != null && value.Unsaved;
                CanManipulateImage = value != null && !value.IsOperationOngoing;
                IsGray8 = value != null && value.ImageData.IsGray8;
                IsRgb24 = value != null && value.ImageData.IsRgb24;
                IsImageDisplayedNotStreteched = value != null && value.ImageStretchMode == Stretch.None;
                IsImageDisplayedUniformStreched = value != null && value.ImageStretchMode == Stretch.Uniform;
            }
        }

        public ObservableCollection<ImageEditorViewModel> ImageEditors
        {
            get;
        }

        public void RemoveImageEditor(ImageEditorViewModel imageEditorViewModel)
        {
            if (ImageEditors.Contains(imageEditorViewModel))
                ImageEditors.Remove(imageEditorViewModel);
        }

        private void OnImageEditorsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add ||
                e.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (var item in e.NewItems)
                {
                    ImageEditorViewModel imageEditorViewModel = (ImageEditorViewModel)item!;

                    imageEditorViewModel.PropertyChanged += OnImageEditorPropertyChanged;
                }
            }
            
            if (e.Action == NotifyCollectionChangedAction.Remove ||
                e.Action == NotifyCollectionChangedAction.Reset ||
                e.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (var item in e.OldItems)
                {
                    ImageEditorViewModel imageEditorViewModel = (ImageEditorViewModel)item!;

                    imageEditorViewModel.PropertyChanged -= OnImageEditorPropertyChanged;
                }
            }
            
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageEditors)));
        }

        private void OnImageEditorPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender == SelectedItem)
            {
                if (e.PropertyName == nameof(ImageEditorViewModel.Unsaved))
                {
                    CanSave = SelectedItem.Unsaved;
                }
                else if(e.PropertyName == nameof(ImageEditorViewModel.IsOperationOngoing))
                {
                    CanManipulateImage = !SelectedItem.IsOperationOngoing;
                }
                else if (e.PropertyName == nameof(ImageEditorViewModel.ImageData))
                {
                    IsGray8 = SelectedItem.ImageData.IsGray8;
                    IsRgb24 = SelectedItem.ImageData.IsRgb24;
                }
                else if (e.PropertyName == nameof(ImageEditorViewModel.ImageStretchMode))
                {
                    IsImageDisplayedNotStreteched = SelectedItem.ImageStretchMode == Stretch.None;
                    IsImageDisplayedUniformStreched = SelectedItem.ImageStretchMode == Stretch.Uniform;
                }
            }
        }
    }
}
