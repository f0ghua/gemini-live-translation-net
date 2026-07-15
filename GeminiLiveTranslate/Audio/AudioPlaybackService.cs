using NAudio.Wave;

namespace GeminiLiveTranslate.Audio;

public sealed class AudioPlaybackService : IDisposable
{
    private readonly object _gate = new();
    private WaveOutEvent? _output;
    private BufferedWaveProvider? _buffer;

    public void Start(double volume)
    {
        Stop();
        _buffer = new BufferedWaveProvider(new WaveFormat(24000, 16, 1))
        {
            BufferDuration = TimeSpan.FromSeconds(5),
            DiscardOnBufferOverflow = true
        };
        _output = new WaveOutEvent { DesiredLatency = 160, Volume = (float)Math.Clamp(volume, 0, 1) };
        _output.Init(_buffer);
        _output.Play();
    }

    public void SetVolume(double volume)
    {
        if (_output is not null) _output.Volume = (float)Math.Clamp(volume, 0, 1);
    }

    public void EnqueuePcm16(byte[] data)
    {
        lock (_gate)
        {
            _buffer?.AddSamples(data, 0, data.Length);
        }
    }

    public void Stop()
    {
        lock (_gate)
        {
            try { _output?.Stop(); } catch { }
            _output?.Dispose();
            _output = null;
            _buffer = null;
        }
    }

    public void Dispose() => Stop();
}
