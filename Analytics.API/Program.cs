using Analytics.API.Services;
using Analytics.Application;
using Analytics.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddGrpc();

var app = builder.Build();

app.MapGrpcService<AnalyticsGrpcService>();
app.MapGrpcService<RecommendationsGrpcService>();
app.MapGet("/", () => "AnalyticsService gRPC is running.");

app.Run();
