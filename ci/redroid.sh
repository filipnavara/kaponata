#!/bin/sh

# Start the Redroid and Appium containers
kubectl create -f redroid.yaml
kubectl create -f appium.yaml

# Wait for the pods to be ready
kubectl wait --for=condition=ready pod redroid
kubectl wait --for=condition=ready pod redroid-appium

# Run 'adb connect' in the Appium pod
kubectl exec -it redroid-appium -c appium -- /android/platform-tools/adb connect $(kubectl get pods redroid -o=jsonpath='{.status.podIP}')
kubectl exec -it redroid-appium -c appium -- /android/platform-tools/adb devices

# Start port forwarding
kubectl port-forward redroid-appium 4723

# Start the session (port-forward will block, so you'll need a separate terminal for this)
curl -X POST -H "Content-Type: application/json" http://localhost:4723/wd/hub/session --data "@capabilities.json"

# Troubleshooting tips
kubectl exec -it redroid-appium /android/platform-tools/adb shell
