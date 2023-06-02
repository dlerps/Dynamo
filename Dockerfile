FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build

WORKDIR /src
COPY ./Dynamo.Worker/Dynamo.Worker.csproj ./
RUN dotnet restore

COPY ./Dynamo.Worker ./
RUN dotnet publish --no-restore -c Release -o /publish ./Dynamo.Worker.csproj

FROM mcr.microsoft.com/dotnet/runtime:6.0-alpine AS final

LABEL MAINTAINER=contact@lerps.de

ENV DOTNET_ENVIRONMENT=Development

WORKDIR /dynamo
COPY --from=build /publish .
RUN ls -la

ENTRYPOINT [ "dotnet", "Dynamo.Worker.dll" ]
