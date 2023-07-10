using OxyPlot;

namespace TunerWinUI.AudioVisualizers
{
    public interface IRealTimeAudioDataPlotter16Bit
    {
        public PlotModel Model { get; }

        public void Update(byte[] buffer, int bytesRecorded);
    }
}
