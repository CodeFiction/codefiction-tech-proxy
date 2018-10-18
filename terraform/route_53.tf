provider "aws" {
  # us-east-1 instance
  region = "us-east-1"
  alias = "use1"
}

data "aws_route53_zone" "selected_zone" {
  name = "codefiction.tech"
}

resource "aws_acm_certificate" "cert" {
  domain_name = "codefiction.tech"
  subject_alternative_names = ["www.codefiction.tech"]
  validation_method = "DNS"
  provider = "aws.use1"
}

output "validation-options" {
  value = "${aws_acm_certificate.cert.domain_validation_options}"
}

resource "aws_route53_record" "cert-valid-root" {
  name    = "${aws_acm_certificate.cert.domain_validation_options.0.resource_record_name}"
  type    = "${aws_acm_certificate.cert.domain_validation_options.0.resource_record_type}"
  records = ["${aws_acm_certificate.cert.domain_validation_options.0.resource_record_value}"]
  ttl     = "3600"
  zone_id = "${data.aws_route53_zone.selected_zone.zone_id}"
}

resource "aws_route53_record" "cert-valid-www" {
  name    = "${aws_acm_certificate.cert.domain_validation_options.1.resource_record_name}"
  type    = "${aws_acm_certificate.cert.domain_validation_options.1.resource_record_type}"
  records = ["${aws_acm_certificate.cert.domain_validation_options.1.resource_record_value}"]
  ttl     = "3600"
  zone_id = "${data.aws_route53_zone.selected_zone.zone_id}"
}

resource "aws_route53_record" "codefiction_tech" {
  zone_id = "${data.aws_route53_zone.selected_zone.id}"

  name = "${aws_api_gateway_domain_name.codefiction_tech.domain_name}"
  type = "A"

  alias {
      name                   = "${aws_api_gateway_domain_name.codefiction_tech.cloudfront_domain_name}"
      zone_id                = "${aws_api_gateway_domain_name.codefiction_tech.cloudfront_zone_id}"
      evaluate_target_health = true
  }
}

resource "aws_route53_record" "www_codefiction_tech" {
  zone_id = "${data.aws_route53_zone.selected_zone.id}"

  name = "${aws_api_gateway_domain_name.www_codefiction_tech.domain_name}"
  type = "A"

  alias {
      name                   = "${aws_api_gateway_domain_name.www_codefiction_tech.cloudfront_domain_name}"
      zone_id                = "${aws_api_gateway_domain_name.www_codefiction_tech.cloudfront_zone_id}"
      evaluate_target_health = true
  }
}
