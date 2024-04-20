using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;
using STLDotNet6.Formats.StereoLithography;

internal sealed class FileSquishCommand : Command<FileSquishCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [Description("File to convert")]
        [CommandArgument(0, "[Path]")]
        public string? Path { get; init; }

    }

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {

        STLDocument stlString;

        using (var stream = File.OpenRead(settings.Path))
        {
            stlString = STLDocument.Read(stream);
        }

        var minZ = 0.0f;
        var maxZ = 0.0f;
        stlString.ForEach(f =>
        {
            f.Vertices.ForEach(v =>
            {
                minZ = Math.Min(minZ, v.Z);
                maxZ = Math.Max(maxZ, v.Z);
            });
        });

        var heightMax = AnsiConsole.Ask<float>("What height do you want to squish below?");
        var height = AnsiConsole.Ask<float>("What height do you want to squish it to?");
        stlString.Facets.ForEach(f=>f.Vertices.ForEach(v=>{
            if (v.Z < heightMax){
                v.Z = heightMax-(1.0f-((v.Z-minZ)/(maxZ-minZ)*height));
            }
        }));
        using (var writeStream = File.OpenWrite(settings.Path+".modified.stl")){
            stlString.WriteBinary(writeStream);
        }
        return 0;

    }
}