﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NorthPoleEngineering.WaspClassLibrary.Programming;
using NorthPoleEngineering.WaspClassLibrary;

namespace FirmwareUpdateDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        WaspCollection _wasps;
        Wasp _waspUT;
        EventWaitHandle _waspReady;

        /// <summary>
        /// Name of the WASP to be used for this test
        /// </summary>
        string[] _deviceNames = { "WASP-PoE_Lyndon" };

        public MainWindow()
        {
            InitializeComponent();
            buttonPoE.IsEnabled = false;
            _wasps = new WaspCollection();
            _wasps.CollectionVerbosity = Wasp.WaspLogLevel.None;
            _wasps.CollectionChanged += Wasps_CollectionChanged;
            _waspReady = new EventWaitHandle(false, EventResetMode.AutoReset);
        }

        private void buttonPoE_Click(object sender, RoutedEventArgs e)
        {
            Progress.Background = Brushes.Yellow;
            Progress.Value = 0;
            var progress = new Progress<ProgrammingProgress>(TesterProgrammingProgress);
            _waspUT.UpdateFirmware(Bundle.GetBundle("WASP-PoE 2.2.37-Firmware-R1.zip"), progress, new CompletionCallback(ProgrammingComplete), AccessMethod.Ethernet);
        }

        /// <summary>
        /// Called when a WASP is added or removed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Wasps_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                int count = e.NewItems.Count;
                for (int i = 0; i < e.NewItems.Count; i++)
                {
                    if (((Wasp)e.NewItems[i]).Name == _deviceNames[0] )
                    {
                        _waspUT = (Wasp)e.NewItems[i];
                        _waspReady.Set();
                        buttonPoE.IsEnabled = true;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Called on programming update
        /// </summary>
        /// <param name="obj"></param>
        private void TesterProgrammingProgress(ProgrammingProgress obj)
        {
            // Need to call the dispatcher since this update is coming in on a different thread
            Progress.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() => { Progress.Value = obj.Progress; }));
            System.Diagnostics.Trace.WriteLine(obj.Progress);
        }

        /// <summary>
        /// Called upon completion of programming
        /// </summary>
        /// <param name="status">Status update</param>
        private void ProgrammingComplete(ProgrammingStatusCode status)
        {
            Progress.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() => { Progress.Background = Brushes.Green; }));
        }

    }
}