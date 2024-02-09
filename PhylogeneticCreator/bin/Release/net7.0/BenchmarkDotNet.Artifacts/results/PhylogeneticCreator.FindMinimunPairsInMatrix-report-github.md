```

BenchmarkDotNet v0.13.12, Windows 10 (10.0.19045.3930/22H2/2022Update)
Intel Core i7-10750H CPU 2.60GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 7.0.304
  [Host]     : .NET 7.0.7 (7.0.723.27404), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.7 (7.0.723.27404), X64 RyuJIT AVX2


```
| Method    | Mean     | Error    | StdDev   | Gen0   | Allocated |
|---------- |---------:|---------:|---------:|-------:|----------:|
| Base      | 11.40 ns | 0.103 ns | 0.096 ns | 0.0051 |      32 B |
| Immutable | 11.69 ns | 0.035 ns | 0.031 ns | 0.0051 |      32 B |
