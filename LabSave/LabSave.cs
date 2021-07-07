using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace LabSave
{
    public class LabSave: ILabSave
    {
        public LabSave(string fileName, int frameRate, int maxFrameAllowed, bool saveToCache = true)
        {
            FileName = fileName;
            FileNameWithoutExtension = fileName.SplitFileName();
            FrameRate = frameRate;
            MaxFrameAllowed = maxFrameAllowed;
            SaveToCache = saveToCache;
            Frames = new List<Frame>();
            CanAddFrame = true;
        }

        // possible race condition on this and save function
        public void AddOneFrame(Frame frame, bool lastFrame)
        {
            TotalFrames++;
            Frames.Add(frame);
            if (!SaveToCache || !CanAddFrame) //waiting for previous saveing compeletion
            {
                return;
            }
            if ((TotalFrames > 0 && TotalFrames % MaxFrameAllowed == 0) || lastFrame)
            {
                CacheFileNumber++;
                var frames = Frames;
                Task.Run(async () => {
                    await WriteCacheToFile(frames);
                });
                if (lastFrame)
                {
                    WriteToFlr();
                }
                else
                {
                    Frames = new List<Frame>(); // not last frame, continue to add frame, need to create new list to accepting new frame
                }
            }
        }

        public void ClearSave()
        {
            Frames.Clear();
            while (CacheFileNumber > 0)
            {
                File.Delete($"{FileNameWithoutExtension}{CacheFileNumber}.cache");
                CacheFileNumber--;
            }
        }

        public void Save(Action completeSave= null)
        {
            CanAddFrame = false;
            if (Frames.Count > 0)
            {
                var frames = Frames;
                Frames = new List<Frame>();
                CacheFileNumber++;
                Task.Run(async () => {
                    await WriteCacheToFile(frames);
                    CacheFileNumber = 0;
                    CanAddFrame = true;
                    completeSave?.Invoke();
                });
            }
            WriteToFlr();
        }


        private void WriteToFlr()
        {
            using (var fs = new StreamWriter(File.Open($"{FileName}", FileMode.OpenOrCreate)))
            {
                fs.Write($"{AviFilterName}|{CacheFileNumber}|{TotalFrames}|{FrameRate}|{BackgroundColor}");
            }
        }
        public async Task WriteCacheToFile(List<Frame> frames)
        {
            using (var fs = File.Open($"{FileNameWithoutExtension}{CacheFileNumber}.cache", FileMode.OpenOrCreate))
            using (var stream = new MemoryStream())
            {
                // probally needs more work
                var formatter = new BinaryFormatter();
                try
                {
                    formatter.Serialize(stream, frames);
                    var data = stream.ToArray();
                    fs.Seek(0, SeekOrigin.Begin);
                    await fs.WriteAsync(data, 0, data.Length);
                    frames.Clear();
                }
                catch (SerializationException e)
                {
                    Console.WriteLine("Failed to serialize. Reason: " + e.Message);
                    throw;
                }
            }
        }

        public void AddFrames(Frame[] frames)
        {
            throw new NotImplementedException();
        }

        private string FileNameWithoutExtension { get; set; }

        public bool SaveToCache { get; }
        public int MaxFrameAllowed { get; }
        public DateTime StartTime { get; set; }
        public string FileName { get; }
        public bool SaveAvi { get; set; }
        public int FrameRate { get; }
        public int TotalFrames { get; private set;}
        public int CacheFileNumber { get; private set; }
        public string AviFilterName { get; set; }
        public int BackgroundColor { get; set; }
        private List<Frame> Frames { get;  set; }

        private bool CanAddFrame { get; set; }
    }
}