FROM --platform=$BUILDPLATFORM node:14 AS build

ENV DEBIAN_FRONTEND=noninteractive
ENV APPIUM_SKIP_CHROMEDRIVER_INSTALL=1

ARG arch=arm64
ARG version=1.20.2
ARG tini_version=0.19.0

# Download and compile
WORKDIR /src

RUN npm install --target-arch=$arch --only=production --global-style appium@$version

# https://github.com/nodejs/docker-node/blob/master/docs/BestPractices.md#handling-kernel-signals
RUN wget https://github.com/krallin/tini/releases/download/v${tini_version}/tini-${arch} -O /bin/tini \
&& chmod +x /bin/tini

FROM gcr.io/distroless/nodejs:14

ARG host=aarch64-linux-gnu
ARG arch=arm64

ARG GIT_COMMIT_DATE
ARG GIT_COMMIT_ID
ARG GIT_REF
ARG GIT_REMOTE
ARG VERSION

LABEL org.opencontainers.image.title="appium"
LABEL org.opencontainers.image.description="Appium is an open source test automation framework for use with native, hybrid and mobile web apps. It drives iOS, Android, and Windows apps using the WebDriver protocol."
LABEL org.opencontainers.image.licenses="Apache-2.0"

LABEL org.opencontainers.image.created=$GIT_COMMIT_DATE
LABEL org.opencontainers.image.authors="Frederik Carlier <frederik.carlier@quamotion.mobi>"
LABEL org.opencontainers.image.vendor="Quamotion bv <info@quamotion.mobi>"
LABEL org.opencontainers.image.url="https://www.kaponata.io/"
LABEL org.opencontainers.image.source=$GIT_REMOTE
LABEL org.opencontainers.image.version=$VERSION
LABEL org.opencontainers.image.revision=$GIT_COMMIT_ID
LABEL org.opencontainers.image.ref.name=$GIT_REF

COPY --from=build /src/node_modules/ /app/
COPY --from=build /bin/tini /bin/

ENTRYPOINT [ "/bin/tini", "--", "/nodejs/bin/node" ]
CMD [ "/app/appium/build/lib/main.js", "--host", "0.0.0.0" ]
