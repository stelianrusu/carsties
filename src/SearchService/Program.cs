using MongoDB.Driver;
using MongoDB.Entities;
using Polly;
using Polly.Extensions.Http;
using SearchService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpClient<AuctionSvcHttpClient>()
    .AddPolicyHandler(GetPolicy());


var app = builder.Build();

app.Lifetime.ApplicationStarted.Register(async () => {
try{
    await DbInitializer.InitDb(app);
}
catch(Exception e){
    System.Console.WriteLine(e);
}
});

app.UseAuthorization();

app.MapControllers();



app.Run();

static IAsyncPolicy<HttpResponseMessage> GetPolicy() =>
    HttpPolicyExtensions.HandleTransientHttpError()
    .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));