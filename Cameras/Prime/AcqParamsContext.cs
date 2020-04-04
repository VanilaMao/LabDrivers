

namespace LabDrivers.Cameras.Prime
{
    public struct AcqParamsContext
    {
        public AcqParamsContext(int binning)
        {
            Binning = binning;
        }

        public int Binning { get; }
    }
}
