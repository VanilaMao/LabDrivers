using System;

namespace LabDrivers.Cameras.Events
{
    public class AcquistionCompletedEventArgs : EventArgs
    {
        public AcquistionCompletedEventArgs(ushort[] frame, int sizeX, int sizeY)
        {
            Frame = frame;
            SizeX = sizeX;
            SizeY = sizeY;
        }

        public ushort[] Frame { get; }

        public int SizeX { get; }

        public int SizeY { get; }
    }
}
