FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ["CodefictionTech.Proxy/CodefictionTech.Proxy.csproj", "CodefictionTech.Proxy/"]
COPY ["CodefictionTech.Proxy.Core/CodefictionTech.Proxy.Core.csproj", "CodefictionTech.Proxy.Core/"]
RUN dotnet restore "CodefictionTech.Proxy/CodefictionTech.Proxy.csproj"
COPY . .
WORKDIR "/src/CodefictionTech.Proxy"
RUN dotnet build "CodefictionTech.Proxy.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "CodefictionTech.Proxy.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "CodefictionTech.Proxy.dll"]