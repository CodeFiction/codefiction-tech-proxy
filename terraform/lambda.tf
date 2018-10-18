provider "aws" {
  region = "eu-central-1"
}

# variable "app_version" { 
# }

# variable "bucket_postfix" {
#     default = "test"
# }

resource "aws_lambda_function" "cf_proxy" {
  function_name = "CodefictionTechProxy"

  # The bucket name as created earlier with "aws s3api create-bucket"
  s3_bucket = "codefiction-tech-proxy-temp"
  s3_key    = "publish2.zip"

  # "main" is the filename within the zip file (main.js) and "handler"
  # is the name of the property under which the handler function was
  # exported in that file.
  handler = "CodefictionTech.Proxy::CodefictionTech.Proxy.LambdaEntryPoint::FunctionHandlerAsync"
  runtime = "dotnetcore2.1"  
  timeout = "60"

  role = "${aws_iam_role.lambda_exec.arn}"
}

resource "aws_lambda_permission" "cf_proxy_permission" {
  statement_id = "AllowAPIGatewayInvoke"
  action = "lambda:invokeFunction"
  function_name = "${aws_lambda_function.cf_proxy.arn}"
  principal = "apigateway.amazonaws.com"

  # The /*/* portion grants access from any method on any resource
  # within the API Gateway "REST API".
  source_arn = "${aws_api_gateway_deployment.cf_proxy_deploy.execution_arn}/*/*"
}

output "base_url" {
  value = "${aws_api_gateway_deployment.cf_proxy_deploy.invoke_url}"
}


# IAM role which dictates what other AWS services the Lambda function
# may access.
resource "aws_iam_role" "lambda_exec" {
  name = "codefiction_tech_proxy_lamda"

  assume_role_policy = <<EOF
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Action": "sts:AssumeRole",
      "Principal": {
        "Service": "lambda.amazonaws.com"
      },
      "Effect": "Allow",
      "Sid": ""
    }
  ]
}
EOF
}

resource "aws_iam_policy_attachment" "attach-aws-lambda-full-to-lambda-exec" {
  name = "attach-aws-lambda-full-to-lambda-exe"
  roles = ["${aws_iam_role.lambda_exec.name}"]
  policy_arn = "arn:aws:iam::aws:policy/AWSLambdaFullAccess"
}
