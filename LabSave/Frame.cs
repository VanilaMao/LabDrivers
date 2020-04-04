using System;

namespace LabSave
{
    [Serializable]
    public class Frame
    {
        // escape time
        public double Time { get; set; }

        public double MotionPosX { get; set; }

        public double MotionPosY { get; set; }

        public int FrameWidth { get; set; }

        public int FrameHeight { get; set; }

        public ushort[] Data { get; set; }

        public double ImageDistanceMappingToMotionDistance { get; set; }

        // to spupport old image processin, data for caputured biggest worm, 
        // should be removed in the future when new image processing program is ready
        public int ImotionX { get; set; }
        public int ImotionY { get; set; }

        public int IthisPosMotionX { get; set; }

        public int IcenterX { get; set; }

        public int Switch { get; set; }
        public int IlocalFlex { get; set; }

    }
}
