using System;
using System.Diagnostics;
using LabDriveView.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace LabSave.Test
{
    [TestClass]
    public class LabSaveTest
    {
        [TestMethod]
        public void Test()
        {
            var watch = new Stopwatch();
            watch.Start();
            var save = new LabSave("c:\\test\\labsave\\test",123,50);
           
            for (int i = 0; i < 49; i++)
            {
                var frame = new Frame()
                {
                    Time = 1230,
                    FrameWidth = 1024,
                    FrameHeight = 1024,
                    Data = new ushort[1024 * 1024]
                };
                frame.Data.Populate((ushort)50000);
                save.AddOneFrame(frame,false);
            }
            var frame1 = new Frame()
            {
                Time = 1230,
                FrameWidth = 1024,
                FrameHeight = 1024,
                Data = new ushort[1024 * 1024]
            };
            save.AddOneFrame(frame1, true);
            watch.Stop();
            Console.Write(watch.Elapsed.Milliseconds);
        }

        [TestMethod]
        public void TestLoad()
        {
            ILabReader reader = new LabReader();
            reader.ReadAsync("c:\\test\\labsave\\test",a=> { });
        }
    }
}
