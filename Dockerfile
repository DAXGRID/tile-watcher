FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

COPY ./*sln ./

COPY ./src/TileWatcher/*.csproj ./src/TileWatcher/

RUN dotnet restore --packages ./packages

COPY . ./
WORKDIR /app/src/TileWatcher
RUN dotnet publish -c Release -o out --packages ./packages

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:8.0

# Update repos and install dependencies
RUN apt-get update \
  && apt-get -y upgrade \
  && apt-get -y install git build-essential libsqlite3-dev zlib1g-dev procps

# Create a directory and copy in all files
RUN mkdir -p /tmp/tippecanoe-src
RUN git clone -b 2.77.0 https://github.com/felt/tippecanoe.git /tmp/tippecanoe-src
WORKDIR /tmp/tippecanoe-src

# Build tippecanoe
RUN make \
  && make install

# Remove the temp directory and unneeded packages
WORKDIR /
RUN rm -rf /tmp/tippecanoe-src \
  && apt-get -y remove --purge build-essential && apt-get -y autoremove

WORKDIR /app

COPY --from=build-env /app/src/TileWatcher/out .
ENTRYPOINT ["dotnet", "TileWatcher.dll"]
