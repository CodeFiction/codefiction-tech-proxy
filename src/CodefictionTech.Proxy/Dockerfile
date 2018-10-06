FROM microsoft/dotnet:2.1-aspnetcore-runtime
ARG publishFolder
ARG aspnetCoreEnv
WORKDIR /app
COPY $publishFolder .
ENV ASPNETCORE_ENVIRONMENT=$aspnetCoreEnv
EXPOSE 80
ENTRYPOINT ["dotnet", "Armut.JobSegmentService.dll"]
