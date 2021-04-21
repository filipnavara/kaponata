$env:PATH="$PSScriptRoot\tools;${env:PATH}"
$env:KUBECONFIG="$PSScriptRoot\ci\kaponata\k3s.yaml"
$env:DOCKER_HOST="tcp://127.0.0.1:2375"
