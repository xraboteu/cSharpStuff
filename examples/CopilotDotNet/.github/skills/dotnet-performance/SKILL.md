---
name: dotnet-performance
description: Profile and optimize .NET performance, allocations, concurrency and EF Core queries with measurable evidence.
---

# .NET performance optimization

## Workflow
1. Define the workload, performance objective and baseline (latency, throughput, allocations).
2. Reproduce and measure with BenchmarkDotNet, traces, dotnet-counters or representative load tests.
3. Identify bottlenecks before changing code.
4. Evaluate query shape, indexes, tracking, pagination and N+1 issues for EF Core.
5. Consider allocations, boxing, closures, Span/Memory/ArrayPool and GC pressure where profiling justifies them.
6. Review async, cancellation, contention, ThreadPool and concurrency correctness.
7. Make the smallest safe change, then compare measurements under the same conditions.
8. Run correctness and regression tests; document before/after and trade-offs.

## Guardrails
- Never assert that a change is faster without measured evidence.
- Do not introduce pooling, unsafe code or complex caching without a justified benefit.
- Do not optimize away cancellation, validation, correctness or thread safety.
- Distinguish microbenchmarks from end-to-end application performance.
