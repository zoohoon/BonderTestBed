using LogModule;
using System;

namespace ProberInterfaces
{
    public class IntegralImage
    {
        public long[][] nSumImage; // normal  integral image
        //private long[,] sSumImage; // squared integral image
        //private long[,] tSumImage; // tilted  integral image

        //private long* nSum; // normal  integral image
        //private long* sSum; // squared integral image
        //private long* tSum; // tilted  integral image

        public int Width;
        public int Height;

        //private int nWidth;
        //private int nHeight;

        //private int tWidth;
        //private int tHeight;

        public IntegralImage(int width, int height)
        {
            try
            {
                Width = width;
                Height = height;
                //nSumImage = new long[Width, Height];

                long[][] nSumImage = new long[Height][];
                for (int i = 0; i < Height; i++)
                    nSumImage[i] = new long[Width];
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
