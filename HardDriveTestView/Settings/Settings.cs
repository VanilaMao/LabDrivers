using HardDriveTestView.Models;
using LabDrivers.Stages;
using LabImage;
using System;
using System.IO;
using System.Xml.Serialization;

namespace HardDriveTestView.Settings
{
    public class Settings
    {
        private static readonly XmlSerializer Serializer = new XmlSerializer(typeof(Settings));

        private static readonly string XmlFileFolder =Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        public double OriginalX { get; set; }
        public double OriginalY { get; set; }

        public LightScope LightScope { get; set; }

        public TrackingOption TrackingOption { get; set; }

        public ProcessOptions ProcessOptions { get; set; }

        public double CalibrateFactor { get; set; }

        public void SaveSettings()
        {
            var dictory = XmlFileFolder + "\\" + "LabDriver";
            if (!Directory.Exists(dictory))
            {
                Directory.CreateDirectory(dictory);
            }
            using (var writer = new StreamWriter(dictory +"\\"+"LabSettings.xml"))
            {
                Serializer.Serialize(writer, this);
                writer.Flush();
            }
        }

        public static Settings ReadSettings()
        {
            var fileName = XmlFileFolder + "\\" + "LabDriver" + "\\" + "LabSettings.xml";
            if (!File.Exists(fileName))
            {
                return new Settings();
            }
            using (var stream =File.OpenRead(XmlFileFolder + "\\" + "LabDriver"+"\\"+ "LabSettings.xml"))
            {
                var serializer = new XmlSerializer(typeof(Settings));
                return serializer.Deserialize(stream) as Settings;
            }
        }
    }
}
