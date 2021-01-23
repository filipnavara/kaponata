deploy:
	helm uninstall kaponata || echo "Not installed, skipping uninstall"
	helm install --set operator.image.tag="latest-amd64" --set operator.imagePullPolicy=Never kaponata .

lint:
	helm lint .
