using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace Intrinsics
{
    /// <summary>
    /// This class tests parallel execution speed of the "real world application" XOR with minimal memory footprint.
    /// </summary>
    public class XORParallel
    {
        /// <summary>
        /// This gives a small test set for parallel execution.
        /// </summary>
        /// <returns>Three byte[] and the amount of cores.</returns>
        public IEnumerable<object[]> DataSourceBytes()
        {
            Random rng = new Random();

            byte[] src = new byte[512];
            byte[] dst = new byte[512];
            byte[] result = new byte[512];

            rng.NextBytes(src);
            rng.NextBytes(dst);

            for (int cores = 1; cores <= 8; cores *= 2)
            {
                for (int position = 0; position < result.Length; position++)
                    result[position] = 0;

                yield return new object[] { src, dst, result, cores };
            }
        }

        /// <summary>
        /// This gives a small test set for parallel execution.
        /// </summary>
        /// <returns>Three long[] and the amount of cores.</returns>
        public IEnumerable<object[]> DataSourceLongs()
        {
            Random rng = new Random();

            long[] src = new long[64];
            long[] dst = new long[64];
            long[] result = new long[64];

            fillRandom(rng, src);
            fillRandom(rng, dst);

            for (int cores = 1; cores <= 8; cores *= 2)
            {
                for (int position = 0; position < result.Length; position++)
                    result[position] = 0;

                yield return new object[] { src, dst, result, cores };
            }
        }

        public unsafe void fillRandom(Random rng, long[] data)
        {
            fixed (long* tMem = data)
                rng.NextBytes(new Span<byte>((byte*)tMem, data.Length * 8));
        }

        [Benchmark]
        [ArgumentsSource(nameof(DataSourceBytes))]
        public void Manual(byte[] oldScreen, byte[] newScreen, byte[] difference, int cores)
        {
            int max = 1048576 / cores;

            Parallel.For(1, cores + 1, index =>
            {
                for (int bufCnt = 0; bufCnt < max; bufCnt++)
                    for (int position = 0; position < difference.Length; position++)
                        difference[position] = (byte)(oldScreen[position] ^ newScreen[position]);
            });
        }

        [Benchmark]
        [ArgumentsSource(nameof(DataSourceBytes))]
        public unsafe void LongPointers(byte[] oldScreen, byte[] newScreen, byte[] difference, int cores)
        {
            int steps = difference.Length / 8;

            int max = 1048576 / cores;

            Parallel.For(1, cores + 1, index =>
            {
                fixed (byte* pOld = oldScreen)
                fixed (byte* pNew = newScreen)
                fixed (byte* pDiff = difference)
                    for (int bufCnt = 0; bufCnt < max; bufCnt++)
                    {
                        long* ppOld = (long*)pOld;
                        long* ppNew = (long*)pNew;
                        long* ppDiff = (long*)pDiff;

                        for (int position = 0; position < steps; ppOld++, ppNew++, ppDiff++, position++)
                            *ppDiff = *ppOld ^ *ppNew;
                    }
            });
        }

        [Benchmark]
        [ArgumentsSource(nameof(DataSourceBytes))]
        public unsafe void IntrinsicsAVX(byte[] oldScreen, byte[] newScreen, byte[] difference, int cores)
        {
            int steps = difference.Length / 16;

            int max = 1048576 / cores;

            Parallel.For(1, cores + 1, index =>
            {
                fixed (byte* pOld = oldScreen)
                fixed (byte* pNew = newScreen)
                fixed (byte* pDiff = difference)
                    for (int bufCnt = 0; bufCnt < max; bufCnt++)
                    {
                        long* ppOld = (long*)pOld;
                        long* ppNew = (long*)pNew;
                        long* ppDiff = (long*)pDiff;

                        for (int position = 0; position < steps; ppOld += 2, ppNew += 2, ppDiff += 2, position++)
                            Avx.Store(ppDiff, Avx.Xor(Avx.LoadVector128(ppOld), Avx.LoadVector128(ppNew)));
                    }
            });
        }

        [Benchmark]
        [ArgumentsSource(nameof(DataSourceBytes))]
        public unsafe void IntrinsicsAVX2(byte[] oldScreen, byte[] newScreen, byte[] difference, int cores)
        {
            int steps = difference.Length / 32;

            int max = 1048576 / cores;

            Parallel.For(1, cores + 1, index =>
            {
                fixed (byte* pOld = oldScreen)
                fixed (byte* pNew = newScreen)
                fixed (byte* pDiff = difference)
                    for (int bufCnt = 0; bufCnt < max; bufCnt++)
                    {
                        long* ppOld = (long*)pOld;
                        long* ppNew = (long*)pNew;
                        long* ppDiff = (long*)pDiff;

                        for (int position = 0; position < steps; ppOld += 4, ppNew += 4, ppDiff += 4, position++)
                            Avx2.Store(ppDiff, Avx2.Xor(Avx2.LoadVector256(ppOld), Avx2.LoadVector256(ppNew)));
                    }
            });
        }

        [Benchmark]
        [ArgumentsSource(nameof(DataSourceBytes))]
        public void SIMDBytes(byte[] oldScreen, byte[] newScreen, byte[] difference, int cores)
        {
            int simdBlocks = Vector<byte>.Count;
            int steps = difference.Length;

            int max = 1048576 / cores;

            Parallel.For(1, cores + 1, index =>
            {
                for (int bufCnt = 0; bufCnt < max; bufCnt++)
                    for (int position = 0; position < steps; position += simdBlocks)
                        (new Vector<byte>(oldScreen, position) ^ new Vector<byte>(newScreen, position)).CopyTo(difference, position);
            });
        }

        [Benchmark]
        [ArgumentsSource(nameof(DataSourceLongs))]
        public void SIMDLong(long[] oldScreen, long[] newScreen, long[] difference, int cores)
        {
            int simdBlocks = Vector<long>.Count;
            int steps = difference.Length;

            int max = 1048576 / cores;

            Parallel.For(1, cores + 1, index =>
            {
                for (int bufCnt = 0; bufCnt < max; bufCnt++)
                    for (int position = 0; position < steps; position += simdBlocks)
                        (new Vector<long>(oldScreen, position) ^ new Vector<long>(newScreen, position)).CopyTo(difference, position);
            });
        }
    }
}