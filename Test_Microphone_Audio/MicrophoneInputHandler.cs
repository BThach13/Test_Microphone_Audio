using System;
using System.Data;
using System.Numerics;
using System.Diagnostics;
using System.Windows.Forms;
using NAudio;
using NAudio.Wave;
using NAudio.Wasapi;
using NAudio.CoreAudioApi;

namespace Test_Microphone_Audio
{
    public class MicrophoneInputHandler
    {
        private static MicrophoneInputHandler _instance;
        private readonly WasapiCapture _capture;
        private const int SampleRate = 44100; // Sample rate in Hz
        private const int BytesPerSample = 2; // 16 bit audio
        private const int BufferSize = SampleRate * BytesPerSample / 10; // 100ms of audio

        private static float _inputVolume; // Volume of the input
        private static float _inputFrequency; // Frequency of the input

        public static float InputVolume
        {
            get => _inputVolume;
            private set => _inputVolume = value;
        }

        public static float InputFrequency
        {
            get => _inputFrequency;
            private set => _inputFrequency = value;
        }

        private MicrophoneInputHandler()
        {
            var enumerator = new MMDeviceEnumerator();
            var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);

            Debug.WriteLine("Available devices:");
            foreach (var device in devices)
            {
                Debug.WriteLine(device.FriendlyName);
            }

            _capture = new WasapiCapture(devices[0])
            {
                WaveFormat = new WaveFormat(SampleRate, 16, 1)
            };

            _capture.DataAvailable += OnDataAvailable;

            _capture.StartRecording();
            Debug.WriteLine("Recording started...");

        }

        public static MicrophoneInputHandler Instance => _instance ??= new MicrophoneInputHandler();

        public static void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            var bytesToProcess = Math.Min(e.BytesRecorded, BufferSize);
            InputVolume = CalculateRMS(e.Buffer, bytesToProcess);

            Debug.WriteLine($"Volume: {InputVolume}");


            InputFrequency = CalculateFrequency(e.Buffer, bytesToProcess);

            Debug.WriteLine($"Frequency: {InputFrequency}");
        }

        private static float CalculateRMS(byte[] buffer, int bytesRecorded)
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

        private static float CalculateFrequency(byte[] buffer, int bytesRecorded)
        {
            var samples = new short[bytesRecorded / 2];
            Buffer.BlockCopy(buffer, 0, samples, 0, bytesRecorded);

            var fftLength = 4096;
            var fftBuffer = new Complex[fftLength];

            for (int i = 0; i < fftLength; i++)
            {
                var window = 0.54 - 0.46 * Math.Cos(2 * Math.PI * i / (fftLength - 1)); // Hamming window for noise reduction
                fftBuffer[i] = i < samples.Length ? new Complex(samples[i] * window, 0) : Complex.Zero;
            }

            FastFourierTransform.Compute(fftBuffer);

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

            float frequency = maxIndex * SampleRate / fftLength;

            // Check for harmonics
            if (maxIndex > 1 && maxIndex < fftLength / 4)
            {
                float harmonicFrequency = (maxIndex * 2) * SampleRate / fftLength;
                if (Math.Abs(harmonicFrequency - frequency) < frequency * 0.1)
                {
                    frequency = harmonicFrequency / 2;
                }
            }

            return frequency;
        }
    }
}
