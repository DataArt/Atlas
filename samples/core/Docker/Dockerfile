FROM microsoft/dotnet:sdk AS build
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./nuget.config ./
COPY ./SampleService/SampleService.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY ./SampleService/ ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM microsoft/dotnet:2.2-aspnetcore-runtime
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "SampleService.dll"]
EXPOSE 10001/tcp
