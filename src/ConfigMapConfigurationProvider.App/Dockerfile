#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/ConfigMapConfigurationProvider.App/ConfigMapConfigurationProvider.App.csproj", "src/ConfigMapConfigurationProvider.App/"]
RUN dotnet restore "src/ConfigMapConfigurationProvider.App/ConfigMapConfigurationProvider.App.csproj"
COPY . .
WORKDIR "/src/src/ConfigMapConfigurationProvider.App"
RUN dotnet build "ConfigMapConfigurationProvider.App.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish "ConfigMapConfigurationProvider.App.csproj" -c Release -o /app/publish --no-build

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ConfigMapConfigurationProvider.App.dll"]