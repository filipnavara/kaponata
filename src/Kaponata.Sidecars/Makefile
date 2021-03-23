registry=quay.io/kaponata
image=sidecar
version=$(shell nbgv get-version -v SemVer2)

# Set tags to expire in 2 weeks (duration of a sprint) in quay.io;
# exception for tags built from release branches, which never expire.
branch=$(shell git rev-parse --abbrev-ref HEAD)
expires=$(if $(patsubst releases/,,$(branch)),2w,)

all: .arm64.docker-id .amd64.docker-id

clean:
	rm -f .arm64.docker-id .amd64.docker-id

deploy: all
	docker tag $(registry)/$(image):$(version)-amd64 $(registry)/$(image):latest-amd64
# As an exception to the general rule, delete all usbmuxd pods since they rely on the sidecar image.
	kubectl delete pod -l app.kubernetes.io/component=usbmuxd,app.kubernetes.io/name=usbmuxd
  
push: all
	docker push $(registry)/$(image):$(version)-amd64
	docker push $(registry)/$(image):$(version)-arm64
	docker manifest create --amend $(registry)/$(image):$(version) $(registry)/$(image):$(version)-amd64 $(registry)/$(image):$(version)-arm64
	docker manifest push $(registry)/$(image):$(version)

bin/Release/net5.0/publish/Kaponata.Sidecars.dll: $(shell find . -path ./bin -prune -false -o -path ./obj -prune -false -o -regex '.*\.cs' -type f)
	dotnet publish -c Release

.amd64.docker-id: Dockerfile bin/Release/net5.0/publish/Kaponata.Sidecars.dll
	docker buildx build \
		--platform linux/amd64 \
		--build-arg host= \
		--build-arg arch= \
		--build-arg GIT_COMMIT_DATE="$(shell nbgv get-version -v GitCommitDate)" \
		--build-arg GIT_COMMIT_ID="$(shell nbgv get-version -v GitCommitId)" \
		--build-arg GIT_REF="$(shell nbgv get-version -v BuildingRef)" \
		--build-arg GIT_REMOTE="$(shell git remote get-url $(shell git remote))" \
		--build-arg VERSION="$(version)" \
		--build-arg EXPIRES=$(expires) \
		. \
		-t $(registry)/$(image):$(version)-amd64
	docker images --no-trunc --quiet $(registry)/$(image):$(version)-amd64 > .amd64.docker-id

.arm64.docker-id: Dockerfile bin/Release/net5.0/publish/Kaponata.Sidecars.dll
	docker buildx build --platform linux/arm64 \
		--build-arg host=aarch64-linux-gnu \
		--build-arg arch=arm64 \
		--build-arg GIT_COMMIT_DATE="$(shell nbgv get-version -v GitCommitDate)" \
		--build-arg GIT_COMMIT_ID="$(shell nbgv get-version -v GitCommitId)" \
		--build-arg GIT_REF="$(shell nbgv get-version -v BuildingRef)" \
		--build-arg GIT_REMOTE="$(shell git remote get-url $(shell git remote))" \
		--build-arg VERSION="$(version)" \
		--build-arg EXPIRES=$(expires) \
		. \
		-t $(registry)/$(image):$(version)-arm64
	docker images --no-trunc --quiet $(registry)/$(image):$(version)-arm64 > .arm64.docker-id
