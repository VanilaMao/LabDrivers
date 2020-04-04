using HardDriveTestView.Models;
using LabDriveView.Core;

namespace HardDriveTestView
{
    public class LightScopeModel : ViewModelBase
    {
        public LightScopeModel(LightScope lightScope)
        {
            LightScope = lightScope;
        }

        public LightScope LightScope { get; }

        public int LeftMargin
        {
            get => LightScope.GetProperty(scope => scope.LeftMargin);
            set
            {
                SetProperty(scope => scope.LeftMargin, LightScope, value);
            }
        }

        public int RightMargin
        {
            get => LightScope.GetProperty(scope => scope.RightMargin);
            set
            {
                SetProperty(scope => scope.RightMargin, LightScope, value);
            }
        }

        public int TopMargin
        {
            get => LightScope.GetProperty(scope => scope.TopMargin);
            set
            {
                SetProperty(scope => scope.TopMargin, LightScope, value);
            }
        }

        public int BottomMargin
        {
            get => LightScope.GetProperty(scope => scope.BottomMargin);
            set
            {
                SetProperty(scope => scope.BottomMargin, LightScope, value);
            }
        }
    }
}
