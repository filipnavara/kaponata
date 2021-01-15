FROM --platform=$BUILDPLATFORM debian:10 AS build

ARG libusb_version=1.0.23
ARG libplist_version=2.2.0
ARG libusbmuxd_version=2.0.2
ARG libimobiledevice_version=1.3.0
ARG usbmuxd_version=1248a070f872e15a15b348150ff8bfb96491ddc3

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

# Install libssl for the host architecture
RUN apt-get install -y libssl-dev:$arch

WORKDIR /src
RUN curl -L https://github.com/libusb/libusb/archive/v${libusb_version}.tar.gz --output libusb-${libusb_version}.tar.gz
RUN curl -L https://github.com/libimobiledevice/libplist/archive/${libplist_version}.tar.gz --output libplist-${libplist_version}.tar.gz
RUN curl -L https://github.com/libimobiledevice/libusbmuxd/archive/${libusbmuxd_version}.tar.gz --output libusbmuxd-${libusbmuxd_version}.tar.gz
RUN curl -L https://github.com/libimobiledevice/libimobiledevice/archive/${libimobiledevice_version}.tar.gz --output libimobiledevice-${libimobiledevice_version}.tar.gz
RUN curl -L https://github.com/libimobiledevice/usbmuxd/archive/${usbmuxd_version}.tar.gz --output usbmuxd-${usbmuxd_version}.tar.gz

RUN tar -xvzf libusb-${libusb_version}.tar.gz
RUN tar -xvzf libplist-${libplist_version}.tar.gz
RUN tar -xvzf libusbmuxd-${libusbmuxd_version}.tar.gz
RUN tar -xvzf libimobiledevice-${libimobiledevice_version}.tar.gz
RUN tar -xvzf usbmuxd-${usbmuxd_version}.tar.gz

WORKDIR /src/libusb-${libusb_version}

RUN ./autogen.sh enable_udev=no --build=$build --host=$host --prefix=/usr --libdir=/usr/lib/$host \
&& make \
&& make install

WORKDIR /src/libplist-${libplist_version}

RUN ./autogen.sh --without-cython --enable-static=no --build=$build --host=$host --prefix=/usr --libdir=/usr/lib/$host \
&& make \
&& make install

WORKDIR /src/libusbmuxd-${libusbmuxd_version}

RUN ./autogen.sh --enable-static=no --build=$build --host=$host --prefix=/usr --libdir=/usr/lib/$host \
&& make \
&& make install

WORKDIR /src/libimobiledevice-${libimobiledevice_version}

RUN ./autogen.sh --without-cython --enable-static=no --build=$build --host=$host --prefix=/usr --libdir=/usr/lib/$host \
&& make \
&& make install

WORKDIR /src/usbmuxd-${usbmuxd_version}

RUN ./autogen.sh --build=$build --host=$host --prefix=/usr --libdir=/usr/lib/$host \
&& make \
&& make install

FROM gcr.io/distroless/base-debian10

ARG host=aarch64-linux-gnu
ARG arch=arm64

COPY --from=build /usr/lib/$host/libusb-1.0.so.0 /usr/lib/$host/
COPY --from=build /usr/lib/$host/libplist-2.0.so.3 /usr/lib/$host/
COPY --from=build /usr/lib/$host/libusbmuxd-2.0.so.6 /usr/lib/$host/
COPY --from=build /usr/lib/$host/libimobiledevice-1.0.so.6 /usr/lib/$host/
COPY --from=build /usr/bin/idevice_id /usr/bin/
COPY --from=build /usr/bin/idevicepair /usr/bin/
COPY --from=build /usr/bin/ideviceinfo /usr/bin/

COPY --from=build /usr/sbin/usbmuxd /usr/sbin/usbmuxd

ENV USBMUXD_SOCKET_ADDRESS=127.0.0.1:27015

ENTRYPOINT [ "/usr/sbin/usbmuxd", "-v", "-f", "-S", "0.0.0.0:27015" ]
