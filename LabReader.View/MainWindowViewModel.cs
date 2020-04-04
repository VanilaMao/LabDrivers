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

        public MainWindowViewModel()
        {
            ConvertCarbinsCommand = new SimpleCommand(async () => await ConvertCarbins());
        }

        public ICommand ConvertCarbinsCommand { get; }

        public double Progress
        {
            get => _progress;
            set => SetProperty(ref _progress, value, nameof(Progress));
        }

        private async Task ConvertCarbins()
        {
            var dialog = new OpenFileDialog
            {
                DefaultExt = "flr",
                Filter = "Carbin Tracking File (*.flr)|*.flr|All files (*.*)|*.*"
            };
            if (dialog.ShowDialog() == true)
            {
                var fileName = dialog.FileName;
                ILabReader labReader = new LabSave.LabReader();
                await labReader.ReadAsync(fileName,report=> Progress = report);
            }
        }
    }
}
