﻿using System;
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
            InitializeComponent();

            Utils.GUIDispatcher = Dispatcher;

            var service = ApplicationData.Service;
            DataContext = service;
            service.MainWindow = this;
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
    }
}