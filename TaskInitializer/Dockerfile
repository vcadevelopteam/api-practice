#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:3.1 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["TaskInitializer/TaskInitializer.csproj", "TaskInitializer/"]
RUN dotnet restore "TaskInitializer/TaskInitializer.csproj"
COPY . .
WORKDIR "/src/TaskInitializer"
RUN dotnet build "TaskInitializer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TaskInitializer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskInitializer.dll"]