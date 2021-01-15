registry=quay.io/kaponata
image=usbmuxd

all: .arm64.docker-id .amd64.docker-id .version

push: all
	docker push $(registry)/$(image):latest-amd64
	docker push $(registry)/$(image):latest-arm64
	docker manifest create --amend $(registry)/$(image):$(shell cat .version) $(registry)/$(image):latest-amd64 $(registry)/$(image):latest-arm64
	docker manifest push $(registry)/$(image):$(shell cat .version)

.version: .amd64.docker-id
	docker run --rm -it $(shell cat .amd64.docker-id) /usr/bin/usbmuxd --version | awk '{ print $$2 }' | tee .version

.amd64.docker-id: Dockerfile
	docker buildx build --platform linux/amd64 --build-arg host= --build-arg arch= . -t $(registry)/$(image):latest-amd64
	docker images --no-trunc --quiet $(registry)/$(image):latest-amd64 > .amd64.docker-id

.arm64.docker-id: Dockerfile
	docker buildx build --platform linux/arm64 --build-arg host=aarch64-linux-gnu --build-arg arch=arm64 . -t $(registry)/$(image):latest-arm64
	docker images --no-trunc --quiet $(registry)/$(image):latest-arm64 > .arm64.docker-id
