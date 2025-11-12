#!/bin/bash
# Initialize Terraform and validate configuration

set -e

ENVIRONMENT="${1:-dev}"
TERRAFORM_DIR="./infrastructure/terraform/environments/$ENVIRONMENT"

echo "Initializing Terraform for $ENVIRONMENT environment..."

if [ ! -d "$TERRAFORM_DIR" ]; then
    echo "Error: Environment directory not found: $TERRAFORM_DIR"
    exit 1
fi

cd "$TERRAFORM_DIR"

echo "Running terraform init..."
terraform init

echo "Running terraform validate..."
terraform validate

echo "Running terraform fmt check..."
terraform fmt -check -recursive

echo "Running terraform plan..."
terraform plan

echo "✅ Terraform validation complete for $ENVIRONMENT"
