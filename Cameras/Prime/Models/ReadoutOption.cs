using System;
namespace LabDrivers.Cameras.Prime.Models
{
    public struct ReadoutOption
    {
        public ReadoutOption(short port = 0, short speed = 0, short bitDepth=0,  uint gainStates = 0, string portDesc = null) 
        {
            PortDesc = portDesc;
            BitDepth = bitDepth;
            GainStates = gainStates;
            Port = port;
            Speed = speed;
        }

        public Int16 BitDepth { get;}

        public UInt32 GainStates { get; }
   

        public Int16 Port { get;  }


        public Int16 Speed { get;  }

        public string PortDesc {get; }

        public override string ToString()
        {
            return PortDesc + " Speed: " + Speed + " Bit: " + BitDepth + " Port: " + Port;
        }
    }
}
