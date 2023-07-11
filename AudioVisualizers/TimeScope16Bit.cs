using Microsoft.UI.Dispatching;
using OxyPlot;
using System;
using System.Threading.Tasks;

namespace TunerWinUI.AudioVisualizers;

public sealed class TimeScope16Bit : RTScope<short>
{
    public TimeScope16Bit(DispatcherQueue uiThreadDispatcherQueue) : base(uiThreadDispatcherQueue)
    {
        YAxis.Minimum = -18000;
        YAxis.Maximum = 18000;
    }

    public override void Plot(short[] samples)
    {
        Task.Run(() => CalculateFrameRate(DateTime.Now));

        lock (Model.SyncRoot)
        {
            mCollector.Clear();
            for (int i = 0; i < samples.Length; i++)
                mCollector.Add(new DataPoint(i, samples[i]));
        }

        mUIThreadDispatcherQueue.TryEnqueue(() => Model.InvalidatePlot(true));

    }
}
