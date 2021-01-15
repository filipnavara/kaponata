registry=quay.io/kaponata
image=operator
version=$(shell nbgv get-version -v SemVer2)

all: .arm64.docker-id .amd64.docker-id

clean:
	rm -f .arm64.docker-id .amd64.docker-id

push: all
	docker push $(registry)/$(image):$(version)-amd64
	docker push $(registry)/$(image):$(version)-arm64
	docker manifest create --amend $(registry)/$(image):$(version) $(registry)/$(image):$(version)-amd64 $(registry)/$(image):$(version)-arm64
	docker manifest push $(registry)/$(image):$(version)

.amd64.docker-id: Dockerfile
	docker buildx build \
		--platform linux/amd64 \
		--build-arg host= \
		--build-arg arch= \
		--build-arg GIT_COMMIT_DATE="$(shell nbgv get-version -v GitCommitDate)" \
		--build-arg GIT_COMMIT_ID="$(shell nbgv get-version -v GitCommitId)" \
		--build-arg GIT_REF="$(shell nbgv get-version -v BuildingRef)" \
		--build-arg GIT_REMOTE="$(shell git remote get-url $(shell git remote))" \
		--build-arg VERSION="$(version)" \
		. \
		-t $(registry)/$(image):$(version)-amd64
	docker images --no-trunc --quiet $(registry)/$(image):$(version)-amd64 > .amd64.docker-id

.arm64.docker-id: Dockerfile
	docker buildx build --platform linux/arm64 \
		--build-arg host=aarch64-linux-gnu \
		--build-arg arch=arm64 \
		--build-arg GIT_COMMIT_DATE="$(shell nbgv get-version -v GitCommitDate)" \
		--build-arg GIT_COMMIT_ID="$(shell nbgv get-version -v GitCommitId)" \
		--build-arg GIT_REF="$(shell nbgv get-version -v BuildingRef)" \
		--build-arg GIT_REMOTE="$(shell git remote get-url $(shell git remote))" \
		--build-arg VERSION="$(version)" \
		. \
		-t $(registry)/$(image):$(version)-arm64
	docker images --no-trunc --quiet $(registry)/$(image):$(version)-arm64 > .arm64.docker-id
