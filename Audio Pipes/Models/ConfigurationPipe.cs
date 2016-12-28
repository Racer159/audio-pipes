using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Audio_Pipes.Models
{
    [DataContract]
    class ConfigurationPipe
    {
        [DataMember]
        public string SelectedInputDeviceId;
        [DataMember]
        public string SelectedInputFilePath;
        [DataMember]
        public string SelectedOutputDeviceId;
        [DataMember]
        public string SelectedOutputFilePath;
        [DataMember]
        public double Gain;
        [DataMember]
        public string Effect;

        public ConfigurationPipe() { }

        public ConfigurationPipe(string InDev, string InFile, string OutDev, string OutFile)
        {
            SelectedInputDeviceId = InDev;
            SelectedInputFilePath = InFile;
            SelectedOutputDeviceId = OutDev;
            SelectedOutputFilePath = OutFile;
            Gain = 0;
            Effect = "None";
        }

        public ConfigurationPipe(string InDev, string InFile, string OutDev, string OutFile, double GainDB, string SelEffect)
        {
            SelectedInputDeviceId = InDev;
            SelectedInputFilePath = InFile;
            SelectedOutputDeviceId = OutDev;
            SelectedOutputFilePath = OutFile;
            Gain = GainDB;
            Effect = SelEffect;
        }
    }
}
