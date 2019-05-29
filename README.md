# Introduction

This repository just contains some of my micro benchmarks regarding intrinsics in .NET.

Please beware that micro benchmarks are usually *not* a good way of measuring performance.

# XOR

Here I want to use `Intrinsics` to XOR bytes as fast as possible. The way I found using `Intrinsics` (or SIMD) is compared to the fastest way possible without `Intrinsics`.

Currently this is the fastest way of XORing on my Hardware:

```csharp
Avx2.Store(ppDiff, Avx2.Xor(Avx2.LoadVector256(ppOld), Avx2.LoadVector256(ppNew)));
```

# XORParallel

I observed that various techniques don't work well in parallel.

In this case AVX2 just needs ~10 ms when using one core, but 80 ms when using two cores. I asked this question on StackOverflow: [.NET Core 3 Intrinsics AVX2 used in parallel have low performance](https://stackoverflow.com/questions/56362254/net-core-3-intrinsics-avx2-used-in-parallel-has-low-performance).

# XORParallelWithoutMemory

I developed those tests because of Mysticials (StackOverflow) intervention that cache coherency overhead may be the problem.

The results are way better now and as I would expect.

This leads me to the question where this problem may come from. I suspect the CLR to write everything always to RAM, because the content of the `byte[]` is also available outside the test-function scope.

# XORParallelSameScopeMemory

This test tries to find out, if the variable scope does matter for performance.