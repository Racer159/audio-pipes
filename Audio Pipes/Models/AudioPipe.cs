using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Devices.Enumeration;
using Windows.Media.Audio;
using Windows.Media.Render;
using Windows.Storage;
using Windows.UI.Xaml;

namespace Audio_Pipes.Models
{
    [DataContract]
    public class AudioPipe : INotifyPropertyChanged
    {
        public AudioGraph Graph { get; set; }
        public AudioGraphSettings Settings { get; set; }

        public AudioFileOutputNode SelectedOutputFileNode;
        public DispatcherTimer Timer = new DispatcherTimer();

        public string _timerState;
        private int _seconds = 0;
        private int _minutes = 0;
        private int _hours = 0;
        private bool _isEnabled = false;
        private DeviceInformation _selectedOutputDevice;
        private StorageFile _selectedOutputFile;
        private DeviceInformation _selectedInputDevice;
        private StorageFile _selectedInputFile;
        private ObservableCollection<DeviceInformation> _outputDevices;
        private ObservableCollection<DeviceInformation> _inputDevices;

        public AudioPipe(ObservableCollection<DeviceInformation> outputDevices, ObservableCollection<DeviceInformation> inputDevices)
        {
            this.OutputDevices = outputDevices;
            this.InputDevices = inputDevices;
            this.Settings = new AudioGraphSettings(AudioRenderCategory.Communications);
            this.Settings.QuantumSizeSelectionMode = QuantumSizeSelectionMode.LowestLatency;
            this.Timer.Interval = TimeSpan.FromSeconds(1);
            this.Timer.Tick += (object sender, object e) =>
            {
                _seconds++;
                if (_seconds == 60)
                {
                    _seconds = 0;
                    _minutes++;
                }
                if (_minutes == 60)
                {
                    _minutes = 0;
                    _hours++;
                }

                TimerState = _hours + ":" + _minutes.ToString("00") + ":" + _seconds.ToString("00");
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            if (null != PropertyChanged)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        [DataMember]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                NotifyPropertyChanged("IsEnabled");
                NotifyPropertyChanged("IsDisabled");
            }
        }
        
        public bool IsDisabled { get { return !_isEnabled; } }

        [DataMember]
        public DeviceInformation SelectedOutputDevice
        {
            get { return _selectedOutputDevice; }
            set
            {
                _selectedOutputDevice = value;
                _selectedOutputFile = null;
                SelectedOutputFileNode = null;
                NotifyPropertyChanged("SelectedOutputDevice");
                NotifyPropertyChanged("SelectedOutputFile");
            }
        }

        [DataMember]
        public StorageFile SelectedOutputFile
        {
            get { return _selectedOutputFile; }
            set
            {
                _selectedOutputFile = value;
                _selectedOutputDevice = null;
                SelectedOutputFileNode = null;
                NotifyPropertyChanged("SelectedOutputFile");
                NotifyPropertyChanged("SelectedOutputDevice");
            }
        }

        [DataMember]
        public DeviceInformation SelectedInputDevice
        {
            get { return _selectedInputDevice; }
            set
            {
                _selectedInputDevice = value;
                _selectedInputFile = null;
                NotifyPropertyChanged("SelectedInputDevice");
                NotifyPropertyChanged("SelectedInputFile");
            }
        }

        [DataMember]
        public StorageFile SelectedInputFile
        {
            get { return _selectedInputFile; }
            set
            {
                _selectedInputFile = value;
                _selectedInputDevice = null;
                NotifyPropertyChanged("SelectedInputFile");
                NotifyPropertyChanged("SelectedInputDevice");
            }
        }

        [DataMember]
        public string TimerState
        {
            get { return _timerState; }
            set
            {
                if (value == null)
                {
                    _seconds = 0;
                    _minutes = 0;
                    _hours = 0;
                }

                _timerState = value;
                NotifyPropertyChanged("TimerState");
            }
        }

        [DataMember]
        public ObservableCollection<DeviceInformation> OutputDevices
        {
            get { return _outputDevices; }
            set
            {
                _outputDevices = value;
                NotifyPropertyChanged("OutputDevices");
            }
        }

        [DataMember]
        public ObservableCollection<DeviceInformation> InputDevices
        {
            get { return _inputDevices; }
            set
            {
                _inputDevices = value;
                NotifyPropertyChanged("InputDevices");
            }
        }
    }
}
