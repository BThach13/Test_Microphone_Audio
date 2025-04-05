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
            InputVolume = AudioUtils.AudioUtils.CalculateRMS(e.Buffer, bytesToProcess);

            InputFrequency = AudioUtils.AudioUtils.CalculateFrequencyYin(e.Buffer, bytesToProcess, SampleRate);
        }
    }
}
