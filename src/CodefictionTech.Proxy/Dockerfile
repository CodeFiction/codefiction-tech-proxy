FROM microsoft/dotnet:2.1.5-aspnetcore-runtime-alpine3.7
ARG publishFolder
ARG aspnetCoreEnv
WORKDIR /app
COPY $publishFolder .
ENV ASPNETCORE_ENVIRONMENT=$aspnetCoreEnv
EXPOSE 80
ENTRYPOINT ["dotnet", "CodefictionTech.Proxy.dll"]
