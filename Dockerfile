FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src
COPY ./Dynamo.Worker/Dynamo.Worker.csproj ./
RUN dotnet restore

COPY ./Dynamo.Worker ./
RUN dotnet publish --no-restore -c Release -o /publish ./Dynamo.Worker.csproj

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final

LABEL MAINTAINER=contact@lerps.de

ENV DOTNET_ENVIRONMENT=Production

WORKDIR /dynamo
COPY --from=build /publish .

ENTRYPOINT [ "dotnet", "Dynamo.Worker.dll" ]
