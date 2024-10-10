resource "aws_s3_bucket" "techchallenge_bucket" {
  bucket = "techchallenge-starkbank"
}

resource "aws_s3_bucket_public_access_block" "block_public_access" {
  bucket                  = aws_s3_bucket.techchallenge_bucket.id
  block_public_acls       = true
  block_public_policy     = true
  ignore_public_acls      = true
  restrict_public_buckets = true
}

resource "aws_s3_bucket_versioning" "versioning" {
  bucket = aws_s3_bucket.techchallenge_bucket.id

  versioning_configuration {
    status = "Enabled"
  }
}