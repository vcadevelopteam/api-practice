#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
EXPOSE 56377

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["ZyxmeBridge/ZyxmeBridge.csproj", "ZyxmeBridge/"]
RUN dotnet restore "ZyxmeBridge/ZyxmeBridge.csproj"
COPY . .
WORKDIR "/src/ZyxmeBridge"
RUN dotnet build "ZyxmeBridge.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ZyxmeBridge.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ZyxMeBridge.dll"]