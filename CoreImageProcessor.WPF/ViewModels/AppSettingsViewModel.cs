using System;
using System.ComponentModel;

namespace CoreImageProcessor.ViewModels
{
    internal class AppSettingsViewModel : INotifyPropertyChanged
    {
        private bool _Unsaved;
        private bool _UseCUDA;
        private int _MaxDegreeOfParalelization;

        public event PropertyChangedEventHandler? PropertyChanged;
        
        public AppSettingsViewModel() : this(AppSettings.Default)   
        {
        }

        public AppSettingsViewModel(AppSettings appSettings)
        {
            _Unsaved = false;
            _UseCUDA = appSettings.UseCUDA;
            _MaxDegreeOfParalelization = appSettings.ThreadLimit;

            PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!e.PropertyName.Equals(nameof(Unsaved)))
            {
                Unsaved = App.Settings.UseCUDA != UseCUDA ||
                          App.Settings.ThreadLimit != MaxDegreeOfParalelization;
            }
        }

        public int ParallelizationLimit => Environment.ProcessorCount;

        public bool UseCUDA
        {
            get => _UseCUDA;
            set
            {
                if (value != _UseCUDA)
                {
                    _UseCUDA = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UseCUDA)));
                }
            }
        }

        public int MaxDegreeOfParalelization
        {
            get => _MaxDegreeOfParalelization;
            set
            {
                if (value != _MaxDegreeOfParalelization)
                {
                    _MaxDegreeOfParalelization = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MaxDegreeOfParalelization)));
                }
            }
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

        public void Save()
        {
            App.Settings = new AppSettings(UseCUDA, MaxDegreeOfParalelization);
            Unsaved = false;
        }
    }
}
