using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Windows.Devices.Enumeration;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace Audio_Pipes.Models
{
    class ConfigurationHelper
    {
        private static async void SerializeObject(List<ConfigurationPipe> serializableObject, StorageFile file)
        {
            if (serializableObject == null) { return; }

            try
            {
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(List<ConfigurationPipe>));
                using (MemoryStream stream = new MemoryStream())
                {
                    ser.WriteObject(stream, serializableObject);
                    stream.Position = 0;
                    Stream fileStream = await file.OpenStreamForWriteAsync();
                    stream.WriteTo(fileStream);
                    fileStream.Flush();
                    stream.Dispose();
                    fileStream.Dispose();
                }
            }
            catch
            {
            }
        }

        public static void SaveAudioPipes(List<AudioPipe> audioPipes, StorageFile file)
        {
            List<ConfigurationPipe> configPipes = new List<ConfigurationPipe>();

            foreach (AudioPipe pipe in audioPipes)
            {
                ConfigurationPipe cpipe = new ConfigurationPipe(
                    pipe.SelectedInputDevice != null ? pipe.SelectedInputDevice.Id : null, 
                    pipe.SelectedInputFile != null ? pipe.SelectedInputFile.Path : null,
                    pipe.SelectedOutputDevice != null ? pipe.SelectedOutputDevice.Id : null,
                    pipe.SelectedOutputFile != null ? pipe.SelectedOutputFile.Path : null);

                configPipes.Add(cpipe);
            }

            SerializeObject(configPipes, file);
        } 
        
        private static async Task<List<ConfigurationPipe>> DeSerializeObject(StorageFile file)
        {
            List<ConfigurationPipe> configPipes = new List<ConfigurationPipe>();
            try
            {
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(List<ConfigurationPipe>));
                using (Stream fileStream = await file.OpenStreamForReadAsync())
                {
                    configPipes = (List<ConfigurationPipe>)ser.ReadObject(fileStream);
                    fileStream.Dispose();
                }
            }
            catch
            {
            }

            return configPipes;
        }

        public async static Task<Tuple<List<AudioPipe>,bool>> OpenAudioPipes(StorageFile file, List<DeviceInformation> inputDevices,
            List<DeviceInformation> outputDevices)
        {
            List<ConfigurationPipe> configPipes = await DeSerializeObject(file);
            List<AudioPipe> audioPipes = new List<AudioPipe>();
            bool success = true;

            foreach (ConfigurationPipe cpipe in configPipes)
            {
                AudioPipe pipe = new AudioPipe(new ObservableCollection<DeviceInformation>(outputDevices),
                    new ObservableCollection<DeviceInformation>(inputDevices));
                if (cpipe.SelectedInputDeviceId != null)
                {
                    pipe.SelectedInputDevice = inputDevices.Find(delegate (DeviceInformation d) { return (d.Id == cpipe.SelectedInputDeviceId); });
                    if (pipe.SelectedInputDevice == null) { success = false; }
                }
                else
                {
                    string token = StorageApplicationPermissions.FutureAccessList.Entries.FirstOrDefault(item => item.Metadata == cpipe.SelectedInputFilePath).Token;
                    if (token != null)
                    {
                        pipe.SelectedInputFile = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(token);
                    } else { success = false; }
                }

                if (cpipe.SelectedOutputDeviceId != null)
                {
                    pipe.SelectedOutputDevice = outputDevices.Find(delegate (DeviceInformation d) { return (d.Id == cpipe.SelectedOutputDeviceId); });
                    if (pipe.SelectedOutputDevice == null) { success = false; }
                }
                else
                {
                    string token = StorageApplicationPermissions.FutureAccessList.Entries.FirstOrDefault(item => item.Metadata == cpipe.SelectedOutputFilePath).Token;
                    if (token != null)
                    {
                        pipe.SelectedOutputFile = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(token);
                    } else { success = false; }
                }

                audioPipes.Add(pipe);
            }

            return new Tuple<List<AudioPipe>,bool>(audioPipes, success);
        }
    }
}
