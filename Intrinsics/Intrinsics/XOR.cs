using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace Intrinsics
{
    public class XOR
    {
        /// <summary>
        /// This simulates the content of a 24bpp FHD screen.
        /// </summary>
        /// <returns>Three byte[] representing two randomly filled byte arrays and the target where the xor code should be sent to.</returns>
        public IEnumerable<object[]> DataSourceBytes()
        {
            Random rng = new Random();

            byte[] src = new byte[6220800];
            byte[] dst = new byte[6220800];
            byte[] result = new byte[6220800];

            rng.NextBytes(src);
            rng.NextBytes(dst);

            for (int position = 0; position < result.Length; position++)
                result[position] = 0;

            yield return new object[] { src, dst, result };
        }

        /// <summary>
        /// This simulates the content of a 24bpp FHD screen.
        /// </summary>
        /// <returns>Three long[] representing two randomly filled byte arrays and the target where the xor code should be sent to.</returns>
        public IEnumerable<object[]> DataSourceLongs()
        {
            Random rng = new Random();

            long[] src = new long[777600];
            long[] dst = new long[777600];
            long[] result = new long[777600];

            fillRandom(rng, src);
            fillRandom(rng, dst);

            for (int position = 0; position < result.Length; position++)
                result[position] = 0;

            yield return new object[] { src, dst, result };
        }

        public unsafe void fillRandom(Random rng, long[] data)
        {
            fixed (long* tMem = data)
                rng.NextBytes(new Span<byte>((byte*)tMem, data.Length * 8));
        }

        [Benchmark]
        [ArgumentsSource(nameof(DataSourceBytes))]
        public void Manual(byte[] oldScreen, byte[] newScreen, byte[] difference)
        {
            for (int position = 0; position < difference.Length; position++)
                difference[position] = (byte)(oldScreen[position] ^ newScreen[position]);
        }

        [Benchmark]
        [ArgumentsSource(nameof(DataSourceBytes))]
        public unsafe void BytePointers(byte[] oldScreen, byte[] newScreen, byte[] difference)
        {
            int steps = difference.Length / 8;

            fixed (byte* pOld = oldScreen)
            fixed (byte* pNew = newScreen)
            fixed (byte* pDiff = difference)
            {
                byte* ppOld = pOld;
                byte* ppNew = pNew;
                byte* ppDiff = pDiff;

                for (int position = 0; position < steps; ppOld += 8, ppNew += 8, ppDiff += 8, position++)
                    *(long*)ppDiff = *(long*)ppOld ^ *(long*)ppNew;
            }
        }

        [Benchmark]
        [ArgumentsSource(nameof(DataSourceBytes))]
        public unsafe void LongPointers(byte[] oldScreen, byte[] newScreen, byte[] difference)
        {
            int steps = difference.Length / 8;

            fixed (byte* pOld = oldScreen)
            fixed (byte* pNew = newScreen)
            fixed (byte* pDiff = difference)
            {
                long* ppOld = (long*)pOld;
                long* ppNew = (long*)pNew;
                long* ppDiff = (long*)pDiff;

                for (int position = 0; position < steps; ppOld++, ppNew++, ppDiff++, position++)
                    *ppDiff = *ppOld ^ *ppNew;
            }
        }

        [Benchmark]
        [ArgumentsSource(nameof(DataSourceBytes))]
        public unsafe void IntrinsicsAVX(byte[] oldScreen, byte[] newScreen, byte[] difference)
        {
            int steps = difference.Length / 16;

            fixed (byte* pOld = oldScreen)
            fixed (byte* pNew = newScreen)
            fixed (byte* pDiff = difference)
            {
                long* ppOld = (long*)pOld;
                long* ppNew = (long*)pNew;
                long* ppDiff = (long*)pDiff;

                for (int position = 0; position < steps; ppOld += 2, ppNew += 2, ppDiff += 2, position++)
                    Avx.Store(ppDiff, Avx.Xor(Avx.LoadVector128(ppOld), Avx.LoadVector128(ppNew)));
            }
        }

        [Benchmark]
        [ArgumentsSource(nameof(DataSourceBytes))]
        public unsafe void IntrinsicsAVX2(byte[] oldScreen, byte[] newScreen, byte[] difference)
        {
            int steps = difference.Length / 32;

            fixed (byte* pOld = oldScreen)
            fixed (byte* pNew = newScreen)
            fixed (byte* pDiff = difference)
            {
                long* ppOld = (long*)pOld;
                long* ppNew = (long*)pNew;
                long* ppDiff = (long*)pDiff;

                for (int position = 0; position < steps; ppOld += 4, ppNew += 4, ppDiff += 4, position++)
                    Avx2.Store(ppDiff, Avx2.Xor(Avx2.LoadVector256(ppOld), Avx2.LoadVector256(ppNew)));
            }
        }

        [Benchmark]
        [ArgumentsSource(nameof(DataSourceBytes))]
        public void SIMDBytes(byte[] oldScreen, byte[] newScreen, byte[] difference)
        {
            int simdBlocks = Vector<byte>.Count;
            int steps = difference.Length;

            for (int position = 0; position < steps; position += simdBlocks)
                (new Vector<byte>(oldScreen, position) ^ new Vector<byte>(newScreen, position)).CopyTo(difference, position);
        }

        [Benchmark]
        [ArgumentsSource(nameof(DataSourceLongs))]
        public void SIMDLong(long[] oldScreen, long[] newScreen, long[] difference)
        {
            int simdBlocks = Vector<long>.Count;
            int steps = difference.Length;

            for (int position = 0; position < steps; position += simdBlocks)
                (new Vector<long>(oldScreen, position) ^ new Vector<long>(newScreen, position)).CopyTo(difference, position);
        }
    }
}
