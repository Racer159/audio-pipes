using Audio_Pipes.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Media.Audio;
using Windows.Media.Capture;
using Windows.Media.Devices;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Audio_Pipes
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private ObservableCollection<AudioPipe> audioPipes = new ObservableCollection<AudioPipe>();
        public ObservableCollection<DeviceInformation> outputDevices = new ObservableCollection<DeviceInformation>();
        public ObservableCollection<DeviceInformation> inputDevices = new ObservableCollection<DeviceInformation>();

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is ObservableCollection<AudioPipe>)
            {
                audioPipes = (ObservableCollection<AudioPipe>)e.Parameter;
            }

            this.DataContext = audioPipes;
            AudioPipesGridView.ItemsSource = audioPipes;

            SetInputsAndOutputs();
        }

        private async void SetInputsAndOutputs()
        {
            DeviceInformationCollection outputs = await DeviceInformation.FindAllAsync(MediaDevice.GetAudioRenderSelector());
            DeviceInformationCollection inputs = await DeviceInformation.FindAllAsync(MediaDevice.GetAudioCaptureSelector());

            outputDevices.Clear();
            inputDevices.Clear();

            foreach (DeviceInformation device in outputs)
            {
                outputDevices.Add(device);
            }
            foreach (DeviceInformation device in inputs)
            {
                inputDevices.Add(device);
            }
        }

        private void AddPipe_Click(object sender, RoutedEventArgs e)
        {
            AudioPipe pipe = new AudioPipe(outputDevices, inputDevices);
            audioPipes.Add(pipe);
        }

        private async void EnabledCheckbox_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox && ((CheckBox)sender).Tag is AudioPipe)
            {
                AudioPipe pipe = (AudioPipe)((CheckBox)sender).Tag;
                if (!(await EnableAudioGraph(pipe, (bool)((CheckBox)sender).IsChecked)))
                {
                    ((CheckBox)sender).IsChecked = false;
                }
            }
        }

        private async Task<bool> EnableAudioGraph(AudioPipe pipe, bool enabled)
        {
            if (enabled)
            {
                if ((pipe.SelectedInputDevice != null || pipe.SelectedInputFile != null) &&
                    (pipe.SelectedOutputDevice != null || pipe.SelectedOutputFile != null))
                {
                    pipe.Settings.PrimaryRenderDevice = pipe.SelectedOutputDevice;
                    AudioDeviceOutputNode deviceOutputNode = null;
                    AudioFileOutputNode fileOutputNode = null;
                    AudioDeviceInputNode deviceInputNode = null;
                    AudioFileInputNode fileInputNode = null;

                    CreateAudioGraphResult result = await AudioGraph.CreateAsync(pipe.Settings);
                    if (result.Status != AudioGraphCreationStatus.Success)
                    {
                        await new Windows.UI.Popups.MessageDialog("Unable to create the Audio Graph between " +
                            (pipe.SelectedInputDevice.Name != null ? pipe.SelectedInputDevice.Name : pipe.SelectedInputFile.DisplayName) +
                            " and " +
                            (pipe.SelectedOutputDevice.Name != null ? pipe.SelectedOutputDevice.Name : pipe.SelectedOutputFile.DisplayName) +
                            ".  Please try again.").ShowAsync();

                        return false;
                    }

                    pipe.Graph = result.Graph;

                    if (pipe.SelectedOutputFile == null)
                    {
                        CreateAudioDeviceOutputNodeResult deviceOutputNodeResult = await pipe.Graph.CreateDeviceOutputNodeAsync();
                        if (deviceOutputNodeResult.Status != AudioDeviceNodeCreationStatus.Success)
                        {
                            await new Windows.UI.Popups.MessageDialog("Unable to connect to device: " + pipe.SelectedOutputDevice.Name + ". Please try again.").ShowAsync();
                            return false;
                        }

                        deviceOutputNode = deviceOutputNodeResult.DeviceOutputNode;
                    }
                    else
                    {
                        MediaEncodingProfile fileProfile = CreateMediaEncodingProfile(pipe.SelectedOutputFile);
                        CreateAudioFileOutputNodeResult fileOutputNodeResult = await pipe.Graph.CreateFileOutputNodeAsync(pipe.SelectedOutputFile, fileProfile);

                        if (fileOutputNodeResult.Status != AudioFileNodeCreationStatus.Success)
                        {
                            await new Windows.UI.Popups.MessageDialog("Unable to open file: " + pipe.SelectedOutputFile.DisplayName + ". Please try again.").ShowAsync();
                            return false;
                        }

                        fileOutputNode = fileOutputNodeResult.FileOutputNode;
                        pipe.SelectedOutputFileNode = fileOutputNode;
                    }

                    if (pipe.SelectedInputFile == null)
                    {
                        CreateAudioDeviceInputNodeResult deviceInputNodeResult = await pipe.Graph.CreateDeviceInputNodeAsync(MediaCategory.Other, pipe.Graph.EncodingProperties, pipe.SelectedInputDevice);
                        if (deviceInputNodeResult.Status != AudioDeviceNodeCreationStatus.Success)
                        {
                            await new Windows.UI.Popups.MessageDialog("Unable to connect to device: " + pipe.SelectedInputDevice.Name + ". Please try again.").ShowAsync();
                            return false;
                        }

                        deviceInputNode = deviceInputNodeResult.DeviceInputNode;
                    }
                    else
                    {
                        CreateAudioFileInputNodeResult fileInputNodeResult = await pipe.Graph.CreateFileInputNodeAsync(pipe.SelectedInputFile);

                        if (fileInputNodeResult.Status != AudioFileNodeCreationStatus.Success)
                        {
                            await new Windows.UI.Popups.MessageDialog("Unable to open file: " + pipe.SelectedInputFile.DisplayName + ". Please try again.").ShowAsync();
                            return false;
                        }

                        fileInputNode = fileInputNodeResult.FileInputNode;
                    }

                    if (deviceOutputNode != null && deviceInputNode != null)
                    {
                        try { deviceInputNode.AddOutgoingConnection(deviceOutputNode); }
                        catch { await new Windows.UI.Popups.MessageDialog("Unable to connect audio devices: encoding mismatch. Please try again.").ShowAsync(); return false; }
                    }
                    else if (fileOutputNode != null && deviceInputNode != null)
                    {
                        try { deviceInputNode.AddOutgoingConnection(fileOutputNode); }
                        catch { await new Windows.UI.Popups.MessageDialog("Unable to connect audio device to file: encoding mismatch. Please try again.").ShowAsync(); return false; }
                    }
                    else if (deviceOutputNode != null && fileInputNode != null)
                    {
                        try { fileInputNode.AddOutgoingConnection(deviceOutputNode); }
                        catch { await new Windows.UI.Popups.MessageDialog("Unable to connect audio file to device: encoding mismatch. Please try again.").ShowAsync(); return false; }
                    }
                    else
                    {
                        await new Windows.UI.Popups.MessageDialog("Sorry, but you can't play a file into a file.").ShowAsync();
                        return false;
                    }

                    pipe.Graph.Start();
                    pipe.Timer.Start();

                    return true;
                }
                else
                {
                    await new Windows.UI.Popups.MessageDialog("Please select both an input and an output node.").ShowAsync();
                    return false;
                }
            }
            else
            {
                ClosePipe(pipe);
                return true;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button && ((Button)sender).Tag is AudioPipe)
            {
                AudioPipe pipe = (AudioPipe)((Button)sender).Tag;
                ClosePipe(pipe);
                audioPipes.Remove(pipe);
            }
        }

        private async void OpenInputFile_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.FileTypeFilter.Add(".wav");
            openPicker.FileTypeFilter.Add(".wma");
            openPicker.FileTypeFilter.Add(".mp3");
            StorageFile file = await openPicker.PickSingleFileAsync();

            if (file != null && sender is Button && ((Button)sender).Tag is AudioPipe)
            {
                AudioPipe pipe = (AudioPipe)((Button)sender).Tag;
                pipe.SelectedInputFile = file;
            }
        }

        private async void OpenOutputFile_Click(object sender, RoutedEventArgs e)
        {
            FileSavePicker savePicker = new FileSavePicker();
            savePicker.FileTypeChoices.Add("Pulse Code Modulation", new List<string>() { ".wav" });
            savePicker.FileTypeChoices.Add("Windows Media Audio", new List<string>() { ".wma" });
            savePicker.FileTypeChoices.Add("MPEG Audio Layer-3", new List<string>() { ".mp3" });
            StorageFile file = await savePicker.PickSaveFileAsync();

            if (file != null && sender is Button && ((Button)sender).Tag is AudioPipe)
            {
                AudioPipe pipe = (AudioPipe)((Button)sender).Tag;
                pipe.SelectedOutputFile = file;

                if (StorageApplicationPermissions.FutureAccessList.Entries.Count == StorageApplicationPermissions.FutureAccessList.MaximumItemsAllowed)
                {
                    StorageApplicationPermissions.FutureAccessList.Remove(StorageApplicationPermissions.FutureAccessList.Entries.ElementAt(0).Token);
                }

                StorageApplicationPermissions.FutureAccessList.Add(file, file.Path);
            }
        }

        private MediaEncodingProfile CreateMediaEncodingProfile(StorageFile file)
        {
            switch (file.FileType.ToString().ToLowerInvariant())
            {
                case ".wma":
                    return MediaEncodingProfile.CreateWma(AudioEncodingQuality.High);
                case ".mp3":
                    return MediaEncodingProfile.CreateMp3(AudioEncodingQuality.High);
                case ".wav":
                    return MediaEncodingProfile.CreateWav(AudioEncodingQuality.High);
                default:
                    throw new ArgumentException();
            }
        }

        private async void ClosePipe(AudioPipe pipe)
        {
            try
            {
                if (pipe.Graph != null)
                {
                    if (pipe.SelectedOutputFileNode != null)
                    {
                        TranscodeFailureReason finalizeResult = await pipe.SelectedOutputFileNode.FinalizeAsync();
                        if (finalizeResult != TranscodeFailureReason.None)
                        {
                            return;
                        }
                    }

                    pipe.Graph.Stop();
                    pipe.Timer.Stop();
                    pipe.TimerState = null;
                    pipe.Graph.Dispose();
                    pipe.Graph = null;
                }
            }
            catch
            {

            }
        }

        private void RefreshDevices_Click(object sender, RoutedEventArgs e)
        {
            SetInputsAndOutputs();
            List<DeviceInformation> NewInputsList = inputDevices.ToList();
            List<DeviceInformation> NewOutputsList = outputDevices.ToList();

            foreach (AudioPipe pipe in audioPipes)
            {
                List<DeviceInformation> CurrentInputsList = pipe.InputDevices.ToList();
                List<DeviceInformation> CurrentOutputsList = pipe.OutputDevices.ToList();

                //Inputs
                for (int i = 0; i < NewInputsList.Count; i++)
                {
                    if (CurrentInputsList.Find(delegate (DeviceInformation d) { return d.Id == NewInputsList[i].Id; }) == null)
                    {
                        pipe.InputDevices.Add(NewInputsList[i]);
                    }
                }

                for (int i = 0; i < CurrentInputsList.Count; i++)
                {
                    if (NewInputsList.Find(delegate (DeviceInformation d) { return d.Id == CurrentInputsList[i].Id; }) == null)
                    {
                        pipe.InputDevices.Remove(CurrentInputsList.Find(delegate (DeviceInformation d) { return d.Id == CurrentInputsList[i].Id; }));
                    }
                }

                //Outputs
                for (int i = 0; i < NewOutputsList.Count; i++)
                {
                    if (CurrentOutputsList.Find(delegate (DeviceInformation d) { return d.Id == NewOutputsList[i].Id; }) == null)
                    {
                        pipe.OutputDevices.Add(NewOutputsList[i]);
                    }
                }

                for (int i = 0; i < CurrentOutputsList.Count; i++)
                {
                    if (NewOutputsList.Find(delegate (DeviceInformation d) { return d.Id == CurrentOutputsList[i].Id; }) == null)
                    {
                        pipe.OutputDevices.Remove(CurrentOutputsList.Find(delegate (DeviceInformation d) { return d.Id == CurrentOutputsList[i].Id; }));
                    }
                }
            }
        }

        private async void OpenConfig_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.FileTypeFilter.Add(".pipes");
            StorageFile file = await openPicker.PickSingleFileAsync();

            if (file != null)
            {
                Tuple<List<AudioPipe>, bool> openResult = await ConfigurationHelper.OpenAudioPipes(file, inputDevices.ToList(), outputDevices.ToList());
                List<AudioPipe> tempPipes = openResult.Item1;
                if (!openResult.Item2)
                {
                    await new Windows.UI.Popups.MessageDialog("I did my best, but couldn't find all files and devices. If you want, refresh the device list and try again.").ShowAsync();
                }

                foreach (AudioPipe p in audioPipes)
                {
                    ClosePipe(p);
                }

                audioPipes.Clear();
                foreach (AudioPipe p in tempPipes)
                {
                    audioPipes.Add(p);
                }
            }
        }

        private async void SaveConfig_Click(object sender, RoutedEventArgs e)
        {
            FileSavePicker savePicker = new FileSavePicker();
            savePicker.FileTypeChoices.Add("Audio Pipes Configuration", new List<string>() { ".pipes" });
            StorageFile file = await savePicker.PickSaveFileAsync();

            if (file != null)
            {
                ConfigurationHelper.SaveAudioPipes(audioPipes.ToList(), file);
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(About));
        }

        private async void EnableAll_Click(object sender, RoutedEventArgs e)
        {
            if (sender is AppBarToggleButton)
            {
                bool enable = (bool)((AppBarToggleButton)sender).IsChecked;

                foreach (AudioPipe pipe in audioPipes)
                {
                    if (pipe.IsEnabled != enable)
                    {
                        pipe.IsEnabled = enable;

                        if (!(await EnableAudioGraph(pipe, enable))) {
                            pipe.IsEnabled = false;
                        }
                    }
                }
            }
        }
    }
}
