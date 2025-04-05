using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_Microphone_Audio.AudioUtils
{
    public class YinAlgoritm
    {
        public static float Compute(int sampleRate, float[] buffer, int bufferSize, float threshold = 0.15f)
        {
            var difference = new float[bufferSize / 2];
            var cumulativeMeanNormalizedDifference = new float[bufferSize / 2];

            if (buffer.Length < bufferSize)
            {
                throw new ArgumentException("Buffer size is smaller than the required size.");
            }

            for (int tau = 0; tau < difference.Length; tau++)
            {
                float sum = 0f;
                for (int i = 0; i < difference.Length; i++)
                {
                    float delta = buffer[i] - buffer[i + tau];
                    sum += delta * delta;
                }

                difference[tau] = sum;
            }

            // Calculate the cumulative mean normalized difference
            cumulativeMeanNormalizedDifference[0] = 1;
            float runningSum = 0f;
            for (int tau = 1; tau < difference.Length; tau++)
            {
                runningSum += difference[tau];
                cumulativeMeanNormalizedDifference[tau] = difference[tau] * tau / runningSum;
            }

            // Absolute threshold
            int pitchTau = -1;
            for (int tau = 2; tau < cumulativeMeanNormalizedDifference.Length; tau++)
            {
                if (cumulativeMeanNormalizedDifference[tau] < threshold)
                {
                    // Optional refinement step
                    while (tau + 1 < cumulativeMeanNormalizedDifference.Length &&
                           cumulativeMeanNormalizedDifference[tau + 1] < cumulativeMeanNormalizedDifference[tau])
                    {
                        tau++;
                    }

                    pitchTau = tau;
                    break;
                }
            }

            // If no pitch was found, return -1
            if (pitchTau == -1)
            {
                return -1;
            }

            // Parabolic Interpolation to find the exact pitch
            if (pitchTau > 0 && pitchTau < cumulativeMeanNormalizedDifference.Length - 1)
            {
                float y0 = cumulativeMeanNormalizedDifference[pitchTau];
                float yMinus = cumulativeMeanNormalizedDifference[pitchTau - 1];
                float yPlus = cumulativeMeanNormalizedDifference[pitchTau + 1];

                float improvedTau = pitchTau + (yMinus - yPlus) / (2 * (yMinus - 2 * y0 + yPlus));

                return sampleRate / improvedTau;
            }
            else
            {
                return sampleRate / (float)pitchTau;
            }
        }
    }
}
