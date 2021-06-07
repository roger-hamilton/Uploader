FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build

WORKDIR /app

COPY Uploader.csproj ./Uploader.csproj
RUN dotnet restore

COPY ./ ./
RUN dotnet publish -o ./dist

FROM mcr.microsoft.com/dotnet/aspnet:5.0

WORKDIR /app
COPY --from=build /app/dist ./

ENTRYPOINT [ "dotnet" ]
CMD [ "Uploader.dll" ]