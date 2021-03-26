deploy:
	helm uninstall kaponata || echo "Not installed, skipping uninstall"
	helm install \
		--set operator.image.tag="latest-amd64" \
		--set operator.imagePullPolicy=Never \
		--set api.image.tag="latest-amd64" \
		--set api.imagePullPolicy=Never \
		kaponata .

upgrade:
	helm upgrade kaponata .

lint:
	helm lint .
