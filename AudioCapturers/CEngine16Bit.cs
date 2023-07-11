using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using TunerWinUI.AudioVisualizers;

namespace TunerWinUI.AudioCapturers
{
    public class CEngine16Bit : CaptureEngine<short>
    {
        public CEngine16Bit(DataFlow flow = DataFlow.All) : base(flow) { }

#nullable enable

        public override void StartCapture(string device, RTScope<short> plotter, IFFTScope<short>? fftPlotter)
        {
            if (!mDevices.ContainsKey(device))
                throw new ArgumentException($"Invalid/Offline device: {device}");

            // Fortunatamente WasapiLoopback è un'estensione di WasapiCapture -> polimorfismo :)
            var selectedDevice = mDevices[device];
            mCapture = selectedDevice.DataFlow == DataFlow.Capture ? new WasapiCapture(selectedDevice) : new WasapiLoopbackCapture(selectedDevice);

            mCapture.WaveFormat = new(SampleRate, 16, 1);
            mCapture.DataAvailable += (_, data) =>
            {
                var samples16Bit = new short[data.BytesRecorded / 2];

                // TIME PLOT
                for (int i = 0, j = 0; i < data.BytesRecorded; i += 2, j++)
                {
                    var sample = (short)(data.Buffer[i + 1] << 8 | data.Buffer[i]);
                    //var normalized = (sample - minValue) / (double)(maxValue - minValue) * 2 - 1;

                    samples16Bit[j] = sample;
                }

                plotter.Plot(samples16Bit);
                fftPlotter?.Plot(samples16Bit);
            };

            IsBusyCapturing = true;
            mCapture.StartRecording();
        }

#nullable restore

    }
}
