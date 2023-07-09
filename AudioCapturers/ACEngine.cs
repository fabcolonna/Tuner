using Microsoft.UI.Dispatching;
using NAudio.CoreAudioApi;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TunerWinUI.AudioCapturers
{
    public class ACEngine
    {
        private IDictionary<string, MMDevice> mDevices;

        private WasapiCapture mCapture;

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

        public void StartCapture16Bit(string device, PlotModel model, ICollection<DataPoint> coll, DispatcherQueue uiCoreDispatcher, int sampleRate = 44100)
        {
            if (!mDevices.ContainsKey(device))
                throw new ArgumentException($"Invalid/Offline device: {device}");

            if (sampleRate is <= 0 or > 192000)
                throw new ArgumentException($"Invalid sampling rate: {sampleRate}");

            mCapture = new WasapiCapture(mDevices[device]);
            mCapture.WaveFormat = new(44100, 16, 1);
            mCapture.DataAvailable += (_, data) =>
            {
                lock (model.SyncRoot)
                {
                    coll.Clear();
                    for (var i = 0; i < data.BytesRecorded; i += 2)
                    {
                        var sample = (short)(data.Buffer[i + 1] << 8 | data.Buffer[i]);
                        var normalized = sample / (double)short.MaxValue;
                        coll.Add(new DataPoint(i, normalized));
                    }
                }

                uiCoreDispatcher.TryEnqueue(
                    () => model.InvalidatePlot(true));
            };

            mCapture.StartRecording();
        }

        public void StopCapture()
        {
            mCapture?.StopRecording();
            mCapture?.Dispose();
            mCapture = null;
        }
    }
}
