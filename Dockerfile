FROM alpine AS tippecanoe-builder

WORKDIR /tmp

RUN apk add --no-cache build-base git zlib-dev sqlite-dev bash

RUN git clone --depth 1 --branch 2.79.0 https://github.com/felt/tippecanoe.git tippecanoe-src

RUN make -C tippecanoe-src -j"$(nproc)"

RUN make -C tippecanoe-src install

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build-env
WORKDIR /app

COPY ./*sln ./

COPY ./src/TileWatcher/*.csproj ./src/TileWatcher/

RUN dotnet restore --packages ./packages

COPY . ./
WORKDIR /app/src/TileWatcher
RUN dotnet publish -c Release -o out --packages ./packages

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:10.0-alpine

COPY --from=tippecanoe-builder /usr/local/bin/tippecanoe /usr/local/bin/

WORKDIR /app

COPY --from=build-env /app/src/TileWatcher/out .
ENTRYPOINT ["dotnet", "TileWatcher.dll"]
