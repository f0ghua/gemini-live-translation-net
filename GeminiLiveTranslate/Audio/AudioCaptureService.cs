using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace GeminiLiveTranslate.Audio;

public sealed class AudioCaptureService : IDisposable
{
    private const int ChunkSize = 6400;
    private IWaveIn? _capture;
    private Pcm16Chunker? _chunker;

    public int DroppedChunks { get; private set; }

    public IReadOnlyList<string> ListInputDevices()
    {
        var names = new List<string> { "Default system audio (WASAPI loopback)" };
        for (var i = 0; i < WaveIn.DeviceCount; i++)
        {
            names.Add($"{i}: {WaveIn.GetCapabilities(i).ProductName}");
        }
        return names;
    }

    public void Start(string source, int deviceNumber, Action<byte[]> onChunk)
    {
        Stop();
        DroppedChunks = 0;
        _chunker = new Pcm16Chunker(ChunkSize, onChunk);

        if (source == "mic")
        {
            var waveIn = new WaveInEvent
            {
                DeviceNumber = deviceNumber >= 0 ? deviceNumber : 0,
                WaveFormat = new WaveFormat(16000, 16, 1),
                BufferMilliseconds = 100
            };
            waveIn.DataAvailable += OnDataAvailable;
            waveIn.StartRecording();
            _capture = waveIn;
            return;
        }

        var enumerator = new MMDeviceEnumerator();
        var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        var loopback = new WasapiLoopbackCapture(device);
        loopback.DataAvailable += OnDataAvailable;
        loopback.StartRecording();
        _capture = loopback;
    }

    public void Stop()
    {
        if (_capture is null) return;
        try
        {
            _capture.DataAvailable -= OnDataAvailable;
            _capture.StopRecording();
            _capture.Dispose();
        }
        catch
        {
            // Stop should be best effort during app shutdown.
        }
        finally
        {
            _capture = null;
            _chunker?.Reset();
            _chunker = null;
        }
    }

    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        try
        {
            if (_capture is null || _chunker is null) return;
            var data = e.Buffer.AsSpan(0, e.BytesRecorded).ToArray();
            var pcm = Pcm16Processor.ConvertToMono16KhzPcm(data, _capture.WaveFormat);
            _chunker.Append(pcm);
        }
        catch
        {
            DroppedChunks++;
        }
    }

    public void Dispose() => Stop();
}
