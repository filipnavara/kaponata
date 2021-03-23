clean:
	$(MAKE) -C Kaponata.Api clean
	$(MAKE) -C Kaponata.Operator clean
	$(MAKE) -C Kaponata.Sidecars clean

deploy:
	$(MAKE) -C Kaponata.Api deploy
	$(MAKE) -C Kaponata.Operator deploy
	$(MAKE) -C Kaponata.Sidecars deploy
	$(MAKE) -C chart deploy