#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Minecraft-DiscrdBot-Core/Minecraft-DiscrdBot-Core.csproj", "Minecraft-DiscrdBot-Core/"]
RUN dotnet restore "Minecraft-DiscrdBot-Core/Minecraft-DiscrdBot-Core.csproj"
COPY . .
WORKDIR "/src/Minecraft-DiscrdBot-Core"
RUN dotnet build "Minecraft-DiscrdBot-Core.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Minecraft-DiscrdBot-Core.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Minecraft-DiscrdBot-Core.dll"]