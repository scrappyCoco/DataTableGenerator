using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using CsvHelper;
using CsvHelper.Configuration;
using JetBrains.Annotations;

namespace Coding4fun.DataTableGenerator.Benchmark
{
    [SimpleJob(RuntimeMoniker.Net50, targetCount: 20)]
    [RankColumn]
    public class DataBuilderBenchmark
    {
        private CaseStat[] _records;
        private SqlMappingWithExpression _sqlMappingWithExpression;
        private CaseStatSqlMapping _caseStatSqlMapping;

        [Params(1000, 10_000, 100_000)] [UsedImplicitly] public int N;

        [GlobalSetup]
        public void Setup()
        {
            // Download it from https://github.com/owid/covid-19-data/blob/master/public/data/owid-covid-data.csv
            using var reader = new StreamReader(@"C:\Users\korsh\Downloads\owid-covid-data.csv.txt");
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ","
            });
            _records = csv.GetRecords<CaseStat>().Take(N).ToArray();

            _sqlMappingWithExpression = SqlMappingWithExpression.Create();
            _caseStatSqlMapping = new CaseStatSqlMapping();
        }

        [Benchmark]
        public DataTable Generated()
        {
            _caseStatSqlMapping.FillDataTables(_records);
            return _caseStatSqlMapping.STAT;
        }

        [Benchmark]
        public DataTable Expression()
        {
            _sqlMappingWithExpression.AddRows(_records);
            return _sqlMappingWithExpression.DataTable;
        }
    }

    public static class Program
    {
        public static void Main() => BenchmarkRunner.Run<DataBuilderBenchmark>();
    }
}