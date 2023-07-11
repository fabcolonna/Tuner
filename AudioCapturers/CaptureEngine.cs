using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Linq;
using TunerWinUI.AudioVisualizers;

namespace TunerWinUI.AudioCapturers
{
    public abstract class CaptureEngine<T>
    {
        protected IDictionary<string, MMDevice> mDevices;
        protected WasapiCapture mCapture;

        private int mSampleRate = 44100;

        #region PROPERTIES

        public bool IsBusyCapturing { get; protected set; }

        public int SampleRate
        {
            get => mSampleRate;
            set
            {
                if (value is <= 0 or > 192000)
                    throw new ArgumentOutOfRangeException($"Invalid sampling rate: {mSampleRate}");

                mSampleRate = value;
            }
        }

        public ICollection<string> AvailableSamplingRates { get; private set; }

        public ICollection<string> OnlineDevices => mDevices.Keys;

        #endregion

        protected CaptureEngine(DataFlow flow)
        {
            RefreshAvailableDevicesList(flow);
            AvailableSamplingRates = new List<string> { "64 Hz", "128 Hz", "1024 Hz", "8192 Hz", "22050 Hz", "44100 Hz", "88200 Hz", "192000 Hz" };
        }

        public void RefreshAvailableDevicesList(DataFlow flow)
        {
            mDevices = new Dictionary<string, MMDevice>();
            new MMDeviceEnumerator()
                .EnumerateAudioEndPoints(flow, DeviceState.Active)
                .ToList()
                .ForEach(dev => mDevices.Add(dev.FriendlyName, dev));
        }

#nullable enable

        public abstract void StartCapture(string device, RTScope<T> plotter, IFFTScope<T>? fftPlotter);

#nullable restore

        public virtual void StopCapture()
        {
            if (!IsBusyCapturing) return;

            IsBusyCapturing = false;
            mCapture?.StopRecording();
            mCapture?.Dispose();
            mCapture = null;
        }
    }
}
