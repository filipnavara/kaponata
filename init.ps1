choco install -y gawk
choco install -y docker-cli
choco install -y gnuwin32-coreutils.portable
choco install -y gomplate
choco install -y kubernetes-helm
choco install -y k3d
choco install -y yq
choco install -y make
choco install -y findutils

$env:PATH="$PSScriptRoot\tools;${env:PATH}"
$env:KUBECONFIG="$PSScriptRoot\ci\kaponata\k3s.yaml"
$env:DOCKER_HOST="tcp://127.0.0.1:2375"
