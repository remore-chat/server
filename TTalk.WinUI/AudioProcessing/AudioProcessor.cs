using FragLabs.Audio.Codecs;
using NWaves.Filters;
using NWaves.Filters.Bessel;
using NWaves.Signals;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TTalk.WinUI.AudioProcessing
{
    public static class AudioProcessor
    {
        private static Denoiser denoiser;
        private static PreEmphasisFilter preEmphasisFilter;
        private static RastaFilter rastaFilter;

        static AudioProcessor()
        {
            denoiser = new Denoiser();
            preEmphasisFilter = new PreEmphasisFilter(0.97, true);
            rastaFilter = new RastaFilter();
        }

        public static List<byte[]> ProcessAudio(byte[] buffer, int bytesRecorded, int bytesPerSegment, OpusEncoder encoder, bool denoise = false)
        {
            var _notEncodedBuffer = Array.Empty<byte>();
            try
            {
                if (encoder == null)
                    return null;
                byte[] bytes = buffer;
                var floats = GetFloatsFromBytes(buffer, bytesRecorded);
                var floatsSpan = floats.AsSpan();
                if (denoise)
                    denoiser.Denoise(floatsSpan, false);
                floats = floatsSpan.ToArray();
                var signal = new DiscreteSignal(encoder.InputSamplingRate, floats);
                preEmphasisFilter.ApplyTo(signal);
                rastaFilter.ApplyTo(signal);
                floats = signal.Samples;
                bytes = GetSamplesWaveData(floats, floats.Length);
                byte[] soundBuffer = new byte[bytesRecorded + _notEncodedBuffer.Length];
                for (int i = 0; i < _notEncodedBuffer.Length; i++)
                    soundBuffer[i] = _notEncodedBuffer[i];
                for (int i = 0; i < bytesRecorded; i++)
                    soundBuffer[i + _notEncodedBuffer.Length] = bytes[i];

                int byteCap = bytesPerSegment;
                int segmentCount = (int)Math.Floor((decimal)soundBuffer.Length / byteCap);
                int segmentsEnd = segmentCount * byteCap;
                int notEncodedCount = soundBuffer.Length - segmentsEnd;
                _notEncodedBuffer = new byte[notEncodedCount];
                for (int i = 0; i < notEncodedCount; i++)
                {
                    _notEncodedBuffer[i] = soundBuffer[segmentsEnd + i];
                }
                List<byte[]> chunks = new();
                for (int i = 0; i < segmentCount; i++)
                {
                    byte[] segment = new byte[byteCap];
                    for (int j = 0; j < segment.Length; j++)
                        segment[j] = soundBuffer[(i * byteCap) + j];
                    int len;
                    byte[] buff = encoder.Encode(segment, segment.Length, out len);
                    chunks.Add(buff.Slice(0, len));
                }
                return chunks;

            }
            catch (Exception ee)
            {
                return null;
            }
        }
        private static float[] GetFloatsFromBytes(byte[] buffer, int length)
        {
            float[] floats = new float[length / 2];
            int floatIndex = 0;
            for (int index = 0; index < length; index += 2)
            {
                short sample = (short)((buffer[index + 1] << 8) |
                                        buffer[index + 0]);
                // to floating point
                floats[floatIndex] = sample / 32768f;
                floatIndex++;
            }
            return floats;
        }

        private static byte[] GetSamplesWaveData(float[] samples, int samplesCount)
        {
            var pcm = new byte[samplesCount * 2];
            int sampleIndex = 0,
                pcmIndex = 0;

            while (sampleIndex < samplesCount)
            {
                var outsample = (short)(samples[sampleIndex] * short.MaxValue);
                pcm[pcmIndex] = (byte)(outsample & 0xff);
                pcm[pcmIndex + 1] = (byte)((outsample >> 8) & 0xff);

                sampleIndex++;
                pcmIndex += 2;
            }

            return pcm;
        }
    }
}
