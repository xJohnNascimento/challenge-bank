variable "s3_bucket_name" {
  description = "Nome do bucket S3 que a Lambda irá acessar"
  type        = string
  default     = "techchallenge-starkbank"
}

variable "sns_topic_name" {
  default     = "sns-invoice-payment"
  description = "Nome do tópico SNS"
}

variable "sqs_queue_name" {
  default     = "sqs-invoice-payment"
  description = "Nome da fila SQS"
}

variable "api_gateway_name" {
  default     = "api-gateway-invoice"
  description = "Nome do API Gateway"
}

variable "allowed_ips" {
  type        = list(string)
  default     = ["35.247.226.240/32", "35.245.182.229/32"]
  description = "Lista de IPs permitidos"
}

variable "region" {
  default = "sa-east-1"
}