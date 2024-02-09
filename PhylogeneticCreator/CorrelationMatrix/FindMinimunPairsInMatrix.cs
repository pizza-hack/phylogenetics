// Daniel Muñoz
// 08-02-2024
// PhylogeneticCreator - Bechmarkings.cs

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace PhylogeneticCreator;

[MemoryDiagnoser]
public class FindMinimunPairsInMatrix
{
    private const int N = 500;
    private float[] _matrix = new float[N*N];
    private ImmutableList<float> _listInmutable;
    
    public FindMinimunPairsInMatrix()
    {
        // Fill the matrix with random values from 0 to 1
        var random = new Random();
        for (int i = 0; i < N; i++)
        {
            for (int j = 0; j < N; j++)
            {
                _matrix[i * N + j] = (float) random.NextDouble();
            }
        }
        
        _listInmutable = Unsafe.As<float[], ImmutableList<float>>(ref _matrix);
    }
    
    [GlobalSetup]
    [GlobalCleanup]
    public void Dispose()
    {
        _listInmutable = ImmutableList<float>.Empty;
        _matrix = Array.Empty<float>();
    }

    [Benchmark]
    public void Base()
    {
        List<Tuple<int, int>> returnPairs = new List<Tuple<int, int>>();
        while (_matrix.Cast<float>().Count(value => value > 0) / 2 > 1)
        {
            // Step 2: Identify the pair with the highest correlation
            Tuple<int, int> maxCorrelationIndices = FindMaxCorrelation(_matrix);

            // Output the result or store it as needed
            returnPairs.Add(maxCorrelationIndices);
            
            // Replace the existing row and column with the combined values
            ReplaceRowAndColumn(
                _matrix, 
                maxCorrelationIndices.Item1, 
                maxCorrelationIndices.Item2);

            // Set the second element to zero (or any sentinel value)
            _matrix[maxCorrelationIndices.Item2 * N + maxCorrelationIndices.Item1] = -1;

        }
    }
    
    public Tuple<int,int> FindMaxCorrelation(float[] matrix)
    {
        Tuple<int, int> maxCorrelationIndices = new Tuple<int, int>(-1, -1);
        float maxCorrelation = float.MaxValue;
        for (int i = 0; i < N; i++)
        {
            for (int j = i + 1; j < N; j++)
            {
                if (matrix[i*N+j] < maxCorrelation && matrix[i*N+j] >= 0 && i != j)
                {
                    maxCorrelation = matrix[i*N + j];
                    maxCorrelationIndices = new Tuple<int, int>(i, j);
                }
            }
        }
        
        return maxCorrelationIndices;
    }
    
    public void ReplaceRowAndColumn(float[] matrix, int rowIndex, int columnIndex)
    {
        // Replace the new row with the combined values
        // Since its a symmetrical matrix we replace the column as well
        for (int i = 0; i < N; i++)
        {
            var mean = (matrix[rowIndex*N + i] + matrix[columnIndex*N + i]) / 2;
            matrix[rowIndex*N + i] = mean;
            matrix[i*N + rowIndex] = mean;
        }
        
        // Set the diagonal to 0
        matrix[rowIndex*N + rowIndex] = 0;
        
        // Set the second elements row and column to -1 (or any sentinel value)
        for (int i = 0; i < N; i++)
        {
            matrix[columnIndex*N + i] = -1;
            matrix[i*N + columnIndex] = -1;
        }
    }
    
    [Benchmark]
    public void Immutable()
    {
        List<Tuple<int, int>> returnPairs = new List<Tuple<int, int>>();
        while (_listInmutable.Count(value => value > 0) / 2 > 1)
        {
            // Step 2: Identify the pair with the highest correlation
            Tuple<int, int> maxCorrelationIndices = FindMinimunPairsImmutable(_listInmutable);

            // Output the result or store it as needed
            returnPairs.Add(maxCorrelationIndices);
            
            // Replace the existing row and column with the combined values
            ReplaceRowAndColumnInmutable(
                _listInmutable, 
                maxCorrelationIndices.Item1 , 
                maxCorrelationIndices.Item2 );

            // Set the second element to zero (or any sentinel value)
            _listInmutable = _listInmutable.SetItem(maxCorrelationIndices.Item2 * N + maxCorrelationIndices.Item1, -1);
        }
    }
    
    public Tuple<int,int> FindMinimunPairsImmutable(ImmutableList<float> matrix)
    {
        Tuple<int, int> minimunPairs = new Tuple<int, int>(-1, -1);
        float maxCorrelation = float.MaxValue;
        for (int i = 0; i < N; i++)
        {
            for (int j = i + 1; j < N; j++)
            {
                if (_listInmutable[i * N + j] > maxCorrelation && _listInmutable[i * N + j] >= 0 && i != j)
                {
                    maxCorrelation = _listInmutable[i * N + j];
                    minimunPairs = new Tuple<int, int>(i, j);
                }
            }
        }
        
        return minimunPairs;
    }
    
    public void ReplaceRowAndColumnInmutable(ImmutableList<float> matrix, int rowIndex, int columnIndex)
    {
        // Replace the new row with the combined values
        // Since its a symmetrical matrix we replace the column as well
        for (int i = 0; i < N; i++)
        {
            var mean = (matrix[rowIndex * N + i] + matrix[columnIndex * N + i]) / 2;
            _listInmutable = _listInmutable.SetItem(rowIndex * N + i, mean);
            _listInmutable = _listInmutable.SetItem(i * N + rowIndex, mean);
        }
        
        // Set the diagonal to 0
        _listInmutable = _listInmutable.SetItem(rowIndex * N + rowIndex, 0);
        
        // Set the second elements row and column to -1 (or any sentinel value)
        for (int i = 0; i < N; i++)
        {
            _listInmutable = _listInmutable.SetItem(columnIndex * N + i, -1);
            _listInmutable = _listInmutable.SetItem(i * N + columnIndex, -1);
        }
    }
}