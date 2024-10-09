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

![AWS Architecture](./StarkBank.png)

## Setup Instructions

### Prerequisites

- **AWS Account**
- **StarkBank Sandbox Account**
- **Terraform**

### Installation Steps

...

## Running the Application

Once deployed, the application will:
- Automatically issue invoices every 3 hours.
- Listen for webhook notifications on payments.
- Process payments and make the necessary transfers to the StarkBank account.

### Monitoring and Logging

Use **AWS CloudWatch** to monitor Lambda functions and check logs for any issues.
