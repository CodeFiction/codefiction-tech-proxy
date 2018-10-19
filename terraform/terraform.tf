terraform {
    backend "s3" {
        encrypt = true
        bucket = "cf-proxy-terraform-remote-state-storage"
        dynamodb_table = "cf-proxy-terraform-state-lock-dynamo"
        key = "./terraform.tfstate"
    }
}