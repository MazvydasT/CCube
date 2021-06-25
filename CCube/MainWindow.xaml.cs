using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Forms;

namespace CCube
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            Utils.GUIDispatcher = Dispatcher;
            ApplicationData.Service.MainWindow = this;

            InitializeComponent();
        }

        private void ButtonBrowse_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog()
            {
                Filter = "CC Update Console parameters file (*.xml)|*.xml",
                Multiselect = true
            };

            var openFileResult = openFileDialog.ShowDialog();
            if (openFileResult == System.Windows.Forms.DialogResult.Cancel) return;

            var pathsToParams = openFileDialog.FileNames;
            
            Utils.AddInputsFromParamsXML(pathsToParams, sender == insertButton);
        }

        private void GoButton_Click(object sender, RoutedEventArgs e)
        {
            var applicationDataService = ApplicationData.Service;
            var importManager = applicationDataService.ImportManager;
            var importStatus = importManager.Status;

            if (importStatus == ImportManager.ImportStatusOptions.Idle)
            {
                applicationDataService.Stats.ImportStartTime = DateTime.Now;
                applicationDataService.Stats.IterationsSuccessfulSinceStart = 0;
                importManager.Start(applicationDataService.Inputs);
            }

            else if (importStatus == ImportManager.ImportStatusOptions.Running)
                importManager.Stop();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LogInWindow.Visibility = Visibility.Visible;
        }

        private void FilterCheckBoxChanged(object sender, RoutedEventArgs e)
        {
            ApplicationData.Service.InputsViewSource.View.Refresh();
        }

        private void OpenLogFolderButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = Logger.LogDirectory
            });
        }

        private void ExportConfig_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "CC Update Console parameters file (*.xml)|*.xml",
                OverwritePrompt = true
            };

            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) return;

            var service = ApplicationData.Service;

            var inputsToExport = service.ExportVisibleParams ? service.InputsViewSource.View.Cast<Input>() : dataGridInputs.SelectedItems.Cast<Input>();

            try
            {
                Utils.WriteParamsXML(inputsToExport, saveFileDialog.FileName);
            }

            catch(Exception exception)
            {
                Logger.Service.Log(exception.Message, Notification.NotificationTypes.Error);
            }
        }
    }
}