using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace Intrinsics
{
    /// <summary>
    /// This class tests parallel execution speed of the "real world application" XOR with separate memory per thread due to the claim of Mysticial from StackOverflow.
    /// </summary>
    public class XORParallelWithoutMemory
    {
        /// <summary>
        /// Specifies the number of threads.
        /// </summary>
        /// <returns>The amount of cores.</returns>
        public IEnumerable<int> DataSource()
        {
            for (int cores = 1; cores <= 8; cores *= 2)
                yield return cores;
        }

        [Benchmark]
        [ArgumentsSource(nameof(DataSource))]
        public void Manual(int cores)
        {
            Parallel.For(1, cores + 1, index =>
            {
                int max = 1048576 / cores;
                byte stor = 0x55;

                for (int bufCnt = 0; bufCnt < max; bufCnt++)
                    for (int position = 0; position < 256; position++)
                        stor = (byte)(stor ^ 0xAA);
            });
        }

        [Benchmark]
        [ArgumentsSource(nameof(DataSource))]
        public unsafe void LongPointers(int cores)
        {
            Parallel.For(1, cores + 1, index =>
            {
                int max = 1048576 / cores;
                long[] stor = new long[] { 0x5555555555555555 };

                fixed (long* pStor = stor)
                    for (int bufCnt = 0; bufCnt < max; bufCnt++)
                        for (int position = 0; position < 32; position++)
                            *pStor = *pStor ^ 0x2AAAAAAAAAAAAAAA;
            });
        }

        [Benchmark]
        [ArgumentsSource(nameof(DataSource))]
        public unsafe void IntrinsicsAVX(int cores)
        {
            Parallel.For(1, cores + 1, index =>
            {
                int max = 1048576 / cores;

                long[] stor1 = new long[16];
                long[] stor2 = new long[16];

                for (int position = 0; position < stor1.Length; position++)
                {
                    stor1[position] = 0x5555555555555555;
                    stor2[position] = 0x2AAAAAAAAAAAAAAA;
                }

                fixed (long* pStor1 = stor1)
                fixed (long* pStor2 = stor2)
                {
                    Vector128<long> s1 = Avx.LoadVector128(pStor1);
                    Vector128<long> s2 = Avx.LoadVector128(pStor2);

                    // This may be hard to understand: I want to have 2 calls to reach 256 bytes.
                    for (int bufCnt = 0; bufCnt < max; bufCnt++)
                        s1 = Avx.Xor(s1, Avx.Xor(s1, s2)).AsInt64();
                }
            });
        }

        [Benchmark]
        [ArgumentsSource(nameof(DataSource))]
        public unsafe void IntrinsicsAVX2(int cores)
        {
            Parallel.For(1, cores + 1, index =>
            {
                int max = 1048576 / cores;

                long[] stor1 = new long[32];
                long[] stor2 = new long[32];

                for (int position = 0; position < stor1.Length; position++)
                {
                    stor1[position] = 0x5555555555555555;
                    stor2[position] = 0x2AAAAAAAAAAAAAAA;
                }

                fixed (long* pStor1 = stor1)
                fixed (long* pStor2 = stor2)
                {
                    Vector256<long> s1 = Avx.LoadVector256(pStor1);
                    Vector256<long> s2 = Avx.LoadVector256(pStor2);

                    for (int bufCnt = 0; bufCnt < max; bufCnt++)
                        s1 = Avx2.Xor(s1, s2).AsInt64();
                }
            });
        }
    }
}
