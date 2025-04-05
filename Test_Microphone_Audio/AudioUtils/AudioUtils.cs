using System.Numerics;

namespace Test_Microphone_Audio.AudioUtils
{
    public static class AudioUtils
    {

        public static float CalculateRMS(byte[] buffer, int bytesRecorded)
        {
            var samples = new short[bytesRecorded / 2];
            Buffer.BlockCopy(buffer, 0, samples, 0, bytesRecorded);

            long sum = 0;

            foreach (var sample in samples)
            {
                sum += sample * sample;
            }

            return FastSqrt((float)sum / samples.Length) / short.MaxValue;
        }

        private static float FastSqrt(float x)
        {
            return BitConverter.Int32BitsToSingle((BitConverter.SingleToInt32Bits(x) >> 1) + 532870912);
        }

        public static float CalculateFrequencyYin(byte[] buffer, int bytesRecorded, int sampleRate)
        {
            var samples = new short[bytesRecorded / 2];
            Buffer.BlockCopy(buffer, 0, samples, 0, bytesRecorded);
            var yinBuffer = new float[samples.Length];

            for (int i = 0; i < samples.Length; i++)
            {
                yinBuffer[i] = samples[i] / (float)short.MaxValue;
            }

            var pitch = YinAlgoritm.Compute(sampleRate, yinBuffer, yinBuffer.Length);

            return pitch;
        }

        public static float CalculateFrequencyFFT(byte[] buffer, int bytesRecorded, int sampleRate)
        {
            var samples = new short[bytesRecorded / 2];
            Buffer.BlockCopy(buffer, 0, samples, 0, bytesRecorded);

            var acf = ComputeACF(samples);
            var fundamentalFrequency = FindFundamentalFrequency(acf, sampleRate);

            var fftLength = 4096;
            var fftBuffer = new Complex[fftLength];

            for (int i = 0; i < fftLength; i++)
            {
                var window = 0.54 - 0.46 * Math.Cos(2 * Math.PI * i / (fftLength - 1)); // Hamming window for noise reduction
                fftBuffer[i] = i < samples.Length ? new Complex(samples[i] * window, 0) : Complex.Zero;
            }

            FastFourierTransform.Compute(ref fftBuffer);

            // Find the peak frequency
            float maxMagnitude = 0;
            int maxIndex = 0;
            for (int i = 0; i < fftLength / 2; i++)
            {
                float magnitude = (float)(fftBuffer[i].Magnitude * fftBuffer[i].Magnitude);
                if (magnitude > maxMagnitude)
                {
                    maxMagnitude = magnitude;
                    maxIndex = i;
                }
            }

            float frequency = maxIndex * sampleRate / fftLength;

            // Check for fundamental frequency (try to solve octave errors)
            if (Math.Abs(fundamentalFrequency - frequency) < fundamentalFrequency * 0.1)
            {
                frequency = fundamentalFrequency;
            }

            // Check for harmonics (try to solve octave errors)
            if (maxIndex > 1 && maxIndex < fftLength / 4)
            {
                float harmonicFrequency = (maxIndex * 2) * sampleRate / fftLength;
                if (Math.Abs(harmonicFrequency - frequency) < frequency * 0.1)
                {
                    frequency = harmonicFrequency / 2;
                }
            }

            return frequency;
        }

        private static float[] ComputeACF(short[] samples)
        {
            int length = samples.Length;
            float[] acf = new float[length];

            for (int lag = 0; lag < length; lag++)
            {
                float sum = 0;
                for (int i = 0; i < length - lag; i++)
                {
                    sum += samples[i] * samples[i + lag];
                }
                acf[lag] = sum;
            }

            return acf;
        }

        private static float FindFundamentalFrequency(float[] acf, int sampleRate)
        {
            var maxIndex = 1;
            var maxValue = float.MinValue;

            for (var i = 1; i < acf.Length; i++)
            {
                if (acf[i] > maxValue)
                {
                    maxValue = acf[i];
                    maxIndex = i;
                }
            }

            return sampleRate / maxIndex;
        }
    }
}
