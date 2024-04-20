using Spectre.Console.Cli;

var app = new CommandApp<FileHistogramCommand>();

app.Configure(config =>
{
    config.AddCommand<FileHistogramCommand>("histogram");
    config.AddCommand<FileSquishCommand>("squish");
});

return app.Run(args);
