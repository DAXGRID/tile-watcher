FROM mcr.microsoft.com/dotnet/sdk:5.0.300-buster-slim AS build-env
WORKDIR /app

COPY ./*sln ./

COPY ./src/TileWatcher/*.csproj ./src/TileWatcher/

RUN dotnet restore --packages ./packages

COPY . ./
WORKDIR /app/src/TileWatcher
RUN dotnet publish -c Release -o out --packages ./packages

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:5.0

# Update repos and install dependencies
RUN apt-get update \
  && apt-get -y upgrade \
  && apt-get -y install git build-essential libsqlite3-dev zlib1g-dev procps

# Create a directory and copy in all files
RUN mkdir -p /tmp/tippecanoe-src
RUN git clone https://github.com/mapbox/tippecanoe.git /tmp/tippecanoe-src
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