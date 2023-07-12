using Microsoft.UI.Dispatching;
using OxyPlot;
using System;
using System.Threading.Tasks;

namespace TunerWinUI.AudioVisualizers;

public sealed class TimeScope16Bit : RTScope<short>
{
    private int mMaxValue;

    public int MaximumXValue
    {
        get => mMaxValue;
        set
        {
            if (value <= 0)
                throw new ArgumentException("MaximumValue must be > 0");

            mMaxValue = value;
            XAxis.Maximum = value;
            XAxis.AbsoluteMaximum = value;
        }
    }

    public TimeScope16Bit(DispatcherQueue uiThreadDispatcherQueue) : base(uiThreadDispatcherQueue)
    {
        YAxis.Minimum = -18000;
        YAxis.Maximum = 18000;

        MaximumXValue = 2000;
        XAxis.IsAxisVisible = true;
    }

    public override void Plot(short[] samples)
    {
        Task.Run(() => CalculateFrameRate(DateTime.Now));

        int max = Math.Min(mMaxValue, samples.Length);

        lock (Model.SyncRoot)
        {
            mCollector.Clear();
            for (int i = 0; i < max; i++)
                mCollector.Add(new DataPoint(i, samples[i]));
        }

        mUIThreadDispatcherQueue.TryEnqueue(() => Model.InvalidatePlot(true));

    }
}
