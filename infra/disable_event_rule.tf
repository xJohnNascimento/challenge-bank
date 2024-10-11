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

resource "aws_lambda_function" "lambda_disable_event_rule" {
  function_name = "lambda-disable-event-rule"
  role          = aws_iam_role.lambda_role_create_invoice.arn
  handler       = "StarkBank.DisableEventRule::StarkBank.DisableEventRule.DisableEventRuleFunction::FunctionHandler"
  runtime       = "dotnet8"
  filename         = "../src/StarkBank/Application/StarkBank.DisableEventRule/bin/Release/release.zip"
  source_code_hash = filebase64sha256("../src/StarkBank/Application/StarkBank.DisableEventRule/bin/Release/release.zip")
  timeout = 20
  environment {
    variables = {
      EVENT_RULE_NAME = aws_cloudwatch_event_rule.every_3_hours.name
      THIS_EVENT_RULE_NAME = aws_cloudwatch_event_rule.stop_after_24_hours.name
    }
  }
}

resource "aws_cloudwatch_event_rule" "stop_after_24_hours" {
  name                = "stop_after_24_hours"
  description         = "Disables the every_3_hours rule after 24 hours"
  schedule_expression = "rate(24 hours)"
}

resource "aws_lambda_permission" "allow_eventbridge_for_disable" {
  statement_id  = "AllowExecutionFromEventBridgeForDisable"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.lambda_disable_event_rule.function_name
  principal     = "events.amazonaws.com"
  source_arn    = aws_cloudwatch_event_rule.stop_after_24_hours.arn
}

resource "aws_cloudwatch_event_target" "invoke_disable_lambda" {
  rule      = aws_cloudwatch_event_rule.stop_after_24_hours.name
  target_id = "lambda-disable-event-rule-target"
  arn       = aws_lambda_function.lambda_disable_event_rule.arn
}