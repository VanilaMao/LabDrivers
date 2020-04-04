namespace LabImage
{
    public class ProcessOptions
    {
        public double ThreshHoldFactor { get; set; }

        public ProcessBlobMethod MethodType { get; set; }

        public int CastBits { get; set; }

        public int MaxArea { get; set; }

        public int MinArea { get; set; }

        public AreaFilterType AreaFilter { get; set; }

        public int MaxLength { get; set; }

        public int MinLength { get; set; }

        public bool PickDark { get; set; }

        // will ignore ThreshHoldFactor
        public bool AdaptiveThreshHold { get; set; }

        public bool IsRgbImageAvailable { get; set; }

        public int CutEdege { get; set; }
    }
}
