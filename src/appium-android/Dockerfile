FROM ubuntu:focal AS build

RUN apt-get update \
&& apt-get install -y curl unzip zlib1g \
&& rm -rf /var/lib/apt/lists/*

ARG JDK_VERSION=15.0.2
ARG JDK_SLUG=0d1cfde4252546c6931946de8db48ee2
ENV JAVA_HOME=/java

WORKDIR $JAVA_HOME
ENV PATH=$PATH:$JAVA_HOME

RUN curl -o java.tar.gz https://download.java.net/java/GA/jdk${JDK_VERSION}/${JDK_SLUG}/7/GPL/openjdk-${JDK_VERSION}_linux-x64_bin.tar.gz \
&& tar xvzf java.tar.gz --strip-components=1 \
&& rm java.tar.gz

# The latest build tools and platform tools version number can
# be found in https://dl.google.com/android/repository/repository2-1.xml
ARG BUILD_TOOLS_VERSION="30.0.3"
ARG PLATFORM_TOOLS_VERSION="31.0.0"

ENV ANDROID_HOME=/android

WORKDIR $ANDROID_HOME/build-tools

RUN curl -o build-tools_r${BUILD_TOOLS_VERSION}-linux.zip https://dl.google.com/android/repository/build-tools_r${BUILD_TOOLS_VERSION}-linux.zip \
&& unzip build-tools_r${BUILD_TOOLS_VERSION}-linux.zip \
&& rm build-tools_r${BUILD_TOOLS_VERSION}-linux.zip

WORKDIR $ANDROID_HOME

RUN curl -o platform-tools_r${PLATFORM_TOOLS_VERSION}-linux.zip https://dl.google.com/android/repository/platform-tools_r${PLATFORM_TOOLS_VERSION}-linux.zip \
&& unzip platform-tools_r${PLATFORM_TOOLS_VERSION}-linux.zip \
&& rm platform-tools_r${PLATFORM_TOOLS_VERSION}-linux.zip

FROM quay.io/kaponata/appium:1.20.2

ARG GIT_COMMIT_DATE
ARG GIT_COMMIT_ID
ARG GIT_REF
ARG GIT_REMOTE
ARG VERSION

LABEL org.opencontainers.image.title="appium-android"
LABEL org.opencontainers.image.description="Appium is an open source test automation framework for use with native, hybrid and mobile web apps. It drives iOS, Android, and Windows apps using the WebDriver protocol. This image contains the tools required to run tests on Android devices."

LABEL org.opencontainers.image.created=$GIT_COMMIT_DATE
LABEL org.opencontainers.image.authors="Frederik Carlier <frederik.carlier@quamotion.mobi>"
LABEL org.opencontainers.image.vendor="Quamotion bv <info@quamotion.mobi>"
LABEL org.opencontainers.image.url="https://www.kaponata.io/"
LABEL org.opencontainers.image.source=$GIT_REMOTE
LABEL org.opencontainers.image.version=$VERSION
LABEL org.opencontainers.image.revision=$GIT_COMMIT_ID
LABEL org.opencontainers.image.ref.name=$GIT_REF

COPY --from=build /lib/x86_64-linux-gnu/libz.so* /lib/x86_64-linux-gnu/
COPY --from=build /java /java

# We can significantly reduce the size of the resulting container by only copying
# the Android tools which are actually used by Appium:
COPY --from=build --chmod=a+x /android/platform-tools/adb /android/platform-tools/
COPY --from=build --chmod=a+x /android/platform-tools/NOTICE.txt /android/platform-tools/
COPY --from=build /android/build-tools/android-11/lib/apksigner.jar /android/build-tools/android-11/lib/
COPY --from=build /android/build-tools/android-11/NOTICE.txt /android/build-tools/android-11/

# Alternatively, to just copy the entire Android SDK:
# COPY --from=build /android /android

ENV JAVA_HOME=/java
ENV ANDROID_HOME=/android
ENV PATH=$PATH:$JAVA_HOME:$ANDROID_HOME/platform-tools
