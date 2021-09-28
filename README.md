# Luger.LoggerProvider
Implementation of Microsoft.Extensions.Logging.ILoggerProvider client for Luger server.

## Setup using host builder 
Simply specify target bucket and Url of Luger server (by default port 7931)
```c#
public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureLogging(logging =>
            {
                logging.AddLuger(luger =>
                {
                    luger.Bucket = "project-1";
                    luger.LugerUrl = new Uri("http://localhost:7931");
                });
            })
            .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
}
```

Then use builtin ILogger interface to log.

```c#
public class HomeController : Controller
{
    private readonly ILogger<HomeController> logger;

    public HomeController(ILogger<HomeController> logger)
    {
        this.logger = logger;
    }

    public ActionResult Index()
    {
        logger.LogInformation("Hello Luger log. {ActionName}", "Index");
        return Ok();
    }
}
```