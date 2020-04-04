using System.Windows.Input;
using System.Windows.Media.Imaging;
using LabDrivers.Cameras;
using LabDrivers.Stages;
using LabDriveView.Core;
using LabImage;

namespace HardDriveTestView.ComponentViewModel
{
    public class LineUpViewModel : ViewModelBase
    {
        private int _step;
        private BitmapSource _combinedImage;
        private BitmapSource _wholeImage;
        private int _bits;
        private string _message;
        private bool _speedOption = true;

        // https://stackoverflow.com/questions/14582082/merging-channels-in-opencv
        // https://stackoverflow.com/questions/47700067/emgu-cv-combining-greyscale-images-into-single-bgr-image
        public LineUpViewModel(ICamera camera,ITrackingStage stage)
        {
            Camera = camera;
            Stage = stage;
            Step = 1;
            if (Camera != null)
            {
                Camera.AcquistionCompleted += (o, e) => 
                {
                    Process(e.SizeX, e.SizeY, e.Frame);
                    Camera?.Acquisition();
                };
            }

            StartCommand = new SimpleCommand(Start);
            StopCommand = new SimpleCommand(Stop);
            CloseCommand = new SimpleCommand(Close);
            DebugCommand = new SimpleCommand(DebugProcess);
            StageMoveCommand = new SimpleCommand<StageDirection>(MoveStage);
            Bits =8;
            Message = "Welcome Lineup View!";
            Speed = 200;
        }

        public BitmapSource CombinedImage
        {
            get => _combinedImage;
            set => SetProperty(ref _combinedImage, value, nameof(CombinedImage));
        }

        public BitmapSource WholeImage
        {
            get => _wholeImage;
            set => SetProperty(ref _wholeImage, value, nameof(WholeImage));
        }

        public ICamera Camera { get; }

        public ITrackingStage Stage { get; }

        public ICommand StartCommand { get; }

        public ICommand StageMoveCommand { get; }

        public ICommand StopCommand { get; }
        public ICommand CloseCommand { get; }

        public ICommand DebugCommand { get; }

        public int Step
        {
            get => _step;
            set => SetProperty(ref _step, value, nameof(Step));
        }

        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value, nameof(Message));
        }

        public int Bits
        {
            get => _bits;
            set => SetProperty(ref _bits, value, nameof(Bits));
        }

        public int Speed { get; }

        public bool SpeedOption
        {
            get => _speedOption;
            set => SetProperty(ref _speedOption, value, nameof(SpeedOption));
        }

        private void Process(int sizeX, int sizeY, ushort[] data)
        {
            using (var leftImage = LabImageFactory.From(sizeX, sizeY, data, true, true))
            using (var rightImage = LabImageFactory.From(sizeX, sizeY, data, true))
            using (var wholeImage = LabImageFactory.From(sizeX, sizeY, data))
            {
                wholeImage.GetMinMax(out _, out var max);
                var maxValue = RegulateMax((int)max);
                DrawLines(wholeImage, sizeX, sizeY, maxValue);
                wholeImage.CastTo(ImageType.Bit8Gray,Bits);
                leftImage.CastTo(ImageType.Bit8Gray, Bits);
                rightImage.CastTo(ImageType.Bit8Gray, Bits);
                var combinedImage = LabImageUtilities.Merge(0, leftImage, rightImage);
                var source1 = combinedImage?.ToBitmap(ImageType.Rgb).ToBitmapSource();
                var source2 = wholeImage.ToBitmap(ImageType.Bit8Gray)?.ToBitmapSource();
                source1?.Freeze();
                source2?.Freeze();
                WholeImage = source2;
                CombinedImage = source1;
                combinedImage?.Dispose();
            }
        }
        private void DrawLines(ILabImage image, int sizeX, int sizeY, int color)
        {
            if (Step == 2)
            {
                DrawImageRectangle(image, sizeX / 4, sizeY, color);

                DrawImageRectangle(image, sizeX * 3 / 4, sizeY, color);
            }

            if (Step == 3)
            {
                DrawImageRectangle(image, sizeX / 2, sizeY, color);
            }
        }

        private void DrawImageRectangle(ILabImage image, int left, int sizeY, int color)
        {
            image.DrawImageRectangle(new Roi()
            {
                Left = left,
                Top = 0,
                Width = 2,
                Height = sizeY
            }, ImageType.Bit16Gray, MarkerColor.Default, color);
        }

        private int RegulateMax(int max)
        {
            if (max < 140)
            {
                max = 140;
            }

            if (max > 50000)
            {
                max = 50000;
            }

            return (int) (max * 1.02 + 10);
        }

        private void Start()
        {
            if(Camera?.Start() == true)
            {
                Message = "Camera started";
            }
        }

        private void Close()
        {
            if(Camera?.Close() == true)
            {
                Message = "Camera is Closed";
            }
        }

        private void Stop()
        {
            if(Camera?.Stop() == true)
            {
                Message = "Camera Stopped";
            }
        }

        private void DebugProcess()
        {
            var x = 1200;
            var y = 1200;
            var data = new ushort[x * y];
            data.Populate((ushort)40000);
            Process(x,y,data);
        }

        private bool MoveStage(StageDirection direction)
        {
            var step = SpeedOption ? Speed : Speed / 10;
            switch (direction)
            {
                // from logic stage movement, move to left is tring to decreasing pos
                // and move to top is trying to decreasing pos
                case StageDirection.Left:
                    Stage?.MoveX(-1 * step);
                    break;
                case StageDirection.Right:
                    Stage?.MoveX(step);
                    break;
                case StageDirection.Up:
                    Stage?.MoveY(-1 * step);
                    break;
                case StageDirection.Down:
                    Stage?.MoveY(step);
                    break;
            }
            return true;
        }
    }
}
