resource "aws_lambda_function" "lambda_invoice_webhook_receiver" {
  function_name = "lambda-invoice-webhook-receiver"
  role          = aws_iam_role.lambda_role_invoice_webhook_receiver.arn
  handler       = "StarkBank.InvoiceWebhookReceiver"
  runtime       = "dotnet8"

  filename         = "lambda_function_payload.zip"
  source_code_hash = filebase64sha256("lambda_function_payload.zip")
}

resource "aws_iam_role" "lambda_role_invoice_webhook_receiver" {
  name = "lambda-role-invoice-webhook-receiver"

  assume_role_policy = jsonencode({
    Version = "2012-10-17",
    Statement = [{
      Action    = "sts:AssumeRole",
      Effect    = "Allow",
      Principal = {
        Service = "lambda.amazonaws.com"
      }
    }]
  })
}

resource "aws_iam_role_policy_attachment" "lambda_basic_execution_invoice_webhook_receiver" {
  role       = aws_iam_role.lambda_role_invoice_webhook_receiver.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"
}

resource "aws_api_gateway_rest_api" "api_gateway" {
  name = var.api_gateway_name

  policy = jsonencode({
    Version : "2012-10-17",
    Statement : [
      {
        Effect : "Deny",
        Principal : "*",
        Action : "execute-api:Invoke",
        Resource : "arn:aws:execute-api:${var.region}:${data.aws_caller_identity.current.account_id}:${aws_api_gateway_rest_api.api_gateway.id}/*/*/*",
        Condition : {
          NotIpAddress : {
            "aws:SourceIp" : var.allowed_ips
          }
        }
      }
    ]
  })
}

resource "aws_api_gateway_resource" "root_resource" {
  rest_api_id = aws_api_gateway_rest_api.api_gateway.id
  parent_id   = aws_api_gateway_rest_api.api_gateway.root_resource_id
  path_part   = "{proxy+}"
}

resource "aws_api_gateway_method" "any_method" {
  rest_api_id   = aws_api_gateway_rest_api.api_gateway.id
  resource_id   = aws_api_gateway_resource.root_resource.id
  http_method   = "ANY"
  authorization = "NONE"
}

resource "aws_api_gateway_integration" "lambda_integration" {
  rest_api_id = aws_api_gateway_rest_api.api_gateway.id
  resource_id = aws_api_gateway_resource.root_resource.id
  http_method = aws_api_gateway_method.any_method.http_method
  type        = "AWS_PROXY"
  integration_http_method = "POST"
  uri         = aws_lambda_function.lambda_invoice_webhook_receiver.invoke_arn
}

resource "aws_lambda_permission" "api_gateway_invoke" {
  statement_id  = "AllowAPIGatewayInvoke"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.lambda_invoice_webhook_receiver.function_name
  principal     = "apigateway.amazonaws.com"
  source_arn    = "arn:aws:execute-api:${var.region}:${data.aws_caller_identity.current.account_id}:${aws_api_gateway_rest_api.api_gateway.id}/*/*/*"
}

resource "aws_api_gateway_deployment" "api_deployment" {
  depends_on = [aws_api_gateway_integration.lambda_integration]
  rest_api_id = aws_api_gateway_rest_api.api_gateway.id
  stage_name  = "prod"
}

data "aws_caller_identity" "current" {}
