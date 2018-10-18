provider "aws" {
  region = "eu-central-1"
  profile = "codefiction"
}

# variable "app_version" { 
# }

# variable "bucket_postfix" {
#     default = "test"
# }

resource "aws_lambda_function" "cf_proxy" {
  function_name = "CodefictionTechProxy"

  # The bucket name as created earlier with "aws s3api create-bucket"
  s3_bucket = "codefiction-tech-proxy-lambda"
  s3_key    = "publish.zip"

  # "main" is the filename within the zip file (main.js) and "handler"
  # is the name of the property under which the handler function was
  # exported in that file.
  handler = "CodefictionTech.Proxy::CodefictionTech.Proxy.LambdaEntryPoint::FunctionHandlerAsync"
  runtime = "dotnetcore2.1"  
  timeout = "60"

  role = "${aws_iam_role.lambda_exec.arn}"

  tags {
    Name = "cf_proxy"
  }
}

resource "aws_lambda_permission" "cf_proxy_permission_root" {
  statement_id = "AllowAPIGatewayInvokeRoot"
  action = "lambda:invokeFunction"
  function_name = "${aws_lambda_function.cf_proxy.arn}"
  principal = "apigateway.amazonaws.com"

  # The /*/* portion grants access from any method on any resource
  # within the API Gateway "REST API".
  source_arn = "${aws_api_gateway_deployment.cf_proxy_deploy.execution_arn}/*"
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

resource "aws_iam_policy_attachment" "attach_aws_lambda_full_to_lambda_exec" {
  name = "attach_aws_lambda_basic_to_lambda_exec"
  roles = ["${aws_iam_role.lambda_exec.name}"]
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"
}
