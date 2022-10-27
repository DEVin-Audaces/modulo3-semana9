#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["modulo3-semana9-mensageria-academia-consumer/modulo3-semana9-mensageria-academia-consumer.csproj", "modulo3-semana9-mensageria-academia-consumer/"]
RUN dotnet restore "modulo3-semana9-mensageria-academia-consumer/modulo3-semana9-mensageria-academia-consumer.csproj"
COPY . .
WORKDIR "/src/modulo3-semana9-mensageria-academia-consumer"
RUN dotnet build "modulo3-semana9-mensageria-academia-consumer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "modulo3-semana9-mensageria-academia-consumer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "modulo3-semana9-mensageria-academia-consumer.dll"]