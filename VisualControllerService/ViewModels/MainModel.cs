using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace VisualControllerService.ViewModels
{
    public class MainModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<Process> Processes { get; set; }

        public Process SelectedProcess { get; set; }

        public ImageSource PrcessImg { get; set; }

        private ProcessScreenShot shot;
        public MainModel(ProcessScreenShot shot)
        {
            this.shot = shot;
            RefreshProcesses();
        }

        public void RefreshProcesses()
        {
            Processes = new ObservableCollection<Process>(shot.GetProcesses());
            OnPropertyChanged("Processes");
        }

        public ICommand ProcessClickCmd
        {
            get
            {
                return new RelayCommand(sel =>
                {
                    if(SelectedProcess != null)
                    {
                        this.PrcessImg = shot?.CaptureWindow(SelectedProcess);
                        OnPropertyChanged("PrcessImg");
                    }
                });
            }
        }
    }
}
