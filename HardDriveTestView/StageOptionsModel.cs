using LabDrivers.Stages;
using LabDriveView.Core;

namespace HardDriveTestView
{
    public class StageOptionsModel: ViewModelBase
    {
        private double _calibrateFactor;
        public StageOptionsModel( TrackingOption options)
        {
            Options = options;
        }
        public TrackingOption Options { get; }


        public double CalibrateFactor
        {
            get => _calibrateFactor;
            set => SetProperty(ref _calibrateFactor, value, nameof(CalibrateFactor));
        }


        public bool XDirectionRightIncrease
        {
            get => Options.GetProperty(options => options.XDirectionRightIncrease);
            set
            {
                SetProperty(options => options.XDirectionRightIncrease, Options, value);
            }
        }
        
        public bool YDirectionDownIncrease
        {
            get => Options.GetProperty(options => options.YDirectionDownIncrease);
            set
            {
                SetProperty(options => options.YDirectionDownIncrease, Options,value);
            }
        }

        public bool HighSolution
        {
            get => Options.GetProperty(options => options.HighSolution);
            set
            {
                SetProperty(options => options.HighSolution, Options, value);
            }
        }

        public int DefaultMovingStep
        {
            get => Options.GetProperty(options => options.DefaultMovingStep);
            set
            {
                SetProperty(options => options.DefaultMovingStep, Options, value);
            }
        }
    }
}
