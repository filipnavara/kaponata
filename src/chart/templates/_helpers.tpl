{{/*
Expand the name of the chart.
*/}}
{{- define "kaponata.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Create a default fully qualified app name.
We truncate at 63 chars because some Kubernetes name fields are limited to this (by the DNS naming spec).
If release name contains chart name it will be used as a full name.
*/}}
{{- define "kaponata.fullname" -}}
{{- if .Values.fullnameOverride }}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- $name := default .Chart.Name .Values.nameOverride }}
{{- if contains $name .Release.Name }}
{{- .Release.Name | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- printf "%s-%s" .Release.Name $name | trunc 63 | trimSuffix "-" }}
{{- end }}
{{- end }}
{{- end }}

{{/*
Create chart name and version as used by the chart label.
*/}}
{{- define "kaponata.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Operator: Full Name
*/}}
{{- define "operator.fullname" -}}
{{- printf "%s-operator" (include "kaponata.fullname" .) | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Operator: Cluster Role Full Name
*/}}
{{- define "operator.clusterRole.fullname" -}}
{{- printf "%s-operator-clusterrole" (include "kaponata.fullname" .) | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Operator: Cluster Role Binding Full Name
*/}}
{{- define "operator.clusterRoleBinding.fullname" -}}
{{- printf "%s-operator-clusterrolebinding" (include "kaponata.fullname" .) | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Operator: Role Full Name
*/}}
{{- define "operator.role.fullname" -}}
{{- printf "%s-operator-role" (include "kaponata.fullname" .) | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Operator: Role Binding Full Name
*/}}
{{- define "operator.roleBinding.fullname" -}}
{{- printf "%s-operator-rolebinding" (include "kaponata.fullname" .) | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Operator: Service Account Full Name
*/}}
{{- define "operator.serviceAccount.fullname" -}}
{{- printf "%s-operator-account" (include "kaponata.fullname" .) | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Operator: Common labels
*/}}
{{- define "operator.labels" -}}
helm.sh/chart: {{ include "kaponata.chart" . }}
{{ include "operator.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{/*
Operator: Selector labels
*/}}
{{- define "operator.selectorLabels" -}}
app.kubernetes.io/name: {{ include "kaponata.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
app.kubernetes.io/component: "operator"
{{- end }}
