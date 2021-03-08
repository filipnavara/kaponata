registry=quay.io/kaponata
image=guacamole

all: .amd64.docker-id .version

push: all
	docker push $(registry)/$(image):latest-amd64
	docker manifest rm $(registry)/$(image):$(shell cat .version) || echo "Manifest does not exist"
	docker manifest create --amend $(registry)/$(image):$(shell cat .version) $(registry)/$(image):latest-amd64
	docker manifest push $(registry)/$(image):$(shell cat .version)

deploy: all
	k3d image import $(registry)/$(image):latest-amd64
	docker image ls $(registry)/$(image):latest-amd64
	docker exec -it k3d-k3s-default-server-0 crictl images | grep $(registry)/$(image)

.version: .amd64.docker-id
	docker run --rm -i $(shell cat .amd64.docker-id) unzip -c /opt/guacamole/guacamole.war META-INF/maven/org.apache.guacamole/guacamole/pom.properties | grep version | awk -F= '{ print $$2 }' | tee .version

.amd64.docker-id: Dockerfile
	docker buildx build \
		--progress plain \
		--platform linux/amd64 \
		--build-arg host= \
		--build-arg arch= \
		--build-arg GIT_COMMIT_DATE="$(shell nbgv get-version -v GitCommitDate)" \
		--build-arg GIT_COMMIT_ID="$(shell nbgv get-version -v GitCommitId)" \
		--build-arg GIT_REF="$(shell nbgv get-version -v BuildingRef)" \
		--build-arg GIT_REMOTE="$(shell git remote get-url $(shell git remote))" \
		--build-arg VERSION="$(shell nbgv get-version -v SemVer2)" \
		. \
		-t $(registry)/$(image):latest-amd64
	docker images --no-trunc --quiet $(registry)/$(image):latest-amd64 > .amd64.docker-id
