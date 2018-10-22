variable "bucket_name" {
    type = "string"
    description = "s3 bucket name that will be used by aws lambda function"
}

variable "package_path" {
    type = "string"
    description = "Package path to upload S3"
}

variable "package_name" {
    type = "string"
    description = "Package path to upload S3"
}

resource "aws_s3_bucket" "codefiction_tech_proxy-s3" {
  bucket = "${var.bucket_name}"

  tags {
      Name = "cf_proxy"
  }
}

resource "aws_s3_bucket_object" "lambda_package" {
  bucket = "${var.bucket_name}"
  key = "${var.package_name}"
  source = "${var.package_path}"
  etag = "${md5(file(var.package_name))}"

  depends_on = [
      "aws_s3_bucket.codefiction_tech_proxy-s3"
  ]
}
