using LabDrivers.Stages.Models;

namespace LabDrivers.Stages
{
    public interface ITrackingStage
    {
        void MoveX(double xRelativePos);
        void MoveY(double yRelativePos);

        void MoveXAndY(double xRelativePos, double yRelativePos);

        double X { get; }

        double Y { get; }

        double OriginalX { get; }

        double OriginalY { get; }
        bool Open(TrackingOption options,int port,double originalX,double originalY);
        bool Close();

        bool Stop();

        PosPoints GetStageXAndY();

        string Name { get; }

        TrackingOption Options { get;}
    }
}
