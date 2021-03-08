{{/*
Expand the name of the chart.
*/}}
{{- define "guacamole.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Create a default fully qualified app name.
We truncate at 63 chars because some Kubernetes name fields are limited to this (by the DNS naming spec).
If release name contains chart name it will be used as a full name.
*/}}
{{- define "guacamole.fullname" -}}
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
{{- define "guacamole.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Guacamole: Full Name
*/}}
{{- define "guacamole.guacamole.fullname" -}}
{{- printf "%s-guacamole" (include "guacamole.fullname" .) | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Guacamole: Selector labels
*/}}
{{- define "guacamole.guacamole.selectorLabels" -}}
app.kubernetes.io/name: {{ include "guacamole.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
app.kubernetes.io/component: "guacamole"
{{- end }}

{{/*
Guacamole: Common labels
*/}}
{{- define "guacamole.guacamole.labels" -}}
helm.sh/chart: {{ include "guacamole.chart" . }}
{{ include "guacamole.guacamole.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{/*
Guacamole: Service Name
*/}}
{{- define "guacamole.guacamole.service.fullname" -}}
{{- printf "%s-guacamole-service" (include "guacamole.fullname" .) | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Guacamole: Service Name
*/}}
{{- define "guacamole.guacamole.ingress.fullname" -}}
{{- printf "%s-guacamole-ingress" (include "guacamole.fullname" .) | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
guacd: Full Name
*/}}
{{- define "guacamole.guacd.fullname" -}}
{{- printf "%s-guacd" (include "guacamole.fullname" .) | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
guacd: Selector labels
*/}}
{{- define "guacamole.guacd.selectorLabels" -}}
app.kubernetes.io/name: {{ include "guacamole.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
app.kubernetes.io/component: "guacd"
{{- end }}

{{/*
guacd: Common labels
*/}}
{{- define "guacamole.guacd.labels" -}}
helm.sh/chart: {{ include "guacamole.chart" . }}
{{ include "guacamole.guacd.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{/*
guacd: Service Name
*/}}
{{- define "guacamole.guacd.service.fullname" -}}
{{- printf "%s-guacd-service" (include "guacamole.fullname" .) | trunc 63 | trimSuffix "-" }}
{{- end }}
