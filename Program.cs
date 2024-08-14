using eBayExtension.Contracts;
using eBayExtension.Hubs;
using eBayExtension.Models;
using eBayExtension.Services;
using System.Threading.Channels;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();

// Add services to the container.

builder.Services.AddHttpClient<IEbayDataService, EbayDataService>((serviceProvider, client) =>
{
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.BaseAddress = new Uri("https://jsonplaceholder.typicode.com");
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("defaultPolicy", policy =>
    {
        //policy.SetIsOriginAllowed(orign => true);
        policy.WithMethods("GET", "POST");
        policy.AllowAnyHeader();
        policy.AllowCredentials();
        policy.WithOrigins("http://127.0.0.1:51072");
    });
});
builder.Services.AddControllers();
// builder.Services.AddSingleton<ConcurrentDictionary<string, List<FilterTimer>>>();
builder.Services.AddSingleton(Channel.CreateUnbounded<ConnectionWithFilter>());
builder.Services.AddSingleton<TimerManager>();
builder.Services.AddHostedService<TimedHostedService>();
builder.Services.AddSingleton<SendDataService>();
//builder.Services.AddTransient<TimeManager>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("defaultPolicy");
app.UseAuthorization();

app.MapControllers();


app.MapHub<EbayApiDataHub>("/filtered-data");

app.Run();
