using CommunityToolkit.WinUI.UI.Helpers;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Collections.Generic;

namespace TunerWinUI.AudioVisualizers
{
    public class RTPlotter16Bit : IRealTimeAudioDataPlotter16Bit
    {
        private const short minValue = short.MinValue;
        private const short maxValue = short.MaxValue;

        private static readonly OxyColor sColorForDarkMode = OxyColors.White;
        private static readonly OxyColor sColorForLightMode = OxyColors.Black;

        private readonly ThemeListener mThemeListener = new();
        private readonly DispatcherQueue mUIThreadDispatcherQueue;

        private readonly ICollection<DataPoint> mCollector = new List<DataPoint>();

        public PlotModel Model { get; }

        public Axis XAxis { get; } = new LinearAxis()
        {
            IsAxisVisible = true,
            Position = AxisPosition.Bottom
        };

        public Axis YAxis { get; } = new LinearAxis()
        {
            IsAxisVisible = false,
            Position = AxisPosition.Left,
            IsZoomEnabled = false,
            IsPanEnabled = false,
            MaximumMargin = 20,
            MinimumMargin = 20,
        };

        public RTPlotter16Bit(DispatcherQueue uiThreadDispatcherQueue, int borderThickness = 1, int graphThickness = 1)
        {

            mUIThreadDispatcherQueue = uiThreadDispatcherQueue;

            Model = new()
            {
                PlotAreaBorderThickness = new(borderThickness),
                PlotAreaBorderColor = Application.Current.RequestedTheme == ApplicationTheme.Dark
                    ? sColorForDarkMode
                    : sColorForLightMode,
                Axes = { XAxis, YAxis },
                Series =
                {
                    new LineSeries()
                    {
                        StrokeThickness = graphThickness,
                        Selectable = false,
                        ItemsSource = mCollector
                    }
                }
            };

            mThemeListener.ThemeChanged += sender =>
            {
                switch (sender.CurrentTheme)
                {
                    case ApplicationTheme.Dark:
                        Model.PlotAreaBorderColor = sColorForDarkMode;
                        XAxis.TicklineColor = sColorForDarkMode;
                        XAxis.AxislineColor = sColorForDarkMode;
                        YAxis.TicklineColor = sColorForDarkMode;
                        YAxis.AxislineColor = sColorForDarkMode;
                        break;
                    case ApplicationTheme.Light:
                        XAxis.TicklineColor = sColorForLightMode;
                        XAxis.AxislineColor = sColorForLightMode;
                        YAxis.TicklineColor = sColorForLightMode;
                        YAxis.AxislineColor = sColorForLightMode;
                        break;
                }
            };
        }

        public void Update(byte[] buffer, int bytesRecorded)
        {
            lock (Model.SyncRoot)
            {
                mCollector.Clear();
                for (var i = 0; i < bytesRecorded; i += 2)
                {
                    var sample = (short)(buffer[i + 1] << 8 | buffer[i]);

                    //var normalized = (sample - minValue) / (double)(maxValue - minValue) * 2 - 1;
                    mCollector.Add(new DataPoint(i, sample));
                }
            }

            mUIThreadDispatcherQueue.TryEnqueue(() => Model.InvalidatePlot(true));
        }
    }
}
