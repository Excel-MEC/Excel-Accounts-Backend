FROM mcr.microsoft.com/dotnet/core/sdk:3.1
WORKDIR /api
RUN apt-get update && apt-get install -y libgdiplus
COPY ./API/*.csproj ./
RUN dotnet restore --disable-parallel
COPY ./API/. .
