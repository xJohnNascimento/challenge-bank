resource "aws_lambda_function" "lambda_invoice_webhook_receiver" {
  function_name = "lambda-invoice-webhook-receiver"
  role          = aws_iam_role.lambda_role_invoice_webhook_receiver.arn
  handler       = "StarkBank.InvoiceWebhookReceiver"
  runtime       = "dotnet8"
  filename         = "../src/StarkBank/Application/StarkBank.InvoiceWebhookReceiver/bin/Release/release.zip"
  source_code_hash = filebase64sha256("../src/StarkBank/Application/StarkBank.InvoiceWebhookReceiver/bin/Release/release.zip")
  timeout = 10
  environment {
    variables = {
      TOPIC_ARN   = aws_sns_topic.sns_invoice_payment.arn
    }
  }
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

resource "aws_iam_policy" "lambda_sns_publish_policy" {
  name        = "lambda-sns-publish-policy"
  description = "Policy for Lambda to publish messages to SNS"

  policy = jsonencode({
    Version = "2012-10-17",
    Statement = [
      {
        Effect   = "Allow",
        Action   = "sns:Publish",
        Resource = aws_sns_topic.sns_invoice_payment.arn
      }
    ]
  })
}

resource "aws_iam_role_policy_attachment" "lambda_sns_publish_policy_attachment" {
  role       = aws_iam_role.lambda_role_invoice_webhook_receiver.name
  policy_arn = aws_iam_policy.lambda_sns_publish_policy.arn
}

resource "aws_api_gateway_rest_api" "api_gateway" {
  name = var.api_gateway_name
}

resource "aws_api_gateway_rest_api_policy" "api_gateway_policy_attachment" {
  rest_api_id = aws_api_gateway_rest_api.api_gateway.id

  policy = jsonencode({
    Version   = "2012-10-17",
    Statement = [
      {
        Effect    = "Allow",
        Principal = "*",
        Action    = "execute-api:Invoke",
        Resource  = "arn:aws:execute-api:${var.region}:${data.aws_caller_identity.current.account_id}:${aws_api_gateway_rest_api.api_gateway.id}/*/*/*",
        Condition = {
          IpAddress = {
            "aws:SourceIp": var.allowed_ips
          }
        }
      }
    ]
  })

  depends_on = [aws_api_gateway_rest_api.api_gateway]
}

resource "aws_api_gateway_resource" "root_resource" {
  rest_api_id = aws_api_gateway_rest_api.api_gateway.id
  parent_id   = aws_api_gateway_rest_api.api_gateway.root_resource_id
  path_part   = "webhook"
}

resource "aws_api_gateway_method" "post_webhook_method" {
  rest_api_id   = aws_api_gateway_rest_api.api_gateway.id
  resource_id   = aws_api_gateway_resource.root_resource.id
  http_method   = "POST"
  authorization = "NONE"
}

resource "aws_api_gateway_integration" "lambda_webhook_integration" {
  rest_api_id             = aws_api_gateway_rest_api.api_gateway.id
  resource_id             = aws_api_gateway_resource.root_resource.id
  http_method             = aws_api_gateway_method.post_webhook_method.http_method
  type                    = "AWS_PROXY"
  integration_http_method = "POST"
  uri                     = aws_lambda_function.lambda_invoice_webhook_receiver.invoke_arn
}

resource "aws_lambda_permission" "api_gateway_invoke" {
  statement_id  = "AllowAPIGatewayInvoke"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.lambda_invoice_webhook_receiver.function_name
  principal     = "apigateway.amazonaws.com"
  source_arn    = "arn:aws:execute-api:${var.region}:${data.aws_caller_identity.current.account_id}:${aws_api_gateway_rest_api.api_gateway.id}/*/*/*"
}

resource "aws_api_gateway_deployment" "api_deployment" {
  depends_on = [aws_api_gateway_integration.lambda_webhook_integration]
  rest_api_id = aws_api_gateway_rest_api.api_gateway.id
  stage_name  = "prod"
}

data "aws_caller_identity" "current" {}
