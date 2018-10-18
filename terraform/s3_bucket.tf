resource "aws_s3_bucket" "codefiction_tech_proxy-s3" {
  bucket = "codefiction-tech-proxy-lambda"
  force_destroy = true

  tags {
      Name = "codefiction-tech-proxy"
  }
}
