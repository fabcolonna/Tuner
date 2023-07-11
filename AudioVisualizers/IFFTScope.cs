using System.Collections.Generic;

namespace TunerWinUI.AudioVisualizers
{
    public interface IFFTScope<in T>
    {
        public ICollection<int> AvailableFFTSizes { get; }

        public int FFTSize { get; set; }

        public int SampleRate { get; set; }

        public void Plot(T[] samples);
    }
}
