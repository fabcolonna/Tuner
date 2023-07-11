namespace TunerWinUI.Utilities
{
    public static class MathUtils
    {
        public static bool IsValidFFTSize(int size)
        {
            // Power of 2 -> 1 solo bit settato -> tipo
            /*
             * 00001000 &
             * 00000111 =
             * ----------
             * 00000000 -> è multiplo di 2
             *
             * invece
             *
             * 00001101 &
             * 00001100 =
             * ----------
             * 00001100 -> non è multiplo di 2
             */
            return size > 0 && (size & (size - 1)) == 0;
        }
    }
}
