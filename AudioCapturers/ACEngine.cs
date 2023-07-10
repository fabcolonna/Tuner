using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using TunerWinUI.AudioVisualizers;

namespace TunerWinUI.AudioCapturers
{
    public class ACEngine
    {
        private IDictionary<string, MMDevice> mDevices;

        private WasapiCapture mCapture;

        public bool IsBusyCapturing { get; private set; }

        public ICollection<string> OnlineDevices => mDevices.Keys;

        public ACEngine(DataFlow flow) => RefreshAvailableDevicesList(flow);

        public void RefreshAvailableDevicesList(DataFlow flow)
        {
            mDevices = new Dictionary<string, MMDevice>();
            new MMDeviceEnumerator()
                .EnumerateAudioEndPoints(flow, DeviceState.Active)
                .ToList()
                .ForEach(dev => mDevices.Add(dev.FriendlyName, dev));
        }

        public void StartCapture16Bit(string device, IRealTimeAudioDataPlotter16Bit plotter, int sampleRate = 44100)
        {
            if (!mDevices.ContainsKey(device))
                throw new ArgumentException($"Invalid/Offline device: {device}");

            if (sampleRate is <= 0 or > 192000)
                throw new ArgumentException($"Invalid sampling rate: {sampleRate}");

            // Fortunatamente WasapiLoopback è un'estensione di WasapiCapture -> polimorfismo :)
            var selectedDevice = mDevices[device];
            mCapture = selectedDevice.DataFlow == DataFlow.Capture ? new WasapiCapture(selectedDevice) : new WasapiLoopbackCapture(selectedDevice);

            mCapture.WaveFormat = new(sampleRate, 16, 1);
            mCapture.DataAvailable += (_, data) => plotter.Update(data.Buffer, data.BytesRecorded);

            IsBusyCapturing = true;
            mCapture.StartRecording();
        }

        public void StopCapture()
        {
            if (!IsBusyCapturing) return;

            IsBusyCapturing = false;
            mCapture?.StopRecording();
            mCapture?.Dispose();
            mCapture = null;
        }
    }
}
