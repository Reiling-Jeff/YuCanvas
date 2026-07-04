using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia;
using Microsoft.Extensions.Configuration;

namespace YuCanvas;

internal class Program
{
    public static IConfiguration Configuration { get; private set; } = null!;

    public static void Main(string[] args)
    {
        Configuration = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    private static async Task TestCanvasConnection()
    {
        string? baseUrl = Configuration["Canvas:BaseUrl"];
        string? token = Configuration["Canvas:Token"];

        using HttpClient http = new HttpClient { BaseAddress = new Uri(baseUrl!) };
        http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        try
        {
            string response = await http.GetStringAsync("/api/v1/courses/23259/assignments?include[]=submission&per_page=100");
            Console.WriteLine("=== CANVAS ANTWORT ===");
            Console.WriteLine(response);
            Console.WriteLine("======================");
        }
        catch (Exception ex)
        {
            Console.WriteLine("=== CANVAS FEHLER ===");
            Console.WriteLine(ex.Message);
            Console.WriteLine("=====================");
        }
    }

    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .WithInterFont()
        .LogToTrace();
}
