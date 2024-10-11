resource "aws_iam_role" "lambda_role_create_invoice" {
  name = "lambda-role-create-invoice"

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

resource "aws_iam_role_policy_attachment" "lambda_basic_execution_create_invoice" {
  role       = aws_iam_role.lambda_role_create_invoice.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"
}

resource "aws_iam_policy" "lambda_s3_policy" {
  name        = "lambda_s3_policy"
  description = "Política IAM para a Lambda acessar o bucket S3 específico"

  policy = jsonencode({
    Version = "2012-10-17",
    Statement = [{
      Action   = [
        "s3:GetObject",
        "s3:PutObject",
        "s3:DeleteObject"
      ],
      Effect   = "Allow",
      Resource = "arn:aws:s3:::${var.s3_bucket_name}/*"
    }]
  })
}

resource "aws_iam_policy" "lambda_disable_event_rule_policy" {
  name        = "lambda_disable_event_rule_policy"
  description = "Policy for the Lambda function to disable CloudWatch event rule"

  policy = jsonencode({
    Version = "2012-10-17",
    Statement = [{
      Action   = [
        "events:DisableRule",
        "events:DeleteRule"
      ],
      Effect   = "Allow",
      Resource = aws_cloudwatch_event_rule.every_3_hours.arn
    }]
  })
}

resource "aws_iam_role_policy_attachment" "lambda_disable_event_rule_policy_attachment" {
  role       = aws_iam_role.lambda_role_create_invoice.name
  policy_arn = aws_iam_policy.lambda_disable_event_rule_policy.arn
}


resource "aws_iam_role_policy_attachment" "lambda_s3_policy_attachment" {
  role       = aws_iam_role.lambda_role_create_invoice.name
  policy_arn = aws_iam_policy.lambda_s3_policy.arn
}

resource "aws_lambda_function" "lambda_create_invoice" {
  function_name = "lambda-create-invoice"
  role          = aws_iam_role.lambda_role_create_invoice.arn
  handler       = "StarkBank.CreateInvoice::StarkBank.CreateInvoice.CreateInvoiceFunction::FunctionHandler"
  runtime       = "dotnet8"
  filename         = "../src/StarkBank/Application/StarkBank.CreateInvoice/bin/Release/release.zip"
  source_code_hash = filebase64sha256("../src/StarkBank/Application/StarkBank.CreateInvoice/bin/Release/release.zip")
  timeout = 20
  environment {
    variables = {
      S3_BUCKET_NAME        = var.s3_bucket_name,
      PRIVATE_KEY_NAME      = var.private_key_name,
      STARKBANK_ENVIRONMENT = var.starkbank_environment,
      STARKBANK_PROJECT_ID  = var.starkbank_project_id
    }
  }
}

resource "aws_lambda_function" "lambda_disable_event_rule" {
  function_name = "lambda-disable-event-rule"
  role          = aws_iam_role.lambda_role_create_invoice.arn
  handler       = "StarkBank.DisableEventRule::StarkBank.DisableEventRule.FunctionHandler"
  runtime       = "dotnet8"
  filename         = "../src/StarkBank/Application/StarkBank.DisableEventRule/bin/Release/release.zip"
  source_code_hash = filebase64sha256("../src/StarkBank/Application/StarkBank.DisableEventRule/bin/Release/release.zip")
  timeout = 20
  environment {
    variables = {
      EVENT_RULE_NAME = aws_cloudwatch_event_rule.every_3_hours.name
    }
  }
}


resource "aws_cloudwatch_event_rule" "every_3_hours" {
  name                = "every_3_hours"
  description         = "Dispara a função Lambda a cada 3 horas"
  schedule_expression = "rate(3 hours)"
}


resource "aws_cloudwatch_event_rule" "stop_after_24_hours" {
  name                = "stop_after_24_hours"
  description         = "Disables the every_3_hours rule after 24 hours"
  schedule_expression = "rate(24 hours)"
}

resource "aws_lambda_permission" "allow_eventbridge" {
  statement_id  = "AllowExecutionFromEventBridge"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.lambda_create_invoice.function_name
  principal     = "events.amazonaws.com"
  source_arn    = aws_cloudwatch_event_rule.every_3_hours.arn
}

resource "aws_lambda_permission" "allow_eventbridge_for_disable" {
  statement_id  = "AllowExecutionFromEventBridgeForDisable"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.lambda_disable_event_rule.function_name
  principal     = "events.amazonaws.com"
  source_arn    = aws_cloudwatch_event_rule.stop_after_24_hours.arn
}

resource "aws_cloudwatch_event_target" "invoke_lambda" {
  rule      = aws_cloudwatch_event_rule.every_3_hours.name
  target_id = "lambda-create-invoice-target"
  arn       = aws_lambda_function.lambda_create_invoice.arn
}

resource "aws_cloudwatch_event_target" "invoke_disable_lambda" {
  rule      = aws_cloudwatch_event_rule.stop_after_24_hours.name
  target_id = "lambda-disable-event-rule-target"
  arn       = aws_lambda_function.lambda_disable_event_rule.arn
}