{{/*
Expand the name of the chart.
*/}}
{{- define "usbmuxd.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Create a default fully qualified app name.
We truncate at 63 chars because some Kubernetes name fields are limited to this (by the DNS naming spec).
If release name contains chart name it will be used as a full name.
*/}}
{{- define "usbmuxd.fullname" -}}
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
{{- define "usbmuxd.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Common labels
*/}}
{{- define "usbmuxd.labels" -}}
helm.sh/chart: {{ include "usbmuxd.chart" . }}
{{ include "usbmuxd.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{/*
Selector labels
*/}}
{{- define "usbmuxd.selectorLabels" -}}
app.kubernetes.io/name: {{ include "usbmuxd.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
app.kubernetes.io/component: "usbmuxd"
{{- end }}

{{/*
usbmuxd: Service Account Full Name
*/}}
{{- define "usbmuxd.serviceAccount.fullname" -}}
{{- printf "%s-usbmuxd-account" (include "usbmuxd.fullname" .) | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
usbmuxd: Role Full Name
*/}}
{{- define "usbmuxd.role.fullname" -}}
{{- printf "%s-usbmuxd-role" (include "usbmuxd.fullname" .) | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
usbmuxd: Role Binding Full Name
*/}}
{{- define "usbmuxd.roleBinding.fullname" -}}
{{- printf "%s-usbmuxd-rolebinding" (include "usbmuxd.fullname" .) | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
usbmuxd: ConfigMap Full Name
*/}}
{{- define "usbmuxd.configMap.fullname" -}}
{{- printf "%s-usbmuxd-configmap" (include "usbmuxd.fullname" .) | trunc 63 | trimSuffix "-" }}
{{- end }}