FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine

ARG GIT_COMMIT_DATE
ARG GIT_COMMIT_ID
ARG GIT_REF
ARG GIT_REMOTE
ARG VERSION
ARG EXPIRES

LABEL org.opencontainers.image.title="Kaponata Operator"
LABEL org.opencontainers.image.description=""
LABEL org.opencontainers.image.licenses="MIT"

LABEL org.opencontainers.image.created=$GIT_COMMIT_DATE
LABEL org.opencontainers.image.authors="Frederik Carlier <frederik.carlier@quamotion.mobi>"
LABEL org.opencontainers.image.vendor="Quamotion bv <info@quamotion.mobi>"
LABEL org.opencontainers.image.url="https://www.kaponata.io/"
LABEL org.opencontainers.image.source=$GIT_REMOTE
LABEL org.opencontainers.image.version=$VERSION
LABEL org.opencontainers.image.revision=$GIT_COMMIT_ID
LABEL org.opencontainers.image.ref.name=$GIT_REF

LABEL quay.expires-after=$EXPIRES

WORKDIR /app

COPY bin/Release/net5.0/publish /app/

ENTRYPOINT ["dotnet", "/app/Kaponata.Operator.dll"]
