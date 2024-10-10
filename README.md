# StarkBank Challenge

## Overview

This project is developed as part of the StarkBank Challenge. The goal is to create an integration that generates invoices every 3 hours, processes invoice payment webhooks, and transfers the received amount (after deducting fees) to the Stark Bank account.

## Architecture

The solution is built using AWS services and consists of several components:
- **EventBridge**: Triggers the invoice creation Lambda every 3 hours.
- **Lambda Functions**: 
    - `create-invoice`: Generates 8-12 invoices at each interval.
    - `invoice-webhook-receiver`: Receives webhook notifications from StarkBank when an invoice is paid.
    - `process-invoice-payment`: Processes the payment and triggers a transfer to the Stark Bank account.
- **API Gateway**: Handles incoming webhook requests.
- **SNS & SQS**: SNS topic forwards the webhook data to an SQS queue, from where the `process-invoice-payment` Lambda retrieves it.

## Challenge Requirements

1. **Invoice Creation**: 
    - Issue 8 to 12 invoices every 3 hours for a total of 24 hours.
    - Invoices are sent to random people in the StarkBank Sandbox environment.

2. **Payment Handling**:
    - Receive webhook callbacks when invoices are paid.
    - Transfer the invoice amount (minus fees) to the following account:
        - **Bank Code**: 20018183
        - **Branch**: 0001
        - **Account**: 6341320293482496
        - **Account Name**: Stark Bank S.A.
        - **Tax ID**: 20.018.183/0001-80
        - **Account Type**: Payment

## AWS Architecture Diagram

![AWS Architecture](./docs/StarkBank.png)

## Setup Instructions

### Prerequisites

- **AWS Account**
- **StarkBank Sandbox Account**
- **Terraform**
- **.NET 8 SDK**

### Installation Steps

Follow the steps below to set up and run the project:

1. **Generate Public and Private Keys:**
   - You will need to generate a public-private key pair to authenticate with StarkBank Sandbox. Use the following commands to generate the keys:
     https://starkbank.com/faq/how-to-create-ecdsa-keys

2. **Create a StarkBank Sandbox Project:**
   - Go to the [StarkBank Sandbox](https://sandbox.starkbank.com) and create a new project using the **publicKey.pem** file you just generated.
   - Once the project is created, copy the **Project ID** that was generated.

3. **Update Terraform Variables:**
   - In the `variable.tf` file of the project, update the variable `starkbank_project_id` with the Project ID you just copied from the StarkBank Sandbox:
     ```hcl
     variable "starkbank_project_id" {
        default     = "PASTE YOUR PROJECT ID"
        description = "Project ID"
        }
     ```

4. **Update AWS Credentials:**
   - Open the `~/.aws/credentials` file and add your AWS credentials:
     ```ini
     [default]
     aws_access_key_id = YOUR_ACCESS_KEY
     aws_secret_access_key = YOUR_SECRET_KEY
     region = sa-east-1
     ```

5. **Publish .NET Projects and Compress Files for Deployment:**
   - Open a terminal or PowerShell and navigate to the root folder of the project.
   - For each project in the solution (`StarkBank.CreateInvoice`, `StarkBank.InvoiceWebhookReceiver`, and `StarkBank.ProcessInvoicePayment`), run the following command:
     ```bash
     dotnet publish -c Release -r linux-x64 --self-contained false -o ./bin/release/publish
     powershell Compress-Archive -Path ./bin/release/publish/* -DestinationPath ./bin/release/release.zip -force
     ```

6. **Deploy with Terraform:**
   - Initialize and apply the Terraform scripts to deploy the AWS resources:
     ```bash
     terraform init
     terraform plan
     terraform apply
     ```

7. **Upload the Private Key to S3:**
    - After the deployment is complete, navigate to the S3 bucket `techchallenge-starkbank` created by Terraform.
    - Upload the **privateKey.pem** to the bucket for secure access by the application.

## Running the Application

Once deployed, the application will:
- Automatically issue invoices every 3 hours.
- Listen for webhook notifications on payments.
- Process payments and make the necessary transfers to the StarkBank account.

### Monitoring and Logging

Use **AWS CloudWatch** to monitor Lambda functions and check logs for any issues.
