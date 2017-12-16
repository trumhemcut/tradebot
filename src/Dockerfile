# FROM microsoft/dotnet:2.0-sdk AS build-env
FROM microsoft/dotnet AS build-env
WORKDIR /app

# copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out
COPY ./appsettings.dev.json ./out/appsettings.dev.json

# build runtime image
FROM microsoft/dotnet:runtime 
WORKDIR /app
COPY --from=build-env /app/out ./
ENTRYPOINT ["dotnet", "tradebot.dll"]