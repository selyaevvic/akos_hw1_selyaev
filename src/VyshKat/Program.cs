using System.Globalization;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using VyshKat.Composition;
using VyshKat.ConsoleUi;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo("ru-RU");
CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");

var services = new ServiceCollection()
    .AddVyshKat()
    .AddSingleton(Console.In)
    .AddSingleton(Console.Out);

using var provider = services.BuildServiceProvider(new ServiceProviderOptions
{
    ValidateOnBuild = true,
    ValidateScopes = true,
});

var app = provider.GetRequiredService<FleetConsoleApp>();
if (args.Contains("--demo"))
{
    app.RunDemo();
}
else
{
    app.Run();
}
