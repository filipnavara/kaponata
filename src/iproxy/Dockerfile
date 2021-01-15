FROM --platform=$BUILDPLATFORM debian:10 AS build

ARG libplist_version=2.2.0
ARG libusbmuxd_version=2.0.2
ARG build=x86_64-linux-gnu

ENV DEBIAN_FRONTEND=noninteractive

ENV build=x86_64-linux-gnu
ARG host=aarch64-linux-gnu
ARG arch=arm64

# Add the host architecture if needed
RUN if [ -n "$arch" ]; then dpkg --add-architecture $arch; fi

# Core build tools
RUN apt-get update \
&& apt-get install -y make libtool pkg-config curl dpkg-dev

# Install the cross-compiler toolchain if cross-compiling
RUN if [ -n "$host" ]; then apt-get install -y g++-$host; fi

# Download and compile
WORKDIR /src

RUN curl -L https://github.com/libimobiledevice/libplist/archive/${libplist_version}.tar.gz --output libplist-${libplist_version}.tar.gz
RUN curl -L https://github.com/libimobiledevice/libusbmuxd/archive/${libusbmuxd_version}.tar.gz --output libusbmuxd-${libusbmuxd_version}.tar.gz

RUN tar -xvzf libplist-${libplist_version}.tar.gz
RUN tar -xvzf libusbmuxd-${libusbmuxd_version}.tar.gz

WORKDIR /src/libplist-${libplist_version}

RUN ./autogen.sh --without-cython --enable-static=no --build=$build --host=$host --prefix=/usr --libdir=/usr/lib/$host \
&& make \
&& make install

WORKDIR /src/libusbmuxd-${libusbmuxd_version}

RUN ./autogen.sh --enable-static=no --build=$build --host=$host --prefix=/usr --libdir=/usr/lib/$host \
&& make \
&& make install

FROM gcr.io/distroless/base-debian10

ARG host=aarch64-linux-gnu
ARG arch=arm64

ARG GIT_COMMIT_DATE
ARG GIT_COMMIT_ID
ARG GIT_REF
ARG GIT_REMOTE
ARG VERSION

LABEL org.opencontainers.image.title="iproxy"
LABEL org.opencontainers.image.description="Proxy that enables tcp service access to iOS devices"
LABEL org.opencontainers.image.licenses="GPL-2.0"

LABEL org.opencontainers.image.created=$GIT_COMMIT_DATE
LABEL org.opencontainers.image.authors="Frederik Carlier <frederik.carlier@quamotion.mobi>"
LABEL org.opencontainers.image.vendor="Quamotion bv <info@quamotion.mobi>"
LABEL org.opencontainers.image.url="https://www.kaponata.io/"
LABEL org.opencontainers.image.source=$GIT_REMOTE
LABEL org.opencontainers.image.version=$VERSION
LABEL org.opencontainers.image.revision=$GIT_COMMIT_ID
LABEL org.opencontainers.image.ref.name=$GIT_REF

COPY --from=build /usr/lib/$host/libplist-2.0.so.3 /usr/lib/$host/
COPY --from=build /usr/lib/$host/libusbmuxd-2.0.so.6 /usr/lib/$host/
COPY --from=build /usr/bin/iproxy /usr/bin/

ENTRYPOINT [ "/usr/bin/iproxy" ]
