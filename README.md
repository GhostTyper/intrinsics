# Introduction

This repository just contains some of my micro benchmarks regarding intrinsics in .NET.

Please beware that micro benchmarks are usually *not* a good way of measuring performance.

# XOR

Here I want to use `Intrinsics` to XOR bytes as fast as possible. The way I found using `Intrinsics` (or SIMD) is compared to the fastest way possible without `Intrinsics`.

Currently this is the fastest way of XORing on my Hardware:

```csharp
Avx2.Store(ppDiff, Avx2.Xor(Avx2.LoadVector256(ppOld), Avx2.LoadVector256(ppNew)));
```