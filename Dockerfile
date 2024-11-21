

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS base
WORKDIR /app

COPY *.csproj ./
RUN dotnet restore

COPY . . 

    ENTRYPOINT ["dotnet", "watch", "run", "--urls", "http://0.0.0.0:80"]
