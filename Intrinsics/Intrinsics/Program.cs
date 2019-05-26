using BenchmarkDotNet.Running;
using System;
using System.Numerics;
using System.Runtime.Intrinsics.X86;

namespace Intrinsics
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!Avx.IsSupported)
            {
                Console.WriteLine("AVX is not supported.");

                return;
            }

            if (!Avx2.IsSupported)
            {
                Console.WriteLine("AVX2 is not supported.");

                return;
            }

            if (!Vector.IsHardwareAccelerated)
            {
                Console.WriteLine("SIMD is not supported.");

                return;
            }

            BenchmarkRunner.Run<XOR>();
        }
    }
}
