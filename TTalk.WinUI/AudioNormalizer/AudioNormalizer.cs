using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TTalk.WinUI.AudioNormalizer
{
    public class AudioNormalizer
    {
        //private DynamicAudioNormalizerNET _instance;

        public AudioNormalizer()
        {
            //_instance = new DynamicAudioNormalizerNET(1, 48000, 40, 31, 0.95, 10.0, 0.0, 0.0, true, false, false);
        }


        public double[] Normalize(double[] samples)
        {
            double[,] arr = new double[1, samples.Length];
            for (int i = 0; i < samples.Length; ++i)
                arr[0, i] = samples[i];

            double[,] @out = new double[samples.Length, samples.Length];

            //int outSize = (int)_instance.process(arr, @out, samples.LongLength);
            //return GetRow(@out, 0).Slice(0, outSize);
            return new double[0];
        }


        private static T[] GetRow<T>(T[,] array, int row)
        {
            if (!typeof(T).IsPrimitive)
                throw new InvalidOperationException("Not supported for managed types.");

            if (array == null)
                throw new ArgumentNullException("array");

            int cols = array.GetUpperBound(1) + 1;
            T[] result = new T[cols];

            int size;

            if (typeof(T) == typeof(bool))
                size = 1;
            else if (typeof(T) == typeof(char))
                size = 2;
            else
                size = Marshal.SizeOf<T>();

            Buffer.BlockCopy(array, row * cols * size, result, 0, cols * size);

            return result;
        }

    }
}
