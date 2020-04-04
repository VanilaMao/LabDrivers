
namespace LabDrivers.Cameras.Prime
{
    public class Prime95ContinuousTypeCamera : Prime95Camera<AcquisitionType>
    {
        public Prime95ContinuousTypeCamera(ICameraInfo cameraInfo) : base(cameraInfo)
        {
        }

        public override AcquisitionType Type => AcquisitionType.Continous;

        protected override bool StartAcquisition()
        {
            throw new System.NotImplementedException();
        }

        protected override void AllocateBuffers()
        {
            throw new System.NotImplementedException();
        }

        protected override void UnAllocateBuffers()
        {
            throw new System.NotImplementedException();
        }

        protected override bool StopAcqusition()
        {
            throw new System.NotImplementedException();
        }

        protected override short[] HandleFrame(AcqParamsContext para)
        {
            return null;
        }

        protected override bool SetupCamera()
        {
            throw new System.NotImplementedException();
        }
    }
}
