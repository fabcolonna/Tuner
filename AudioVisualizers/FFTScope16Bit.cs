using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using Microsoft.UI.Dispatching;
using OxyPlot;
using System;
using System.Collections.Generic;
using TunerWinUI.Utilities;

namespace TunerWinUI.AudioVisualizers;

public sealed class FFTScope16Bit : RTScope<short>, IFFTScope<short>
{
    private int mFFTSize = 8192, mSampleRate = 44100, mMaxHertz = 10000;
    private int mMaxYAxis;

    public ICollection<int> AvailableFFTSizes { get; }

    public int FFTSize
    {
        get => mFFTSize;
        set
        {
            if (!MathUtils.IsValidFFTSize(value))
                throw new ArgumentException("FFT Size must be a power of 2");

            mFFTSize = value;
        }
    }

    public int SampleRate
    {
        get => mSampleRate;
        set
        {
            if (value is <= 0 or > 192000)
                throw new ArgumentException($"Invalid sampling rate: {mSampleRate}");

            mSampleRate = value;
        }
    }

    public int MaxHertz
    {
        get => mMaxHertz;
        set
        {
            if (value is <= 0 or > 20000)
                throw new ArgumentException($"Invalid max Hertz threshold: {mMaxHertz}");

            mMaxHertz = value;
        }
    }

    public int MaximumYValue
    {
        get => mMaxYAxis;
        set
        {
            if (value <= 0)
                throw new ArgumentException("MaximumYValue must be > 0");

            mMaxYAxis = value;
            YAxis.Maximum = value;
            YAxis.AbsoluteMaximum = value;
        }
    }

    public FFTScope16Bit(DispatcherQueue uiThreadDispatcherQueue) : base(uiThreadDispatcherQueue)
    {
        AvailableFFTSizes = new List<int> { 2048, 4096, 8192, 16384, 32768, 65536 };

        XAxis.AbsoluteMaximum = 20000;
        XAxis.Maximum = mMaxHertz;
        XAxis.MajorGridlineStyle = LineStyle.Solid;
        XAxis.MinorGridlineStyle = LineStyle.Dot;
        XAxis.MinorGridlineThickness = 1;
        XAxis.MajorGridlineThickness = 2;
        XAxis.Title = "Frequency (kHz)";
        XAxis.IsAxisVisible = true;

        MaximumYValue = 25000;

        YAxis.IsAxisVisible = true;
        YAxis.Minimum = 0;
        YAxis.AbsoluteMinimum = 0;
        YAxis.IsZoomEnabled = false;
        YAxis.IsPanEnabled = false;
        YAxis.LabelFormatter = val => ((int)val / 1000).ToString();
    }

    public override void Plot(short[] samples)
    {
        // Non ho 20000 samples da analizzare da 0 a 20kHz, ne ho quanti NAudio ne ha presi in un dato momento
        // nel dominio dei tempi. Quelli li andrò a campionare con un rate dato da fftSize: produrrà in output
        // un buffer di samples da 0 a 20kHz (ovviamente preciso in base a fftSize) -> CREDO!

        // Può succedere che mFFTSize > samples.Length, in quel caso occorre
        // aggiungere del zero padding a complexes[]
        var maxBound = Math.Min(samples.Length, mFFTSize);

        var complexes = new Complex32[mFFTSize];
        for (int i = 0; i < maxBound; i++)
            complexes[i] = new Complex32(samples[i], 0);

        for (var i = maxBound; i < mFFTSize; i++)
            complexes[i] = Complex32.Zero;

        // Esegue FFT sul campione di dimensione arbitraria
        Fourier.Forward(complexes);

        lock (Model.SyncRoot)
        {
            mCollector.Clear();
            for (var i = 0; i < mFFTSize / 2; i++)
            {
                var freq = (float)i * mSampleRate / mFFTSize;
                var ampl = complexes[i].Magnitude > mMaxYAxis ? mMaxYAxis : complexes[i].Magnitude;
                mCollector.Add(new DataPoint(freq, ampl));
            }
        }

        mUIThreadDispatcherQueue.TryEnqueue(() => Model.InvalidatePlot(true));
    }
}