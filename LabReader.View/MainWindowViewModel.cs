using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using LabDriveView.Core;
using LabSave;
using Microsoft.Win32;

namespace LabReader.View
{
    public class MainWindowViewModel:ViewModelBase
    {
        private double _progress;
        private ObservableCollection<FileItem> _files;
        private bool _canProcess;
        private bool _canAddFile;

        public MainWindowViewModel()
        {
            ConvertCarbinsCommand = new SimpleCommand(AddFile);
            ProcessCommand = new SimpleCommand(async () => await ConvertCarbins());
            DeleteCommand = new SimpleCommand<FileItem>(e=>DeleteFile(e));
            _files = new ObservableCollection<FileItem>();
            _canAddFile = true;
        }

        public ICommand ConvertCarbinsCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ProcessCommand { get; }

        public double Progress
        {
            get => _progress;
            set => SetProperty(ref _progress, value, nameof(Progress));
        }

        public ObservableCollection<FileItem> Files
        {
            get
            {
                return _files;
            } 
            set
            {
                SetProperty(ref _files, value, nameof(Files));
            }
        }

        public bool CanProcess
        {
            get { return _canProcess; }
            set { SetProperty(ref _canProcess, value, nameof(CanProcess)); }
        }

        public bool CanAddFile
        {
            get { return _canAddFile; }
            set { SetProperty(ref _canAddFile, value, nameof(CanAddFile)); }
        }

        private bool DeleteFile(FileItem file)
        {
            Files.Remove(file);
            if(Files.Count == 0)
            {
                CanProcess = false;
            }
            return true;
        }

        private void AddFile()
        {
            var dialog = new OpenFileDialog
            {
                DefaultExt = "flr",
                Filter = "Carbin Tracking File (*.flr)|*.flr|All files (*.*)|*.*",
            };
            if (dialog.ShowDialog() == true)
            {
                string fileName = dialog.FileName;
                if (!string.IsNullOrEmpty(fileName))
                {
                    Files.Add(new FileItem() { FileName = fileName });
                    CanProcess = true;
                }
            }
        }

        private async Task ConvertCarbins()
        {
            CanProcess = false;
            CanAddFile = false;
            foreach (var fileName in Files)
            {
                ILabReader labReader = new LabSave.LabReader();
                await labReader.ReadAsync(fileName.FileName, report => {
                    Progress = report;
                    fileName.Percentage = report;
                    }
                );
            }
        }
    }

    public class FileItem:ViewModelBase
    {
        private double _percentage;
        public string FileName { get; set; }

        public double Percentage
        {
            get { return _percentage; }
            set
            {
                SetProperty(ref _percentage, value, nameof(Percentage));
            }
     
        }
    }
}
