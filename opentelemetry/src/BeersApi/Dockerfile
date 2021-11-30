#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["BeersApi/BeersApi.csproj", "BeersApi/"]
RUN dotnet restore "BeersApi/BeersApi.csproj"
COPY . .
WORKDIR "/src/BeersApi"
RUN dotnet build "BeersApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BeersApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BeersApi.dll"]