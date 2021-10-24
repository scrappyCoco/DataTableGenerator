using System.Globalization;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using CsvHelper;

namespace Coding4fun.DataTableGenerator.Benchmark
{
    [SimpleJob(RuntimeMoniker.Net50 | RuntimeMoniker.Mono)]
    [RPlotExporter, RankColumn]
    public class DataBuilderBenchmark
    {
        private CaseStat[] records;

        [Params(1000, 10000)]
        public int N;

        [GlobalSetup]
        public void Setup()
        {
            // Download it from https://github.com/owid/covid-19-data/blob/master/public/data/owid-covid-data.csv
            using var reader = new StreamReader("/Users/artemkorsunov/Downloads/owid-covid-data.csv");
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            records = csv.GetRecords<CaseStat>().Take(N).ToArray();
        }

        [Benchmark]
        public void Sha256()
        {
            foreach (var caseStat in records)
            {
                //new CaseStatSqlMapping().AddSTAT(caseStat);
            }
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<DataBuilderBenchmark>();
        }
    }
}