using Yarp.ReverseProxy;


var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapReverseProxy();


app.MapGet("/", () => "Hello World!");

app.Run();
