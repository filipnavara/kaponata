deploy:
	helm uninstall kaponata || echo "Not installed, skipping uninstall"
	helm install kaponata .

lint:
	helm lint .
