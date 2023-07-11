using CommunityToolkit.WinUI.UI.Helpers;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace TunerWinUI.AudioVisualizers;

public abstract class RTScope<T> : INotifyPropertyChanged
{
    protected readonly OxyColor sColorForDarkMode = OxyColors.White;
    protected readonly OxyColor sColorForLightMode = OxyColors.Black;

    protected readonly ThemeListener mThemeListener = new();
    protected readonly DispatcherQueue mUIThreadDispatcherQueue;

    protected readonly ICollection<DataPoint> mCollector = new List<DataPoint>();

    private int mFrameThickness = 1, mGraphThickness = 2;
    private string mFrameRate = "0";
    private DateTime mPreviousFrame;

    #region PROPERTIES

    public PlotModel Model { get; }

    public Axis XAxis { get; protected set; } = new LinearAxis
    {
        IsAxisVisible = false,
        Minimum = 0,
        AbsoluteMinimum = 0,
        MajorStep = 1000,
        MinorStep = 100,
        LabelFormatter = val => ((int)val / 1000).ToString(),
        Selectable = false,
        Position = AxisPosition.Bottom
    };

    public Axis YAxis { get; protected set; } = new LinearAxis
    {
        IsAxisVisible = false,
        Position = AxisPosition.Left,
        IsZoomEnabled = false,
        IsPanEnabled = false,
        Selectable = false
    };

    public int FrameThickness
    {
        get => mFrameThickness;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(FrameThickness));

            mFrameThickness = value;
        }
    }

    public int GraphThickness
    {
        get => mGraphThickness;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(GraphThickness));

            mGraphThickness = value;
        }
    }

    public OxyColor GraphColor { get; set; } = OxyColors.Green;

    public event PropertyChangedEventHandler PropertyChanged;

    public string FrameRate
    {
        get => mFrameRate;
        protected set
        {
            mFrameRate = value;

            mUIThreadDispatcherQueue.TryEnqueue(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FrameRate)));
            });
        }
    }

    #endregion

    #region METHODS

    protected RTScope(DispatcherQueue uiThreadDispatcherQueue)
    {
        mUIThreadDispatcherQueue = uiThreadDispatcherQueue;
        mPreviousFrame = DateTime.Now;

        Model = new()
        {
            PlotAreaBorderThickness = new(mFrameThickness),
            PlotAreaBorderColor = Application.Current.RequestedTheme == ApplicationTheme.Dark
                ? sColorForDarkMode
                : sColorForLightMode,
            Axes = { XAxis, YAxis },
            Series =
            {
                new LineSeries()
                {
                    StrokeThickness = mGraphThickness,
                    Color = GraphColor,
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

    protected void CalculateFrameRate(DateTime current)
    {
        var fr = Math.Round(1000 / (current - mPreviousFrame).TotalMilliseconds);
        FrameRate = fr.ToString(CultureInfo.CurrentCulture);
        mPreviousFrame = current;
    }

    public abstract void Plot(T[] samples);

    #endregion
}