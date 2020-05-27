using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace CCube
{
    public class ApplicationData : BindableBase
    {
        private ApplicationData()
        {
            InputsViewSource.Filter += (object sender, FilterEventArgs e) =>
            {
                var status = ((Input)e.Item).CurrentActiveIteration?.CurrentStatus;

                if (status == null)
                {
                    e.Accepted = true;
                    return;
                }

                if ((status == Iteration.Status.Waiting && (MainWindow.CheckBoxWaiting.IsChecked ?? false)) ||
                    (status == Iteration.Status.Running && (MainWindow.CheckBoxRunning.IsChecked ?? false)) ||
                    (status == Iteration.Status.Succeeded && (MainWindow.CheckBoxSuccessful.IsChecked ?? false)) ||
                    (status == Iteration.Status.Failed && (MainWindow.CheckBoxFailed.IsChecked ?? false)))
                    e.Accepted = true;

                else
                    e.Accepted = false;
            };

            Inputs = new Input[0];
        }

        public static ApplicationData Service { get; } = new ApplicationData();

        //public ObservableCollection<Input> Inputs { get; } = new ObservableCollection<Input>();
        private IEnumerable<Input> inputs;
        public IEnumerable<Input> Inputs
        {
            get { return inputs ?? new Input[0]; }
            set { if (SetProperty(ref inputs, value ?? new Input[0])) InputsViewSource.Source = inputs; }
        }
        public CollectionViewSource InputsViewSource { get; } = new CollectionViewSource();

        public Notifier Notifier { get; } = Notifier.Service;

        public ImportManager ImportManager { get; } = ImportManager.Service;

        public Stats Stats { get; } = Stats.Service;

        public XmlLanguage Language { get { return XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag); } }

        public string CCCommandExeName { get { return "CCCommand.exe"; } }

        private string userSetPathToCCCommandExe = @"C:\Program Files\Siemens\Tecnomatix_14.1\eMPower\eM-Planner\CCCommand.exe";
        public string UserSetPathToCCCommandExe
        {
            get { return userSetPathToCCCommandExe; }
            set
            {
                if (Equals(userSetPathToCCCommandExe, value)) return;

                userSetPathToCCCommandExe = value;
                ccCommandExeFound = null;

                OnPropertyChanged();
                OnPropertyChanged("PathToCCCommandExe");
                OnPropertyChanged("CCCommandExeFound");
            }
        }

        public string PathToCCCommandExe { get { return string.IsNullOrWhiteSpace(UserSetPathToCCCommandExe) ? CCCommandExeName : UserSetPathToCCCommandExe; } }

        private bool? ccCommandExeFound;
        public bool CCCommandExeFound
        {
            get
            {
                if (ccCommandExeFound == null)
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo()
                        {
                            FileName = PathToCCCommandExe,
                            Arguments = "syncNode",
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }).WaitForExit();

                        ccCommandExeFound = true;
                    }

                    catch (Exception)
                    {
                        ccCommandExeFound = false;
                    }
                }

                return ccCommandExeFound.Value;
            }
        }

        public MainWindow MainWindow { get; set; }
    }
}