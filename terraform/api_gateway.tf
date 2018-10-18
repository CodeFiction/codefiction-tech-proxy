resource "aws_api_gateway_rest_api" "cf_proxy_gateway" {
  name = "cf_proxy_gateway"
  description = "Terraform codefiction.tech proxy api gateway"
}

# Add gateway http method to API Gateway
resource "aws_api_gateway_method" "proxy_root" {
  rest_api_id = "${aws_api_gateway_rest_api.cf_proxy_gateway.id}"
  resource_id = "${aws_api_gateway_rest_api.cf_proxy_gateway.root_resource_id}"
  http_method = "ANY"
  authorization = "NONE"
}

# Create integrate between root http method of the API Gateway and lambda function 
resource "aws_api_gateway_integration" "lambda_root" {
  rest_api_id = "${aws_api_gateway_rest_api.cf_proxy_gateway.id}"
  resource_id = "${aws_api_gateway_method.proxy_root.resource_id}"
  http_method = "${aws_api_gateway_method.proxy_root.http_method}"

  integration_http_method = "POST"
  type = "AWS_PROXY"
  uri = "${aws_lambda_function.cf_proxy.invoke_arn}"
}

# Add proxy resource to API Gateway
resource "aws_api_gateway_resource" "proxy" {
  rest_api_id = "${aws_api_gateway_rest_api.cf_proxy_gateway.id}"
  parent_id = "${aws_api_gateway_rest_api.cf_proxy_gateway.root_resource_id}"
  path_part = "{proxy+}"
}

# Add gateway http method to proxy resource
resource "aws_api_gateway_method" "proxy" {
  rest_api_id = "${aws_api_gateway_rest_api.cf_proxy_gateway.id}"
  resource_id = "${aws_api_gateway_resource.proxy.id}"
  http_method = "ANY"
  authorization = "NONE"
}

# Create integrate between http method of the proxy resource and lambda function 
resource "aws_api_gateway_integration" "lambda" {
  rest_api_id = "${aws_api_gateway_rest_api.cf_proxy_gateway.id}"
  resource_id = "${aws_api_gateway_method.proxy.resource_id}"
  http_method = "${aws_api_gateway_method.proxy.http_method}"

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
  stage_name = "Prod"
}

# add custom domain name to API Gateway - codefiction.tech
resource "aws_api_gateway_domain_name" "codefiction_tech" {
  depends_on = [
      "aws_route53_record.cert-valid-root",
      "aws_route53_record.cert-valid-www"
  ]
  domain_name = "codefiction.tech"

  certificate_arn = "${aws_acm_certificate.cert.arn}"
}

# add custom domain name to API Gateway - www.codefiction.tech
resource "aws_api_gateway_domain_name" "www_codefiction_tech" {
  depends_on = [
      "aws_route53_record.cert-valid-root",
      "aws_route53_record.cert-valid-www"
  ]
  domain_name = "www.codefiction.tech"

  certificate_arn = "${aws_acm_certificate.cert.arn}"
}

# add base path mapping to custom domain name - codefiction.tech
resource "aws_api_gateway_base_path_mapping" "Prod-root" {
  api_id = "${aws_api_gateway_rest_api.cf_proxy_gateway.id}"
  stage_name = "${aws_api_gateway_deployment.cf_proxy_deploy.stage_name}"
  domain_name = "${aws_api_gateway_domain_name.codefiction_tech.domain_name}"
}

# add base path mapping to custom domain name - www.codefiction.tech
resource "aws_api_gateway_base_path_mapping" "Prod-www" {
  api_id = "${aws_api_gateway_rest_api.cf_proxy_gateway.id}"
  stage_name = "${aws_api_gateway_deployment.cf_proxy_deploy.stage_name}"
  domain_name = "${aws_api_gateway_domain_name.www_codefiction_tech.domain_name}"
}