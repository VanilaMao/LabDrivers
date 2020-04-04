using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using LabImage;

namespace LabSave
{
    public class LabReader : ILabReader
    {

        public void Read(string fileName, Action<double> reportProgress)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }
            FileName = fileName.SplitFileName();
            using (var fs = new StreamReader(File.Open(fileName, FileMode.Open)))
            {
                var header = fs.ReadLine();
                var values = header?.Split('|');
                if (values != null)
                {
                    AviFilterName = values[0];
                    var cacheFileNumber = int.Parse(values[1]);
                    TotalFrames = int.Parse(values[2]);
                    FrameRate = int.Parse(values[3]);
                    BackgroundColor = int.Parse(values[4]);
                    var fileNumber = 0;
                    bool bFirst = true;
                    var directorName = fileName.DirectorName();
                    using (var ftd = new StreamWriter(File.Open(FileName + ".ftd", FileMode.OpenOrCreate)))
                    {
                        for (;cacheFileNumber > 0 && fileNumber < TotalFrames;)
                        {
                            var frames = ReadOneCacheFile(FileName + cacheFileNumber + ".cache");
                            foreach (var frame in frames)
                            {
                                using (var tiffFs = new FileStream(directorName+fileNumber.ToString("0000")+".tif", FileMode.OpenOrCreate))
                                {
                                    tiffFs.Seek(0, SeekOrigin.Begin);
                                    tiffFs.SaveToTiff(frame.FrameWidth, frame.FrameHeight, frame.Data);
                                }
                                ftd.WriteLine(FlattenData(frame,bFirst, TotalFrames));
                                bFirst = false;
                                fileNumber++;
                                var report = fileNumber *1.0 / TotalFrames;
                                reportProgress?.Invoke(report);
                            }
                            cacheFileNumber--;
                        }
                    }  
                }
            }
        }

        public async Task ReadAsync(string fileName, Action<double> reportProgress)
        {
            await Task.Run(() => Read(fileName, reportProgress));
        }

        private string FlattenData(Frame frame, bool bFirst, double totalFrames)
        {
            var frameInfo = $"{frame.Time}|{frame.ImotionX}|{frame.ImotionY}|{frame.IthisPosMotionX}|{frame.IcenterX}|{frame.Switch}|{frame.IlocalFlex}";
            if (bFirst)
            {
                frameInfo = totalFrames + "|" + frameInfo;
            }

            return frameInfo;
        }

        private List<Frame> ReadOneCacheFile(string file)
        {
            using (var fs = new FileStream(file, FileMode.Open))
            using (var stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                try
                {
                    fs.CopyTo(stream);
                    stream.Seek(0,SeekOrigin.Begin);
                    var list =  formatter.Deserialize(stream);
                    return list as List<Frame>;
                }
                catch (SerializationException e)
                {
                    Console.WriteLine("Failed to serialize. Reason: " + e.Message);
                    throw;
                }
            }
        }   

        public string FileName { get; private set; }
        public string AviFilterName { get; private set; }

        public int FrameRate { get; private set; }
        public int TotalFrames { get; private set; }

        public int BackgroundColor { get; private set; }
    }
}
