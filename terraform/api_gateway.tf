resource "aws_api_gateway_rest_api" "cf_proxy_gateway" {
  name = "cf_proxy_gateway"
  description = "Terraform codefiction.tech proxy api gateway"
}

resource "aws_api_gateway_resource" "proxy" {
  rest_api_id = "${aws_api_gateway_rest_api.cf_proxy_gateway.id}"
  parent_id = "${aws_api_gateway_rest_api.cf_proxy_gateway.root_resource_id}"
  path_part = "{proxy+}"
}

resource "aws_api_gateway_method" "proxy" {
  rest_api_id = "${aws_api_gateway_rest_api.cf_proxy_gateway.id}"
  resource_id = "${aws_api_gateway_resource.proxy.id}"
  http_method = "ANY"
  authorization = "NONE"
}

# resource "aws_api_gateway_method_response" "method_response" {
#   rest_api_id = "${aws_api_gateway_rest_api.cf_proxy_gateway.id}"
#   resource_id = "${aws_api_gateway_resource.proxy.id}"
#   http_method = "${aws_api_gateway_method.proxy.http_method}"
#   status_code = "200"
# }

# resource "aws_api_gateway_integration_response" "integration_response" {
#    rest_api_id = "${aws_api_gateway_rest_api.cf_proxy_gateway.id}"
#    resource_id = "${aws_api_gateway_resource.proxy.id}"
#    http_method = "${aws_api_gateway_method.proxy.http_method}"
#    status_code = "${aws_api_gateway_method_response.method_response.status_code}"

#    response_templates = {
#        "application/json" = ""
#    } 
# }

resource "aws_api_gateway_integration" "lambda" {
  rest_api_id = "${aws_api_gateway_rest_api.cf_proxy_gateway.id}"
  resource_id = "${aws_api_gateway_method.proxy.resource_id}"
  http_method = "${aws_api_gateway_method.proxy.http_method}"

  integration_http_method = "POST"
  type = "AWS_PROXY"
  uri = "${aws_lambda_function.cf_proxy.invoke_arn}"
}

resource "aws_api_gateway_method" "proxy_root" {
  rest_api_id = "${aws_api_gateway_rest_api.cf_proxy_gateway.id}"
  resource_id = "${aws_api_gateway_rest_api.cf_proxy_gateway.root_resource_id}"
  http_method = "ANY"
  authorization = "NONE"
}

# resource "aws_api_gateway_method_response" "root_method_response" {
#   rest_api_id = "${aws_api_gateway_rest_api.cf_proxy_gateway.id}"
#   resource_id = "${aws_api_gateway_rest_api.cf_proxy_gateway.root_resource_id}"
#   http_method = "${aws_api_gateway_method.proxy_root.http_method}"
#   status_code = "200"

#   response_models = {
#         "application/json" = "Empty"
#   }
# }

# resource "aws_api_gateway_integration_response" "root_integration_response" {
#    rest_api_id = "${aws_api_gateway_rest_api.cf_proxy_gateway.id}"
#    resource_id = "${aws_api_gateway_rest_api.cf_proxy_gateway.root_resource_id}"
#    http_method = "${aws_api_gateway_method.proxy_root.http_method}"
#    status_code = "${aws_api_gateway_method_response.root_method_response.status_code}"

#    response_templates = {
#        "application/json" = ""
#    } 
# }

resource "aws_api_gateway_integration" "lambda_root" {
  rest_api_id = "${aws_api_gateway_rest_api.cf_proxy_gateway.id}"
  resource_id = "${aws_api_gateway_method.proxy_root.resource_id}"
  http_method = "${aws_api_gateway_method.proxy_root.http_method}"

  integration_http_method = "POST"
  type = "AWS_PROXY"
  uri = "${aws_lambda_function.cf_proxy.invoke_arn}"
}

resource "aws_api_gateway_deployment" "cf_proxy_deploy" {
  depends_on = [
      "aws_api_gateway_integration.lambda",
      "aws_api_gateway_integration.lambda_root"
  ]

  rest_api_id = "${aws_api_gateway_rest_api.cf_proxy_gateway.id}"
  stage_name = "test"

}