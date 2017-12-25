# FROM microsoft/dotnet:2.0-sdk AS build-env
FROM microsoft/dotnet AS build-env
WORKDIR /app

COPY . ./

WORKDIR /app/tradebot.console
RUN dotnet restore

RUN dotnet publish -c Release -o out

# Use your own setting here
COPY ./tradebot.console/appsettings.dev.json ./out/appsettings.dev.json

# build runtime image
FROM microsoft/dotnet:runtime 
WORKDIR /app
COPY --from=build-env /app/tradebot.console/out ./
ENTRYPOINT ["dotnet", "tradebot.dll"]

# ADA by default
CMD [ "ADA 0.00000100" ]