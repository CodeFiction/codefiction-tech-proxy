variable "bucket_name" {
    type = "string"
    description = "s3 bucket name that will be used by aws lambda function"
}

resource "aws_s3_bucket" "codefiction_tech_proxy-s3" {
  bucket = "${var.bucket_name}"

  tags {
      Name = "cf_proxy"
  }
}