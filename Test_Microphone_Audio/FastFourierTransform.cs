using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_Microphone_Audio
{
    public class FastFourierTransform
    {
        public static void Compute(Complex[] buffer)
        {
            int n = buffer.Length;
            if (n == 1)
                return;

            if ((n & (n - 1)) != 0)
                throw new ArgumentException("n Must be a power of 2");

            // Bit-reversal permutation
            int m = (int)Math.Log(n, 2);
            for (int i = 0; i < n; i++)
            {
                int j = ReverseBits(i, m);
                if (j > i)
                {
                    var temp = buffer[i];
                    buffer[i] = buffer[j];
                    buffer[j] = temp;
                }
            }

            // Cooley-Tukey FFT
            for (int s = 1; s <= m; s++)
            {
                int m2 = 1 << s;
                Complex wm = Complex.Exp(-2 * Math.PI * Complex.ImaginaryOne / m2);
                for (int k = 0; k < n; k += m2)
                {
                    Complex w = 1;
                    for (int j = 0; j < m2 / 2; j++)
                    {
                        Complex t = w * buffer[k + j + m2 / 2];
                        Complex u = buffer[k + j];
                        buffer[k + j] = u + t;
                        buffer[k + j + m2 / 2] = u - t;
                        w *= wm;
                    }
                }
            }
        }

        private static int ReverseBits(int x, int bits)
        {
            int result = 0;
            for (int i = 0; i < bits; i++)
            {
                result = (result << 1) | (x & 1);
                x >>= 1;
            }
            return result;
        }
    }
}
