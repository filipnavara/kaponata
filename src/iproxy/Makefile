registry=quay.io/kaponata
image=iproxy

all: .arm64.docker-id .amd64.docker-id .version

push: all
	docker push $(registry)/$(image):latest-amd64
	docker push $(registry)/$(image):latest-arm64
	docker manifest create $(registry)/$(image):$(shell cat .version) $(registry)/$(image):latest-amd64 $(registry)/$(image):latest-arm64
	docker manifest push $(registry)/$(image):$(shell cat .version)

.version: .amd64.docker-id
	docker run --rm -it $(shell cat .amd64.docker-id) /usr/bin/iproxy --version | awk '{ print $$2 }' | tee .version

.amd64.docker-id: Dockerfile
	docker buildx build --platform linux/amd64 --build-arg BUILDCONTAINER=dockcross/linux-x64 . -t $(registry)/$(image):latest-amd64
	docker images --no-trunc --quiet $(registry)/$(image):amd64-latest > .amd64.docker-id

.arm64.docker-id: Dockerfile
	docker buildx build --platform linux/arm64 --build-arg BUILDCONTAINER=dockcross/linux-arm64 . -t $(registry)/$(image):latest-arm64
	docker images --no-trunc --quiet $(registry)/$(image):arm64-latest > .arm64.docker-id
