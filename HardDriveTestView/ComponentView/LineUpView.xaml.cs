using System.Windows.Controls;
using HardDriveTestView.ComponentViewModel;
using LabDrivers.Cameras;
using LabDrivers.Stages;

namespace HardDriveTestView.ComponentView
{
    /// <summary>
    /// Interaction logic for LineUpView.xaml
    /// </summary>
    public partial class LineUpView : UserControl
    {
        public LineUpView()
        {
            InitializeComponent();
        }

        public LineUpView(ICamera camera, ITrackingStage stage): this()
        {
            this.DataContext = new LineUpViewModel(camera, stage);
        }
    }
}
