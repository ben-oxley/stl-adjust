using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;
using STLDotNet6.Formats.StereoLithography;

internal sealed class FileHistogramCommand : Command<FileHistogramCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [Description("File to convert")]
        [CommandArgument(0, "[Path]")]
        public string? SearchPath { get; init; }

    }

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {

        var searchPath = settings.SearchPath;
        var files = new FileInfo(searchPath);

        var totalFileSize = files.Length;

        AnsiConsole.MarkupLine($"Total file size for [green]{searchPath}[/]: [blue]{totalFileSize:N0}[/] bytes");

        // Synchronous
        AnsiConsole.Progress().Start(ctx =>
        {

            // Define tasks
            var loadingTask = ctx.AddTask("[green]Loading File[/]");
            var boundsTask = ctx.AddTask("[green]Calculating Bounds[/]");
            var histogramTask = ctx.AddTask("[green]Generating Histogram[/]");

            STLDocument stlString;

            using (var stream = File.OpenRead(settings.SearchPath))
            {
                stlString = STLDocument.Read(stream);
            }

            loadingTask.Value = 100;

            var minZ = double.MaxValue;
            var maxZ = double.MinValue;

            stlString.ForEach(f =>
            {
                boundsTask.Increment(100.0/stlString.Facets.Count);
                f.Vertices.ForEach(v =>
                {
                    
                    minZ = Math.Min(minZ, v.Z);
                    maxZ = Math.Max(maxZ, v.Z);
                });
            });
            boundsTask.Value=100;

            AnsiConsole.MarkupLine($"Min Z height is [green]{minZ}[/] and max is [blue]{maxZ}[/]");



            var buckets = new int[21];
            var step = (maxZ - minZ) / 20.0;
            stlString.ForEach(f =>
            {
                histogramTask.Increment(100.0/stlString.Facets.Count);
                f.Vertices.ForEach(v =>
                {
                    buckets[(int)Math.Floor((v.Z - minZ) / step)] += 1;
                });
            });

            histogramTask.Value = 100;

            Dictionary<double, int> histogram = ToHistogram(buckets, step, minZ);

            BarChart barChart = new BarChart()
                .Width(60)
                .Label("[green bold underline]Histogram[/]")
                .CenterLabel().AddItems(histogram.Reverse(), (item) => new BarChartItem(
                    item.Key.ToString("0.00"), Math.Log10(item.Value), Color.Red.Blend(Color.Blue, (float)((item.Key - minZ) / (maxZ - minZ)))));

            AnsiConsole.Write(barChart);

        });
        return 0;
    }

    private static Dictionary<double, int> ToHistogram(int[] buckets, double step, double minZ)
    {
        var i = 0;
        var histogram = new Dictionary<double, int>();

        buckets.ForEach(b =>
        {
            histogram[(i * step) + minZ] = buckets[i];
            i++;
        });
        return histogram;
    }
}