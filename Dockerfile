FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["AnalyticsService/AnalyticsService.csproj", "AnalyticsService/"]
RUN dotnet restore "AnalyticsService/AnalyticsService.csproj"
COPY . .
WORKDIR "/src/AnalyticsService"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "AnalyticsService.dll"]
