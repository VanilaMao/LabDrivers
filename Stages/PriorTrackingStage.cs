using LabDrivers.Stages.Models;
using PRIORLib;
using System;

namespace LabDrivers.Stages
{
    public class PriorTrackingStage : ITrackingStage
    {
        private  IStage _stage;
        private  IScan _scan;

        public PriorTrackingStage(double originalX, double originalY)
        {          
            OriginalX = originalX;
            OriginalY = originalY;
        }

        internal bool Connect()
        {
            try
            {
                _scan = new Scan();
                _stage = new Stage();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public bool IsOpened { get; set; }

        public void MoveX(double xRelativePos)
        {            
            var movement = xRelativePos * (Options.XDirectionRightIncrease ? 1 : -1);
            X += movement;
            _stage.MoveRelative(movement, 0);
        }

        public void MoveY(double yRelativePos)
        {
            var movement = yRelativePos * (Options.YDirectionDownIncrease ? 1 : -1);
            Y += movement;
            _stage.MoveRelative(0, movement);
        }

        public void MoveXAndY(double xRelativePos, double yRelativePos)
        {
            var moveX = xRelativePos * (Options.XDirectionRightIncrease?1:-1);
            var moveY = yRelativePos * (Options.YDirectionDownIncrease ? 1 : -1);
            X += moveX;
            Y += moveY;
            _stage.MoveRelative(moveX, moveY);
        }

        public double X { get; private set; } //the X total realtive distance from orginal pos 
        public double Y { get; private set; }
        public double OriginalX { get; set; }
        public double OriginalY { get; set; }

        public bool Open(TrackingOption options, int port,double originalX, double originalY)
        {
            IsOpened = true;
            OriginalX = originalX;
            OriginalY = originalY;
            Options = options;
            if (_scan.IsConnected <=0 && _scan.Connect(port) != Constants.PRIOR_OK)
            {
                IsOpened = false;
                return false;
                //throw new Exception($"can not open the prior stage via port {port}");
            }
            _stage.MoveToAbsolute(OriginalX, OriginalY);
            return true;
        }

        public bool Close()
        {
            IsOpened = false;
            return Stop() && _scan.DisConnect()>=0;
        }

        public bool Stop()
        {
            X = 0;
            Y = 0;
            _stage.MoveToAbsolute(OriginalX, OriginalY);
            return true;
        }

        public PosPoints GetStageXAndY()
        {
            _stage.GetPosition(out var x, out var y);
            return new PosPoints(x, y);
        }

        public string Name => "Prior Stage";
        public TrackingOption Options { get; private set; }
    }
}
