# resource "aws_s3_bucket" "terraform-state-storage-s3" {
#     bucket = "cf-proxy-terraform-remote-state-storage"
 
#     versioning {
#       enabled = true
#     }
 
#     lifecycle {
#       prevent_destroy = true
#     }
 
#     tags {
#       Description = "DynamoDB Terraform State Lock Table"
#       Name = "cf_proxy"
#     }      
# }

# resource "aws_dynamodb_table" "dynamodb-terraform-state-lock" {
#   name = "cf-proxy-terraform-state-lock-dynamo"
#   hash_key = "LockID"
#   read_capacity = 20
#   write_capacity = 20
 
#   attribute {
#     name = "LockID"
#     type = "S"
#   }
 
#   tags {
#     Description = "DynamoDB Terraform State Lock Table"
#     Name = "cf_proxy"
#   }
# }
