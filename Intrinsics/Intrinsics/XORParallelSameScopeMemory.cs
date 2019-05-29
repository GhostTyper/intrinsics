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
    /// Same like XORParallel but with strictly local memory.
    /// </summary>
    public class XORParallelSameScopeMemory
    {
        /// <summary>
        /// Specifies the number of threads.
        /// </summary>
        /// <returns>The amount of cores.</returns>
        public IEnumerable<int> DataSourceCores()
        {
            for (int cores = 1; cores <= 8; cores *= 2)
                yield return cores;
        }

        private unsafe void fillRandom(Random rng, long[] data)
        {
            fixed (long* tMem = data)
                rng.NextBytes(new Span<byte>((byte*)tMem, data.Length * 8));
        }

        [Benchmark]
        [ArgumentsSource(nameof(DataSourceCores))]
        public void Baseline(int cores)
        {
            int max = 1048576 / cores;

            Parallel.For(1, cores + 1, index =>
            {
                byte[] oldScreen = new byte[256];
                byte[] newScreen = new byte[256];
                byte[] difference = new byte[256];

                Random rng = new Random();

                rng.NextBytes(oldScreen);
                rng.NextBytes(newScreen);

                for (int position = 0; position < 256; position++)
                    difference[position] = 0x00;
            });
        }

        [Benchmark]
        [ArgumentsSource(nameof(DataSourceCores))]
        public void Manual(int cores)
        {
            Parallel.For(1, cores + 1, index =>
            {
                int max = 1048576 / cores;

                byte[] oldScreen = new byte[256];
                byte[] newScreen = new byte[256];
                byte[] difference = new byte[256];

                Random rng = new Random();

                rng.NextBytes(oldScreen);
                rng.NextBytes(newScreen);

                for (int position = 0; position < 256; position++)
                    difference[position] = 0x00;

                for (int bufCnt = 0; bufCnt < max; bufCnt++)
                    for (int position = 0; position < difference.Length; position++)
                        difference[position] = (byte)(oldScreen[position] ^ newScreen[position]);
            });
        }

        [Benchmark]
        [ArgumentsSource(nameof(DataSourceCores))]
        public unsafe void LongPointers(int cores)
        {
            Parallel.For(1, cores + 1, index =>
            {
                int max = 1048576 / cores;

                byte[] oldScreen = new byte[256];
                byte[] newScreen = new byte[256];
                byte[] difference = new byte[256];

                Random rng = new Random();

                rng.NextBytes(oldScreen);
                rng.NextBytes(newScreen);

                for (int position = 0; position < 256; position++)
                    difference[position] = 0x00;

                int steps = difference.Length / 32;

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
        [ArgumentsSource(nameof(DataSourceCores))]
        public unsafe void IntrinsicsAVX(int cores)
        {
            Parallel.For(1, cores + 1, index =>
            {
                int max = 1048576 / cores;

                byte[] oldScreen = new byte[256];
                byte[] newScreen = new byte[256];
                byte[] difference = new byte[256];

                Random rng = new Random();

                rng.NextBytes(oldScreen);
                rng.NextBytes(newScreen);

                for (int position = 0; position < 256; position++)
                    difference[position] = 0x00;

                int steps = difference.Length / 16;

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
        [ArgumentsSource(nameof(DataSourceCores))]
        public unsafe void IntrinsicsAVX2(int cores)
        {
            Parallel.For(1, cores + 1, index =>
            {
                int max = 1048576 / cores;

                byte[] oldScreen = new byte[256];
                byte[] newScreen = new byte[256];
                byte[] difference = new byte[256];

                Random rng = new Random();

                rng.NextBytes(oldScreen);
                rng.NextBytes(newScreen);

                for (int position = 0; position < 256; position++)
                    difference[position] = 0x00;

                int steps = difference.Length / 32;

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
        [ArgumentsSource(nameof(DataSourceCores))]
        public void SIMDBytes(int cores)
        {
            Parallel.For(1, cores + 1, index =>
            {
                int max = 1048576 / cores;
                int simdBlocks = Vector<byte>.Count;

                byte[] oldScreen = new byte[256];
                byte[] newScreen = new byte[256];
                byte[] difference = new byte[256];

                Random rng = new Random();

                rng.NextBytes(oldScreen);
                rng.NextBytes(newScreen);

                for (int position = 0; position < 256; position++)
                    difference[position] = 0x00;

                int steps = difference.Length;

                for (int bufCnt = 0; bufCnt < max; bufCnt++)
                    for (int position = 0; position < steps; position += simdBlocks)
                        (new Vector<byte>(oldScreen, position) ^ new Vector<byte>(newScreen, position)).CopyTo(difference, position);
            });
        }

        [Benchmark]
        [ArgumentsSource(nameof(DataSourceCores))]
        public void SIMDLong(int cores)
        {
            Parallel.For(1, cores + 1, index =>
            {
                int simdBlocks = Vector<long>.Count;

                int max = 1048576 / cores;

                long[] oldScreen = new long[32];
                long[] newScreen = new long[32];
                long[] difference = new long[32];

                Random rng = new Random();

                fillRandom(rng, oldScreen);
                fillRandom(rng, newScreen);

                for (int position = 0; position < 32; position++)
                    difference[position] = 0x00;

                int steps = difference.Length;

                for (int bufCnt = 0; bufCnt < max; bufCnt++)
                    for (int position = 0; position < steps; position += simdBlocks)
                        (new Vector<long>(oldScreen, position) ^ new Vector<long>(newScreen, position)).CopyTo(difference, position);
            });
        }
    }
}
