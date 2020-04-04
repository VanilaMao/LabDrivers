using LabDriveView.Core;
using LabImage;
namespace HardDriveTestView
{
    public class ProcessOptionsModel : ViewModelBase
    {
 
        public ProcessOptionsModel(ProcessOptions options)
        {
            Options = options;
        }

        // automap can do this, but for simplilification
        public ProcessOptions Options { get; }

        public double ThreshHoldFactor
        {
            get => Options.GetProperty(options => options.ThreshHoldFactor);
            set
            {
                SetProperty(options => options.ThreshHoldFactor, Options, value);
            }
        }

        public ProcessBlobMethod MethodType
        {
            get => Options.GetProperty(options => options.MethodType);
            set
            {
                SetProperty(options => options.MethodType, Options, value);
            }
        }

        public int CastBits
        {
            get => Options.GetProperty(options => options.CastBits);
            set
            {
                SetProperty(options => options.CastBits, Options, value);
            }
        }

        public int MaxArea
        {
            get => Options.GetProperty(options => options.MaxArea);
            set
            {
                SetProperty(options => options.MaxArea, Options, value);
            }
        }

        public int MinArea
        {
            get => Options.GetProperty(options => options.MinArea);
            set
            {
                SetProperty(options => options.MinArea, Options, value);
            }
        }

        public int MaxLength
        {
            get => Options.GetProperty(options => options.MaxLength);
            set
            {
                SetProperty(options => options.MaxLength, Options, value);
            }
        }

        public int MinLength
        {
            get => Options.GetProperty(options => options.MinLength);
            set
            {
                SetProperty(options => options.MinLength, Options, value);
            }
        }

        public bool PickDark
        {
            get => Options.GetProperty(options => options.PickDark);
            set
            {
                SetProperty(options => options.PickDark, Options, value);
            }
        }

        // will ignore ThreshHoldFactor
        public bool AdaptiveThreshHold
        {
            get => Options.GetProperty(options => options.AdaptiveThreshHold);
            set
            {
                SetProperty(options => options.AdaptiveThreshHold, Options, value);
            }
        }

        public bool IsRgbImageAvailable
        {
            get => Options.GetProperty(options => options.IsRgbImageAvailable);
            set
            {
                SetProperty(options => options.IsRgbImageAvailable, Options, value);
            }
        }

        public int CutEdege
        {
            get => Options.GetProperty(options => options.CutEdege);
            set
            {
                SetProperty(options => options.CutEdege, Options, value);
            }
        }
    }
}
