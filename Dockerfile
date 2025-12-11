FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Analytics.API/Analytics.API.csproj", "Analytics.API/"]
COPY ["Analytics.Application/Analytics.Application.csproj", "Analytics.Application/"]
COPY ["Analytics.Domain/Analytics.Domain.csproj", "Analytics.Domain/"]
COPY ["Analytics.Infrastructure/Analytics.Infrastructure.csproj", "Analytics.Infrastructure/"]
RUN dotnet restore "Analytics.API/Analytics.API.csproj"
COPY . .
WORKDIR "/src/Analytics.API"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "Analytics.API.dll"]
