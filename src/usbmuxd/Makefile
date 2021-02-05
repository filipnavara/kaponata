registry=quay.io/kaponata
image=usbmuxd

all: .arm64.docker-id .amd64.docker-id .amd64.udev.docker-id .version

push: all
	docker push $(registry)/$(image):latest-amd64
	docker push $(registry)/$(image):latest-arm64
	docker manifest rm $(registry)/$(image):$(shell cat .version) || echo "Manifest does not exist"
	docker manifest create --amend $(registry)/$(image):$(shell cat .version) $(registry)/$(image):latest-amd64 $(registry)/$(image):latest-arm64
	docker manifest push $(registry)/$(image):$(shell cat .version)
	
	docker push $(registry)/$(image):latest-udev-amd64

.version: .amd64.docker-id
	docker run --rm -it $(shell cat .amd64.docker-id) /usr/bin/usbmuxd --version | awk '{ print $$2 }' | tee .version

Dockerfile.default: Dockerfile.template default.yaml
	gomplate -d config=./default.yaml -f Dockerfile.template -o Dockerfile.default

Dockerfile.udev: Dockerfile.template udev.yaml
	gomplate -d config=./udev.yaml -f Dockerfile.template -o Dockerfile.udev

.amd64.docker-id: Dockerfile.default
	docker buildx build \
		--platform linux/amd64 \
		--build-arg host= \
		--build-arg arch= \
		--build-arg GIT_COMMIT_DATE="$(shell nbgv get-version -v GitCommitDate)" \
		--build-arg GIT_COMMIT_ID="$(shell nbgv get-version -v GitCommitId)" \
		--build-arg GIT_REF="$(shell nbgv get-version -v BuildingRef)" \
		--build-arg GIT_REMOTE="$(shell git remote get-url $(shell git remote))" \
		--build-arg VERSION="$(shell nbgv get-version -v SemVer2)" \
		. \
		-f Dockerfile.default \
		-t $(registry)/$(image):latest-amd64
	docker images --no-trunc --quiet $(registry)/$(image):latest-amd64 > .amd64.docker-id

.arm64.docker-id: Dockerfile.default
	docker buildx build --platform linux/arm64 \
		--build-arg host=aarch64-linux-gnu \
		--build-arg arch=arm64 \
		--build-arg GIT_COMMIT_DATE="$(shell nbgv get-version -v GitCommitDate)" \
		--build-arg GIT_COMMIT_ID="$(shell nbgv get-version -v GitCommitId)" \
		--build-arg GIT_REF="$(shell nbgv get-version -v BuildingRef)" \
		--build-arg GIT_REMOTE="$(shell git remote get-url $(shell git remote))" \
		--build-arg VERSION="$(shell nbgv get-version -v SemVer2)" \
		. \
		-f Dockerfile.default \
		-t $(registry)/$(image):latest-arm64
	docker images --no-trunc --quiet $(registry)/$(image):latest-arm64 > .arm64.docker-id

.amd64.udev.docker-id: Dockerfile.udev
	docker buildx build \
		--platform linux/amd64 \
		--build-arg host= \
		--build-arg arch= \
		--build-arg with_udev=yes \
		--build-arg GIT_COMMIT_DATE="$(shell nbgv get-version -v GitCommitDate)" \
		--build-arg GIT_COMMIT_ID="$(shell nbgv get-version -v GitCommitId)" \
		--build-arg GIT_REF="$(shell nbgv get-version -v BuildingRef)" \
		--build-arg GIT_REMOTE="$(shell git remote get-url $(shell git remote))" \
		--build-arg VERSION="$(shell nbgv get-version -v SemVer2)" \
		. \
		-f Dockerfile.udev \
		-t $(registry)/$(image):latest-udev-amd64
	docker images --no-trunc --quiet $(registry)/$(image):latest-udev-amd64 > .amd64.udev.docker-id
