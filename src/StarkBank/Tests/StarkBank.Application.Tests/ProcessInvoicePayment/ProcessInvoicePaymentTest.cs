using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Moq;
using StarkBank.Domain.Interfaces.Application.ProcessInvoicePayment;
using StarkBank.Domain.Interfaces.Infrastructure;
using StarkBank.ProcessInvoicePayment;

namespace StarkBank.Application.Tests.ProcessInvoicePayment
{
    public class ProcessInvoicePaymentTest
    {
        private readonly ProcessInvoicePaymentFunction _function;
        private readonly Mock<IStarkBankAuthenticationService> _mockAuthenticationService;
        private readonly Mock<IS3Service> _mockS3Service;
        private readonly Mock<ILambdaContext> _mockLambdaContext;
        private readonly Mock<ILambdaLogger> _mockLogger;
        private readonly Mock<ITransferService> _mockTransferService;

        #region Json

        private const string SnsNotification =
            """{"Type": "Notification", "MessageId": "8493f63c-5e36-5345-a898-a9be6f20cecd", "TopicArn": "arn:aws:sns:sa-east-1:943457664629:sns-invoice-payment", "Message": "{\"event\": {\"created\": \"2024-10-10T19:40:24.641669+00:00\", \"id\": \"6226686284660736\", \"log\": {\"created\": \"2024-10-10T19:40:17.876633+00:00\", \"errors\": [], \"id\": \"4596228889247744\", \"invoice\": {\"amount\": 368803, \"brcode\": \"00020101021226890014br.gov.bcb.pix2567brcode-h.sandbox.starkinfra.com/v2/670d7abd88b24013a355b1facd03f1455204000053039865802BR5925Stark Bank S.A. - Institu6009Sao Paulo62070503***63048764\", \"created\": \"2024-10-10T19:35:52.568799+00:00\", \"descriptions\": [], \"discountAmount\": 0, \"discounts\": [], \"displayDescription\": \"ditto\", \"due\": \"2024-10-12T19:35:52.551076+00:00\", \"expiration\": 5097600, \"fee\": 0, \"fine\": 2.0, \"fineAmount\": 0, \"id\": \"5053493438054400\", \"interest\": 1.0, \"interestAmount\": 0, \"link\": \"https://challenge-jonathan-nascimento.sandbox.starkbank.com/invoicelink/670d7abd88b24013a355b1facd03f145\", \"name\": \"Bella Rivera\", \"nominalAmount\": 368803, \"pdf\": \"https://sandbox.api.starkbank.com/v2/invoice/670d7abd88b24013a355b1facd03f145.pdf\", \"rules\": [], \"splits\": [], \"status\": \"paid\", \"tags\": [\"e41669227202410101940jn4uepvkq3m\"], \"taxId\": \"153.132.907-11\", \"transactionIds\": [\"10737491985752699395627513031637\"], \"updated\": \"2024-10-10T19:40:17.876728+00:00\"}, \"type\": \"credited\"}, \"subscription\": \"invoice\", \"workspaceId\": \"6299034633371648\"}}", "Timestamp": "2024-10-10T19:45:26.458Z", "SignatureVersion": "1", "Signature": "iNwdO6XALu1WNq6hLHs9PFRUprHOTK8dqSwYxAC8RijzCc6Opj5MaYkq4Qx9qppmA9L48RLgGXkNEY0PJiSVU0wZtxq8RfI8kEZxWGm0QW9I0Hoh/AIrnDJxnC/BodKhANZjMC3KawNkHyM4sXgj+rZbYBE/Iuw3Rgg5FS2Hztppf93vNDzX60Kcty+IddRGg1V0WoxenFKpDpMFxMdtsp/gpPVZF83BkXsC8+qhujipfcaCHAny7bEcLtzQU2CE9Pjga565lbdGbMtU6/aR8fRB01b2kNglxtiar0aKv8nLZ0C4zn4HM3NE8lvSgAvkAvFfB9efVEdHGr+ewa979w==", "SigningCertURL": "https://sns.sa-east-1.amazonaws.com/SimpleNotificationService-60eadc530605d63b8e62a523676ef735.pem", "UnsubscribeURL": "https://sns.sa-east-1.amazonaws.com/?Action=Unsubscribe&SubscriptionArn=arn:aws:sns:sa-east-1:943457664629:sns-invoice-payment:88b01b96-181b-4ded-a5d9-a4a704e70504"}""";

        private const string SnsNotificationWithoutAmount =
                    """{"Type": "Notification", "MessageId": "8493f63c-5e36-5345-a898-a9be6f20cecd", "TopicArn": "arn:aws:sns:sa-east-1:943457664629:sns-invoice-payment", "Message": "{\"event\": {\"created\": \"2024-10-10T19:40:24.641669+00:00\", \"id\": \"6226686284660736\", \"log\": {\"created\": \"2024-10-10T19:40:17.876633+00:00\", \"errors\": [], \"id\": \"4596228889247744\", \"invoice\": {\"brcode\": \"00020101021226890014br.gov.bcb.pix2567brcode-h.sandbox.starkinfra.com/v2/670d7abd88b24013a355b1facd03f1455204000053039865802BR5925Stark Bank S.A. - Institu6009Sao Paulo62070503***63048764\", \"created\": \"2024-10-10T19:35:52.568799+00:00\", \"descriptions\": [], \"discountAmount\": 0, \"discounts\": [], \"displayDescription\": \"ditto\", \"due\": \"2024-10-12T19:35:52.551076+00:00\", \"expiration\": 5097600, \"fee\": 0, \"fine\": 2.0, \"fineAmount\": 0, \"id\": \"5053493438054400\", \"interest\": 1.0, \"interestAmount\": 0, \"link\": \"https://challenge-jonathan-nascimento.sandbox.starkbank.com/invoicelink/670d7abd88b24013a355b1facd03f145\", \"name\": \"Bella Rivera\", \"nominalAmount\": 368803, \"pdf\": \"https://sandbox.api.starkbank.com/v2/invoice/670d7abd88b24013a355b1facd03f145.pdf\", \"rules\": [], \"splits\": [], \"status\": \"paid\", \"tags\": [\"e41669227202410101940jn4uepvkq3m\"], \"taxId\": \"153.132.907-11\", \"transactionIds\": [\"10737491985752699395627513031637\"], \"updated\": \"2024-10-10T19:40:17.876728+00:00\"}, \"type\": \"credited\"}, \"subscription\": \"invoice\", \"workspaceId\": \"6299034633371648\"}}", "Timestamp": "2024-10-10T19:45:26.458Z", "SignatureVersion": "1", "Signature": "iNwdO6XALu1WNq6hLHs9PFRUprHOTK8dqSwYxAC8RijzCc6Opj5MaYkq4Qx9qppmA9L48RLgGXkNEY0PJiSVU0wZtxq8RfI8kEZxWGm0QW9I0Hoh/AIrnDJxnC/BodKhANZjMC3KawNkHyM4sXgj+rZbYBE/Iuw3Rgg5FS2Hztppf93vNDzX60Kcty+IddRGg1V0WoxenFKpDpMFxMdtsp/gpPVZF83BkXsC8+qhujipfcaCHAny7bEcLtzQU2CE9Pjga565lbdGbMtU6/aR8fRB01b2kNglxtiar0aKv8nLZ0C4zn4HM3NE8lvSgAvkAvFfB9efVEdHGr+ewa979w==", "SigningCertURL": "https://sns.sa-east-1.amazonaws.com/SimpleNotificationService-60eadc530605d63b8e62a523676ef735.pem", "UnsubscribeURL": "https://sns.sa-east-1.amazonaws.com/?Action=Unsubscribe&SubscriptionArn=arn:aws:sns:sa-east-1:943457664629:sns-invoice-payment:88b01b96-181b-4ded-a5d9-a4a704e70504"}""";
        #endregion


        public ProcessInvoicePaymentTest()
        {
            _mockAuthenticationService = new Mock<IStarkBankAuthenticationService>();
            _mockS3Service = new Mock<IS3Service>();
            _mockLambdaContext = new Mock<ILambdaContext>();
            _mockLogger = new Mock<ILambdaLogger>();

            _mockLambdaContext.Setup(c => c.Logger).Returns(_mockLogger.Object);
            _mockTransferService = new Mock<ITransferService>();

            _function = new ProcessInvoicePaymentFunction(_mockAuthenticationService.Object, _mockS3Service.Object, _mockTransferService.Object);
        }

        [Fact]
        public async Task FunctionHandler_Should_ProcessInvoice_With_AllFields_Valid()
        {
            var sqsMessage = new SQSEvent.SQSMessage
            {
                Body = SnsNotification
            };

            var sqsEvent = new SQSEvent
            {
                Records = new List<SQSEvent.SQSMessage> { sqsMessage }
            };

            const string privateKeyContent = """
                                         -----BEGIN EC PRIVATE KEY-----
                                         MHQCAQEEIAylf2v7MvJwCVB8ndn2x4jjkQoDCgAKMlH0CbEqbAajoAcGBSuBBAAK
                                         oUQDQgAEpAN+dsC3SDyKoQHEBz+56sL7jLXFrsV+3HoxOe9cQg36jZmse5nBBIyp
                                         LxXv5JqaGME9qq7VkaMntbwULS41bw==
                                         -----END EC PRIVATE KEY-----
                                         """;

            _mockS3Service.Setup(s => s.GetTextFile(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(privateKeyContent);

            var project = new Project("sandbox", "project-id", privateKeyContent);

            _mockAuthenticationService.Setup(a => a.InitializeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            _mockAuthenticationService.Setup(a => a.GetProject()).Returns(project);

            Environment.SetEnvironmentVariable("S3_BUCKET_NAME", "test-bucket");
            Environment.SetEnvironmentVariable("PRIVATE_KEY_NAME", "test-key");
            Environment.SetEnvironmentVariable("STARKBANK_ENVIRONMENT", "sandbox");
            Environment.SetEnvironmentVariable("STARKBANK_PROJECT_ID", "project-id");

            _mockTransferService.Setup(t => t.CreateTransfer(It.IsAny<long>(), It.IsAny<User>()))
                .Returns([]);

            await _function.FunctionHandler(sqsEvent, _mockLambdaContext.Object);

            _mockLogger.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("Processed successfully message"))), Times.Once);
            _mockTransferService.Verify(t => t.CreateTransfer(It.IsAny<long>(), It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task FunctionHandler_Should_LogError_When_InvoiceIsMissingFields()
        {
            var sqsMessage = new SQSEvent.SQSMessage
            {
                Body = SnsNotificationWithoutAmount
            };

            var sqsEvent = new SQSEvent
            {
                Records = new List<SQSEvent.SQSMessage> { sqsMessage }
            };

            await _function.FunctionHandler(sqsEvent, _mockLambdaContext.Object);

            _mockLogger.Verify(l => l.LogError(It.Is<string>(s => s.Contains("Invoice is missing required fields"))), Times.Once);
        }
    }
}
