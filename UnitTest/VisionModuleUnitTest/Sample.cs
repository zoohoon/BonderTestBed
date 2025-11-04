namespace ProcessingModule.Tests
{
    public static class Sample
    {
        //세로 그라데이션
        public static byte[] CreateSampeImage_VerticalGradient(int width, int height)
        {
            byte[] ret = new byte[width * height];

            for (int h = 0; h < height; ++h)
            {
                for (int w = 0; w < width; ++w)
                {
                    ret[h * width + w] = (byte)((255.0 / width) * (w + 1));
                }
            }

            return ret;
        }

       /// <summary>
       /// L 형태 
       /// </summary>
       /// <param name="width"></param>
       /// <param name="height"></param>
       /// <returns></returns>
        public static byte[] CreateSampeImage_L(int width, int height)
        {
            byte[] ret = new byte[width * height];

            for (int h = height/4; h < height/2; ++h) //상단 1/4 부터  ~ 1/2
            {
                int drawWidth = width / 3;

                if(h>height/3)
                {
                    drawWidth = width / 2;
                }

                for (int w = width/4; w < drawWidth; ++w) // 왼쪽 1/4부터 왼쪽  1/3
                {
                    ret[h * width + w] = (byte)(255.0);
                }
            }

            return ret;
        }
    }
}
