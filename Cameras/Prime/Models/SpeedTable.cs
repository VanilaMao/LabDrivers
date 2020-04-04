
using System;
using System.Collections.Generic;

namespace LabDrivers.Cameras.Prime.Models
{
    public class SpeedTable
    {
        public UInt32 ReadoutPorts { get; set; }
        public UInt32 ReadoutSpeeds { get; set; }
        public List<ReadoutOption> ReadoutOptions { get; }

        public SpeedTable()
        {
            ReadoutOptions = new List<ReadoutOption>();
        }
    }
}
