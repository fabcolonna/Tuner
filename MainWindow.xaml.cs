using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using NAudio.CoreAudioApi;
using TunerWinUI.AudioCapturers;
using TunerWinUI.AudioVisualizers;

namespace TunerWinUI
{
    public sealed partial class MainWindow : Window
    {
        public ACEngine AudioEngine { get; } = new(DataFlow.All);

        public RTPlotter16Bit TimeDomainPlot { get; } = new(DispatcherQueue.GetForCurrentThread())
        {
            XAxis =
            {
                IsAxisVisible = true,
                Minimum = 0,
                Maximum = 5000,
                AbsoluteMaximum = 5000,
                AbsoluteMinimum = 0,
                MajorStep = 1000,
                MinorStep = 100,
                LabelFormatter = (val) => ((int)val / 1000).ToString(),

            },
            YAxis =
            {
                Minimum = -18000,
                Maximum = 18000,
            }
        };

        public MainWindow() => InitializeComponent();

        private void ToggleCaptureClick(object sender, RoutedEventArgs e)
        {
            if (AudioEngine.IsBusyCapturing)
            {
                AudioEngine.StopCapture();
                CaptureButtonSymbol.Symbol = Symbol.Microphone;
                CaptureButton.Background = default;
            }
            else
            {
                AudioEngine.StartCapture16Bit(SourceDevice.SelectedItem?.ToString(), TimeDomainPlot);

                CaptureButtonSymbol.Symbol = Symbol.Stop;
                // CaptureButton.Background = new SolidColorBrush(Color.FromArgb(255, 133, 27, 45));
            }
        }

        private void OnRefreshOnlineDevicesClick(object sender, RoutedEventArgs e) => AudioEngine.RefreshAvailableDevicesList(DataFlow.Capture);
    }
}
