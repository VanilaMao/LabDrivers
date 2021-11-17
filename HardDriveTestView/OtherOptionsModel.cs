
using HardDriveTestView.Models;
using LabDriveView.Core;

namespace HardDriveTestView
{
    public class OtherOptionsModel : ViewModelBase
    {
        public OtherOptionsModel(OtherOptions options)
        {
            Options = options;
        }

        public OtherOptions Options { get; }

        public bool LocalFlex
        {
            get => Options.GetProperty(options => options.LocalFlex);
            set
            {
                SetProperty(options => options.LocalFlex, Options, value);
            }
        }

        public int LocalFlexNumber
        {
            get => Options.GetProperty(options => options.LocalFlexNumber);
            set
            {
                SetProperty(options => options.LocalFlexNumber, Options, value);
            }
        }

        public bool BinSplit
        {
            get => Options.GetProperty(options => options.BinSplit);
            set
            {
                SetProperty(options => options.BinSplit, Options, value);
            }
        }

        public bool ProcessLeft
        {
            get => Options.GetProperty(options => options.ProcessLeft);
            set
            {
                SetProperty(options => options.ProcessLeft, Options, value);
            }
        }


    }
}
