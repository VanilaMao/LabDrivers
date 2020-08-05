using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using HardDriveTestView.ComponentView;
using LabDrivers.Cameras;
using LabDrivers.Core;
using LabDrivers.Stages;
using LabDrivers.Stages.Models;
using LabDriveView.Core;
using LabImage;
using LabSave;
using Microsoft.Win32;

namespace HardDriveTestView
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly ICameraService _cameraService;
        private ICameraInfo _selectedCamera;
        private BitmapSource _source;
        private BitmapSource _source1;
        private BitmapSource _source2;
        private int _fameCount;
        private double _seconds;
        private double _stageX;
        private double _stageY;
        private int _calibrateMoveStep;
        private bool _trackingObject;
        private bool _isCalibration;
        private bool _isCalibrateObjectMarked;
        private Roi _calibrateRoi;
        private PosPoints? _calibrateStagePos;
        private Roi _currentRoi;
        private PosPoints? _currentStagePos;
        private int _currentImageSizeX;
        private int _currentImageSizeY;
        private bool _saveData;
        private string _savedFileName;
        private bool _localFlex;
        private int _localFlexNum;
        private bool _binSplit;
        private bool _binSplitHandleLeft;
        private bool _isCameraOpened;
        private double _imageSize;

        public MainWindowViewModel()
        {
            IStageService stageService = new StageService();
            Cameras = new ObservableCollection<ICameraInfo>();
            _cameraService = new CameraService();
            _cameraService.FetchCameras().Foreach(x => Cameras.Add(x));
            Stage = stageService.GetStages().FirstOrDefault();
            OpenCameraCommand = new SimpleCommand(OpenCamrea);
            CloseCameraCommand = new SimpleCommand(CloseCamera);
            StartCameraCommand = new SimpleCommand(StartRecord);
            OpenImageCommand = new SimpleCommand(OpenImage);
            ProcessImageCommand = new SimpleCommand(ProcessImage);
            MoveStageCommand = new SimpleCommand<StageDirection>(MoveStage);
            StartCalibrateCommand = new SimpleCommand(StartClibrate);
            StopCalibrateCommand = new SimpleCommand(StopCalibrate);
            MarkCalibrateCommand = new SimpleCommand(MarkCalibrateObject);
            StageXyPosCommand = new SimpleCommand(GetStageXyPos);
            SaveToFileCommand = new SimpleCommand(SaveToFile);
            RefreshDevicesCommand = new SimpleCommand(RefreshDevices);
            MakeXyPosOriginCommand = new SimpleCommand(MakeXyPosOrigin);
            OpenLineUpCommand = new SimpleCommand(OpenLineUp);
            AppSettings = Settings.Settings.ReadSettings();
            Watch = new Stopwatch();
            

            ImageSize = 800;
            LocalFlexNum = 5;

            


            if (AppSettings.ProcessOptions == null)
            {
                AppSettings.ProcessOptions = new ProcessOptions()
                {
                    ThreshHoldFactor = 0.2,
                    CastBits = 5,
                    MethodType = ProcessBlobMethod.FindContour,
                    MinLength = 10,
                    MaxLength = 2000,
                    MinArea = 10,
                    MaxArea = 20000,
                    PickDark = false,
                    IsRgbImageAvailable = true,
                    CutEdege = 0
                };
            }
            ProcessOptions = new ProcessOptionsModel(AppSettings.ProcessOptions);

            if (AppSettings.TrackingOption == null)
            {
                AppSettings.TrackingOption = new TrackingOption()
                {
                    DefaultMovingStep = 500,
                    XDirectionRightIncrease = true,
                    YDirectionDownIncrease = true
                };
            }
            StageOptions= new StageOptionsModel(AppSettings.TrackingOption)
            {
                CalibrateFactor = AppSettings.CalibrateFactor
            };
            
            if(AppSettings.LightScope == null)
            {
                AppSettings.LightScope = new Models.LightScope();
            }
            LightScope = new LightScopeModel(AppSettings.LightScope);

            LightScope.PropertyChanged += (o,e) => AppSettings.SaveSettings();
            StageOptions.PropertyChanged += (o, e) => AppSettings.SaveSettings();
            ProcessOptions.PropertyChanged += (o, e) => AppSettings.SaveSettings();

            StageX = AppSettings.OriginalX;
            StageY = AppSettings.OriginalY;
            Stage?.Open(StageOptions.Options, 3, StageX, StageY);
        }

        public int StageAfterCenterDelay { get; set; }

        public Settings.Settings AppSettings { get; private set; }

        public ObservableCollection<ICameraInfo> Cameras { get; }

        public ICamera Camera { get; set; }

        public ITrackingStage Stage { get; set; }

        public ICommand MoveStageCommand { get; }

        public ICommand OpenImageCommand { get; }

        public ICommand ProcessImageCommand { get; }

        public ICommand OpenCameraCommand { get; }

        public ICommand CloseCameraCommand { get; }

        public ICommand StartCameraCommand { get; }

        public ICommand StartCalibrateCommand { get; }

        public ICommand StopCalibrateCommand { get; }

        public ICommand MarkCalibrateCommand { get; }

        public ICommand StageXyPosCommand { get; }

        public ICommand SaveToFileCommand { get; }

        public ICommand RefreshDevicesCommand { get; }

        public ICommand MakeXyPosOriginCommand { get; }

        public ICommand OpenLineUpCommand { get; set; }

        public ILabSave LabSave { get; private set; }
        public ILabImage LabImage { get; set; }
        public ProcessOptionsModel ProcessOptions { get; }
        public StageOptionsModel StageOptions { get; }

        public LightScopeModel LightScope { get; }

        public Stopwatch Watch { get; }

        private bool ReOpenStageToNewOriginalPos { get; set; }

        public string SavedFileName
        {
            get => _savedFileName;
            set => SetProperty(ref _savedFileName, value, nameof(SavedFileName));
        }

        public double Seconds
        {
            get => _seconds;
            set => SetProperty(ref _seconds, value, nameof(Seconds));
        }

        public int FrameCount
        {
            get => _fameCount;
            set => SetProperty(ref _fameCount, value, nameof(FrameCount));
        }


        public BitmapSource Source
        {
            get => _source;
            set => SetProperty(ref _source, value, nameof(Source));
        }

        public BitmapSource Source1
        {
            get => _source1;
            set => SetProperty(ref _source1, value, nameof(Source1));
        }

        public BitmapSource Source2
        {
            get => _source2;
            set => SetProperty(ref _source2, value, nameof(Source2));
        }

        public int CalibrateMoveStep
        {
            get => _calibrateMoveStep;
            set => SetProperty(ref _calibrateMoveStep, value, nameof(CalibrateMoveStep));
        }

        public double StageX
        {
            get => _stageX;
            set => SetProperty(ref _stageX, value, nameof(StageX));
        }

        public double StageY
        {
            get => _stageY;
            set => SetProperty(ref _stageY, value, nameof(StageY));
        }

        public bool TrackingObject
        {
            get => _trackingObject;
            set => SetProperty(ref _trackingObject, value, nameof(TrackingObject));
        }

        public bool IsCalibration
        {
            get => _isCalibration;
            set => SetProperty(ref _isCalibration, value, nameof(IsCalibration));
        }

        public bool LocalFlex
        {
            get => _localFlex;
            set => SetProperty(ref _localFlex, value, nameof(LocalFlex));
        }

        public int LocalFlexNum
        {
            get => _localFlexNum;
            set => SetProperty(ref _localFlexNum, value, nameof(LocalFlexNum));
        }


        public bool BinSplit
        {
            get => _binSplit;
            set 
            { 
                SetProperty(ref _binSplit, value, nameof(BinSplit));
                ImageSize = 800 * (_binSplit ? 0.5 : 1);
            }
        }

        public bool BinSplitHandleLeft
        {
            get => _binSplitHandleLeft;
            set => SetProperty(ref _binSplitHandleLeft, value, nameof(BinSplitHandleLeft));
        }
        private int CurrentLocalFlexNum { get; set; }

        public bool SaveData
        {
            get => _saveData;
            set
            {
                SetProperty(ref _saveData, value, nameof(SaveData));
                if (!value)
                {
                    LabSave?.ClearSave();
                }
            }
        }

        public double ImageSize
        {
            get => _imageSize;
            set => SetProperty(ref _imageSize, value, nameof(ImageSize));
        }

        public ICameraInfo SelectedCamera
        {
            get => _selectedCamera;
            set => SetProperty(ref _selectedCamera, value, nameof(SelectedCamera));
        }

        public bool IsCameraOpened
        {
            get => _isCameraOpened;
            set => SetProperty(ref _isCameraOpened, value, nameof(IsCameraOpened));
        }


        private void RefreshDevices()
        {
            if (Cameras.Count <= 0)
            {
                _cameraService?.FetchCameras().Foreach(x => Cameras.Add(x));
            }
            if (Stage == null)
            {
                Stage = new StageService().GetStages().FirstOrDefault();
                Stage?.Open(StageOptions.Options, 3, StageX, StageY);
            }
        }

        private void SaveToFile()
        {
            var dialog = new SaveFileDialog()
            {
                FileName = "imageData",
                DefaultExt = "flr",
                Filter = "Carbin Tracking File (*.flr)|*.flr|All files (*.*)|*.*"
            };
            if (dialog.ShowDialog() == true)
            {
                var file = dialog.FileName;
                SavedFileName = file;
                LabSave = new LabSave.LabSave(file, 10, 100)
                {
                    StartTime = DateTime.Now
                };
            }
        }

        private void OpenImage()
        {
            LabImage?.Dispose();
            var dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == true)
            {
                var file = dialog.FileName;
                LabImage = file.ReadFromFile();
            }
        }

        private void ProcessImage()
        {
            Watch.Restart();
            PocessImagAsync();
            ShowImage();
            Watch.Stop();
            TimeSpan ts = Watch.Elapsed;
            Seconds = ts.Milliseconds;
        }

        private Task<Roi> PocessImagAsync()
        {
            // https://blog.stephencleary.com/2013/11/there-is-no-thread.html
            // https://stackoverflow.com/questions/17661428/async-stay-on-the-current-thread
            // https://stackoverflow.com/questions/27051169/why-does-await-loadasync-freeze-the-ui-while-await-task-run-load/27071434#27071434
            //https://stackoverflow.com/questions/37419572/if-async-await-doesnt-create-any-additional-threads-then-how-does-it-make-appl
            //https://stackoverflow.com/questions/40324300/calling-async-methods-from-non-async-code
            //probally put it in another thread to run to get real async
            //I'm not going to compete with Eric Lippert or Lasse V. Karlsen, and others, I just would like to draw attention to another facet of this question, that I think was not explicitly mentioned.

            //Using await on it's own does not make your app magically responsive. If whatever you do in the method you are awaiting on from the UI thread blocks, it will still block your UI the same way as non-awaitable version would.

            // You have to write your awaitable method specifically so it either spawn a new thread or use a something like a completion port(which will return execution in the current thread and call something else for continuation whenever completion port gets signaled).But this part is well explained in other answers.
            if (LabImage.DetectBlobs(ProcessOptions.Options))
            {
                var result = LabImage.Rois.OrderByDescending(x => x.Width * x.Height).First();
                LabImage.DrawBinaryImageAllRectangles();
                if (ProcessOptions.IsRgbImageAvailable)
                {
                    LabImage.DrawImageRectangle(result, ImageType.Rgb, MarkerColor.Red);
                }

                return Task.FromResult(result);
            }

            return Task.FromResult((Roi)null);
        }

        private void OpenCamrea()
        {
            if (SelectedCamera != null)
            {
                Camera = _cameraService.OpenCamera(SelectedCamera);
                if (Camera == null || !Camera.IsOpened)
                {
                    return;
                }
                IsCameraOpened = true;
                Camera.AcquistionCompleted += async (o, e) =>
                {
                    LabImage = LabImageFactory.From(e.SizeX, e.SizeY, e.Frame, BinSplit, BinSplitHandleLeft);
                    _currentImageSizeX = BinSplit ? e.SizeX / 2 : e.SizeX;
                    _currentImageSizeY = e.SizeY;
                    var roi = await PocessImagAsync();
                    _currentRoi = roi;
                    _currentStagePos = Stage?.GetStageXAndY();
                    if (BinSplit)
                    {
                        using(var image = LabImageFactory.From(e.SizeX, e.SizeY, e.Frame, BinSplit, !BinSplitHandleLeft))
                        {
                            ShowBinSplitImage(image, ProcessOptions.Options.CastBits);
                        }
                    }
                    ShowImage();
                    if (_isCalibrateObjectMarked && IsCalibration)
                    {
                        _calibrateRoi = _currentRoi;
                        _calibrateStagePos = _currentStagePos;
                        _isCalibrateObjectMarked = false;
                    }

                    if (TrackingObject && !IsCalibration)
                    {
                        ProcessStage();
                    }

                    if (SaveData)
                    {
                        SaveFrame(roi, e.Frame);
                    }

                    FrameCount++;
                    if (FrameCount % 100 == 0)
                    {
                        Watch.Stop();
                        TimeSpan ts = Watch.Elapsed;
                        Seconds += ts.Seconds;
                        Watch.Restart();
                    }

                    LabImage?.Dispose();
                    Camera.Acquisition();
                };
            }
        }

        private void SaveFrame(Roi roi, ushort[] data)
        {
            if (LabSave == null)
            {
                return;
            }
            var motionPosX = _currentStagePos?.X ?? 0;
            var motionPosY = _currentStagePos?.Y ?? 0;
            int wormX = 0, wormY = 0;
            if (roi != null)
            {
                wormX = StageOptions.XDirectionRightIncrease ?  roi.CenterX : -1 * roi.CenterX;
                wormY = StageOptions.YDirectionDownIncrease ? roi.CenterY : -1 * roi.CenterY;
            }
            var frame = new Frame()
            {
                Time = (DateTime.Now - LabSave.StartTime).TotalMilliseconds,
                MotionPosX = motionPosX,
                MotionPosY = motionPosY,
                FrameHeight = _currentImageSizeY,
                FrameWidth = BinSplit?_currentImageSizeX*2: _currentImageSizeX,
                Data = data,
                ImageDistanceMappingToMotionDistance = StageOptions.CalibrateFactor,
                ImotionX = (int)(motionPosX / StageOptions.CalibrateFactor + wormX),
                ImotionY = (int)(motionPosY / StageOptions.CalibrateFactor + wormY),
                IthisPosMotionX = (int)motionPosX,
                IcenterX = roi?.CenterX ?? 0,
                IlocalFlex = LocalFlexNum
            };
            LabSave.AddOneFrame(frame);
        }

        // for stage, left means negative movement, right means postive, up means negative, down means postive, no matter how real stage move, stage itself will do the adjustment

        private void ProcessStage()
        {
            // StageAfterCenterDelay usuage: try to delay the stage processing after the stage is moving, try to solve the stage jumping issue
            // define the min frames delay between two stage moving behaviors
            if (LocalFlex)
            {
                CurrentLocalFlexNum--;
            }
            else
            {
                StageAfterCenterDelay++;
            }

            if (_currentRoi == null) return;

            if (LocalFlex && CurrentLocalFlexNum <= 0)
            {
                CurrentLocalFlexNum = LocalFlexNum;
                MoveStage();
            }
            else if (!LocalFlex && StageAfterCenterDelay > 5 && IsCellOutOfTheLigtScope())
            {
                StageAfterCenterDelay = 0;
                MoveStage();
            }
        }


        private bool IsCellOutOfTheLigtScope()
        {
            return _currentRoi.Left < 50  ||
                   _currentRoi.Left + _currentRoi.Width + 50  > _currentImageSizeX ||
                   _currentRoi.Top < 50  ||
                   _currentRoi.Top + _currentRoi.Height + 50  > _currentImageSizeY;
        }
        private void MoveStage()
        {
            // need to recaculate center of the light
            var centerLightX = (_currentImageSizeX - LightScope.LeftMargin - LightScope.RightMargin) / 2 + LightScope.LeftMargin;
            var centerLightY = (_currentImageSizeY - LightScope.TopMargin - LightScope.BottomMargin) / 2 + LightScope.TopMargin;
            var moveX = centerLightX - _currentRoi.CenterX;
            var moveY = centerLightY - _currentRoi.CenterY;
            // if cell is on the left of the center, means stage should move to right
            // if cell is on the top of the center, means stage should move to the bottom
            Stage?.MoveXAndY(moveX * StageOptions.CalibrateFactor, moveY * StageOptions.CalibrateFactor);
        }

        private void ShowBinSplitImage(ILabImage binSplitImage, int bits)
        {
            binSplitImage.CastTo(ImageType.Bit8Gray, bits);
            var source = binSplitImage.ToBitmap(ImageType.Bit8Gray).ToBitmapSource();
            source?.Freeze();
            Source2 = source;
        }

        private void ShowImage()
        {
            var source = LabImage.ToBitmap(ImageType.Binary).ToBitmapSource();
            var source1 = ProcessOptions.IsRgbImageAvailable ? LabImage.ToBitmap(ImageType.Rgb).ToBitmapSource() : null;
            source?.Freeze();
            source1?.Freeze();
            Source = source;
            Source1 = source1;
        }


        private void CloseCamera()
        {
            Camera?.Stop();
            Stage?.Stop();
            LabSave?.Save();
        }

        private void StartRecord()
        {
            Watch.Start();
            Seconds = 0;
            FrameCount = 0;
            Camera?.Start();
            if (ReOpenStageToNewOriginalPos && Stage?.Options != null)
            {
                Stage?.Open(Stage.Options, 3, StageX, StageY);
                ReOpenStageToNewOriginalPos = false;
            }
        }

        private bool MoveStage(StageDirection direction)
        {
            var step = (StageOptions.HighSolution ? 0.1 : 1) * StageOptions.DefaultMovingStep;
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

        private void StartClibrate()
        {
            IsCalibration = true;
            _isCalibrateObjectMarked = false;
            _calibrateRoi = null;
            _calibrateStagePos = null;
            StartRecord();
        }

        private void MarkCalibrateObject()
        {
            _isCalibrateObjectMarked = true;
        }

        private void StopCalibrate()
        {
            CloseCamera();
            IsCalibration = false;
            _isCalibrateObjectMarked = false;
            if (_calibrateRoi != null && _calibrateStagePos != null)
            {
                if (_currentStagePos != null && _currentRoi != null)
                {
                    var stageDistance = Math.Sqrt(Math.Pow(_currentStagePos.Value.X - _calibrateStagePos.Value.X, 2) +
                                                  Math.Pow(_currentStagePos.Value.Y - _calibrateStagePos.Value.Y, 2));
                    var imageDistance = Math.Sqrt(Math.Pow(_currentRoi.CenterX - _calibrateRoi.CenterY, 2) +
                                                  Math.Pow(_currentRoi.CenterY - _calibrateRoi.CenterY, 2));
                    StageOptions.CalibrateFactor = stageDistance / imageDistance;
                    AppSettings.CalibrateFactor = StageOptions.CalibrateFactor;
                }
            }
        }

        private void GetStageXyPos()
        {
            var pos = Stage?.GetStageXAndY();
            if (pos != null)
            {
                StageX = pos.Value.X;
                StageY = pos.Value.Y;
            }
        }

        private void MakeXyPosOrigin()
        {
            ReOpenStageToNewOriginalPos = true;
            AppSettings.OriginalX = StageX;
            AppSettings.OriginalY = StageY;
            AppSettings.SaveSettings();
        }

        private void OpenLineUp()
        {
            var dialog = new Window
            {
                Title = "LineUp Dual View",
                Content = new LineUpView(SelectedCamera != null ? _cameraService.OpenCamera(SelectedCamera) : null, Stage),
                SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.NoResize
            };
            dialog.ShowDialog();
        }
    }
}
