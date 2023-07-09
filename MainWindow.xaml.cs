using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using NAudio.CoreAudioApi;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Collections.Generic;
using TunerWinUI.AudioCapturers;

namespace TunerWinUI
{
    public sealed partial class MainWindow : Window
    {
        private DispatcherQueue mUiThreadDispatcher;

        private static readonly ICollection<DataPoint> data = new List<DataPoint>();

        public ACEngine AudioEngine { get; } = new(DataFlow.All);

        public MainWindow()
        {
            InitializeComponent();
            mUiThreadDispatcher = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
        }

        public PlotModel TempModel { get; } = new PlotModel
        {
            Axes =
            {
                new LinearAxis { Position = AxisPosition.Bottom },
                new LinearAxis
                {
                    Position = AxisPosition.Left,
                    Minimum = -1,
                    Maximum = 1
                },
            },
            Series =
            {
                new LineSeries
                {
                    MarkerType = MarkerType.None,
                    ItemsSource = data
                }
            }
        };

        private void ToggleCaptureClick(object sender, RoutedEventArgs e)
        {
            AudioEngine.StartCapture16Bit(SourceDevice.SelectedItem?.ToString(), TempModel, data, mUiThreadDispatcher);
        }

        private void OnRefreshOnlineDevicesClick(object sender, RoutedEventArgs e) => AudioEngine.RefreshAvailableDevicesList(DataFlow.Capture);
    }
}
