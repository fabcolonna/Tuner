using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using NAudio.CoreAudioApi;
using TunerWinUI.AudioCapturers;
using TunerWinUI.AudioVisualizers;

namespace TunerWinUI;

public sealed partial class MainWindow : Window
{
    #region PROPERTIES

    public CaptureEngine<short> AudioEngine { get; } = new CEngine16Bit(DataFlow.All);

    public RTScope<short> TimeDomainPlot { get; } = new TimeScope16Bit(DispatcherQueue.GetForCurrentThread());

    public FFTScope16Bit FrequencyDomainPlot { get; } = new(DispatcherQueue.GetForCurrentThread());

    #endregion

    public MainWindow()
    {
        InitializeComponent();
    }

    private void ToggleCapture_Click(object sender, RoutedEventArgs e)
    {
        if (AudioEngine.IsBusyCapturing)
        {
            AudioEngine.StopCapture();

            SourceDeviceCombo.IsEnabled = true;
            SamplingRateCombo.IsEnabled = true;
            FFTSizeCombo.IsEnabled = true;

            CaptureButtonSymbol.Symbol = Symbol.Play;
        }
        else
        {
            var selectedSampleRate = SamplingRateCombo.SelectedItem?.ToString();
            if (selectedSampleRate != null)
            {
                var intValue = int.Parse(selectedSampleRate.Split(" ")[0]);
                AudioEngine.SampleRate = intValue;
                FrequencyDomainPlot.SampleRate = intValue;
            }

            FrequencyDomainPlot.FFTSize = (int)FFTSizeCombo.SelectedItem;

            AudioEngine.StartCapture(SourceDeviceCombo.SelectedItem?.ToString() ?? string.Empty, TimeDomainPlot, FrequencyDomainPlot);

            CaptureButtonSymbol.Symbol = Symbol.Stop;

            SourceDeviceCombo.IsEnabled = false;
            SamplingRateCombo.IsEnabled = false;
            FFTSizeCombo.IsEnabled = false;
        }
    }

    private void RefreshOnlineDevices_Click(object sender, RoutedEventArgs e) => AudioEngine.RefreshAvailableDevicesList(DataFlow.Capture);
}