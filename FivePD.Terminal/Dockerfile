FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["FivePD.Terminal/FivePD.Terminal.csproj", "FivePD.Terminal/"]
RUN dotnet restore "FivePD.Terminal/FivePD.Terminal.csproj"
COPY . .
WORKDIR "/src/FivePD.Terminal"
RUN dotnet build "FivePD.Terminal.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FivePD.Terminal.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FivePD.Terminal.dll"]
