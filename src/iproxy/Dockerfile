ARG BUILDCONTAINER

FROM --platform=$BUILDPLATFORM $BUILDCONTAINER AS build

ARG libplist_version=2.2.0
ARG libusbmuxd_version=2.0.2
ARG build=x86_64-linux-gnu

WORKDIR /src

RUN curl -L https://github.com/libimobiledevice/libplist/archive/${libplist_version}.tar.gz --output libplist-${libplist_version}.tar.gz
RUN curl -L https://github.com/libimobiledevice/libusbmuxd/archive/${libusbmuxd_version}.tar.gz --output libusbmuxd-${libusbmuxd_version}.tar.gz

RUN tar -xvzf libplist-${libplist_version}.tar.gz
RUN tar -xvzf libusbmuxd-${libusbmuxd_version}.tar.gz

WORKDIR /src/libplist-${libplist_version}

RUN ./autogen.sh --without-cython --enable-static=no --build=$build --host=$CROSS_TRIPLE --prefix=/usr \
&& make \
&& DESTDIR=$CROSS_ROOT/$CROSS_TRIPLE/sysroot/ make install \
&& make install

WORKDIR /src/libusbmuxd-${libusbmuxd_version}

RUN ./autogen.sh --without-cython --enable-static=no --build=$build --host=$CROSS_TRIPLE --prefix=/usr \
&& make \
&& DESTDIR=$CROSS_ROOT/$CROSS_TRIPLE/sysroot/ make install \
&& make install

ARG TARGETPLATFORM
ARG BUILDPLATFORM

FROM gcr.io/distroless/base-debian10

COPY --from=build /usr/lib/libplist-2.0.so.3 /usr/lib/
COPY --from=build /usr/lib/libusbmuxd-2.0.so.6 /usr/lib/
COPY --from=build /usr/bin/iproxy /usr/bin/

ENTRYPOINT [ "/usr/bin/iproxy" ]