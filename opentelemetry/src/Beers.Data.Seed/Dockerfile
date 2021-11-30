#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Beers.Data.Seed/Beers.Data.Seed.csproj", "Beers.Data.Seed/"]
COPY ["Beers.Data/Beers.Data.csproj", "Beers.Data/"]
RUN dotnet restore "Beers.Data.Seed/Beers.Data.Seed.csproj"
COPY . .
WORKDIR "/src/Beers.Data.Seed"
RUN dotnet build "Beers.Data.Seed.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Beers.Data.Seed.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Beers.Data.Seed.dll"]