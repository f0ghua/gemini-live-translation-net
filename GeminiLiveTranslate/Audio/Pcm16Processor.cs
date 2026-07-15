namespace GeminiLiveTranslate.Audio;

public sealed class Pcm16Chunker
{
    private readonly int _chunkSize;
    private readonly Action<byte[]> _onChunk;
    private readonly List<byte> _pending = new();

    public Pcm16Chunker(int chunkSize, Action<byte[]> onChunk)
    {
        _chunkSize = chunkSize;
        _onChunk = onChunk;
    }

    public void Append(byte[] bytes)
    {
        if (bytes.Length == 0) return;
        _pending.AddRange(bytes);
        while (_pending.Count >= _chunkSize)
        {
            var chunk = _pending.GetRange(0, _chunkSize).ToArray();
            _pending.RemoveRange(0, _chunkSize);
            _onChunk(chunk);
        }
    }

    public void Reset() => _pending.Clear();
}

public static class Pcm16Processor
{
    public static byte[] ConvertToMono16KhzPcm(byte[] input, NAudio.Wave.WaveFormat format)
    {
        var samples = DecodeToMonoFloat(input, format);
        if (samples.Length == 0) return [];
        var resampled = ResampleLinear(samples, format.SampleRate, 16000);
        var output = new byte[resampled.Length * 2];
        for (var i = 0; i < resampled.Length; i++)
        {
            var sample = Math.Clamp(resampled[i], -1f, 1f);
            var value = (short)(sample < 0 ? sample * 32768 : sample * 32767);
            output[i * 2] = (byte)(value & 0xff);
            output[i * 2 + 1] = (byte)((value >> 8) & 0xff);
        }
        return output;
    }

    private static float[] DecodeToMonoFloat(byte[] input, NAudio.Wave.WaveFormat format)
    {
        var channels = Math.Max(1, format.Channels);
        var bytesPerSample = Math.Max(1, format.BitsPerSample / 8);
        var frames = input.Length / (bytesPerSample * channels);
        var mono = new float[frames];

        for (var frame = 0; frame < frames; frame++)
        {
            double sum = 0;
            for (var channel = 0; channel < channels; channel++)
            {
                var offset = (frame * channels + channel) * bytesPerSample;
                sum += DecodeSample(input, offset, format);
            }
            mono[frame] = (float)(sum / channels);
        }

        return mono;
    }

    private static float DecodeSample(byte[] input, int offset, NAudio.Wave.WaveFormat format)
    {
        if (format.Encoding == NAudio.Wave.WaveFormatEncoding.IeeeFloat && format.BitsPerSample == 32)
            return BitConverter.ToSingle(input, offset);
        if (format.BitsPerSample == 16)
            return BitConverter.ToInt16(input, offset) / 32768f;
        if (format.BitsPerSample == 24)
        {
            var value = input[offset] | (input[offset + 1] << 8) | (input[offset + 2] << 16);
            if ((value & 0x800000) != 0) value |= unchecked((int)0xff000000);
            return value / 8388608f;
        }
        if (format.BitsPerSample == 32)
            return BitConverter.ToInt32(input, offset) / 2147483648f;
        return 0;
    }

    private static float[] ResampleLinear(float[] input, int sourceRate, int targetRate)
    {
        if (sourceRate <= 0 || sourceRate == targetRate) return input;
        var outputLength = Math.Max(1, (int)Math.Round(input.Length * (double)targetRate / sourceRate));
        var output = new float[outputLength];
        var ratio = sourceRate / (double)targetRate;
        for (var i = 0; i < outputLength; i++)
        {
            var src = i * ratio;
            var left = (int)Math.Floor(src);
            var right = Math.Min(left + 1, input.Length - 1);
            var weight = src - left;
            output[i] = (float)(input[left] * (1 - weight) + input[right] * weight);
        }
        return output;
    }
}
