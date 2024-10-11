using Amazon.Lambda.Core;
using Moq;
using StarkBank.CreateInvoice;
using StarkBank.Domain.Interfaces.Application.CreateInvoice;
using StarkBank.Domain.Interfaces.Infrastructure;
using StarkBank.Domain.Interfaces.Shared;

namespace StarkBank.Application.Tests.CreateInvoice
{
    public class CreateInvoiceTest
    {
        private readonly CreateInvoiceFunction _function;
        private readonly Mock<IS3Service> _mockS3Service;
        private readonly Mock<IStarkBankAuthenticationService> _mockAuthenticationService;
        private readonly Mock<IInvoiceService> _mockInvoiceService;
        private readonly Mock<IClientGeneratorService> _mockClientGeneratorService;
        private readonly Mock<ILambdaContext> _mockLambdaContext;
        private readonly Mock<ILambdaLogger> _mockLogger;

        public CreateInvoiceTest()
        {
            _mockS3Service = new Mock<IS3Service>();
            _mockAuthenticationService = new Mock<IStarkBankAuthenticationService>();
            _mockInvoiceService = new Mock<IInvoiceService>();
            _mockClientGeneratorService = new Mock<IClientGeneratorService>();
            _mockLambdaContext = new Mock<ILambdaContext>();
            _mockLogger = new Mock<ILambdaLogger>();

            _mockLambdaContext.Setup(c => c.Logger).Returns(_mockLogger.Object);

            _function = new CreateInvoiceFunction(
                _mockAuthenticationService.Object,
                _mockS3Service.Object,
                _mockInvoiceService.Object,
                _mockClientGeneratorService.Object
            );
        }

        [Fact]
        public async Task FunctionHandler_Should_CreateInvoices_When_AllDependenciesWork()
        {
            Environment.SetEnvironmentVariable("S3_BUCKET_NAME", "test-bucket");
            Environment.SetEnvironmentVariable("PRIVATE_KEY_NAME", "test-key-name");
            Environment.SetEnvironmentVariable("STARKBANK_ENVIRONMENT", "sandbox");
            Environment.SetEnvironmentVariable("STARKBANK_PROJECT_ID", "project-id");

            const string privateKeyContent = """
                -----BEGIN EC PRIVATE KEY-----
                MHQCAQEEIAylf2v7MvJwCVB8ndn2x4jjkQoDCgAKMlH0CbEqbAajoAcGBSuBBAAK
                oUQDQgAEpAN+dsC3SDyKoQHEBz+56sL7jLXFrsV+3HoxOe9cQg36jZmse5nBBIyp
                LxXv5JqaGME9qq7VkaMntbwULS41bw==
                -----END EC PRIVATE KEY-----
                """;

            _mockS3Service.Setup(s => s.GetTextFile("test-bucket", "test-key-name"))
                .ReturnsAsync(privateKeyContent);

            var project = new Project(
                environment: "sandbox",
                id: "project-id",
                privateKey: privateKeyContent
            );

            _mockAuthenticationService.Setup(a => a.InitializeAsync(privateKeyContent, "sandbox", "project-id"))
                .Returns(Task.CompletedTask);
            _mockAuthenticationService.Setup(a => a.GetProject())
                .Returns(project);

            _mockClientGeneratorService.Setup(c => c.GenerateName()).Returns("John");
            _mockClientGeneratorService.Setup(c => c.GenerateCpf()).Returns("124.550.680-30");

            await _function.FunctionHandler(_mockLambdaContext.Object);

            _mockS3Service.Verify(s => s.GetTextFile("test-bucket", "test-key-name"), Times.Once);
            _mockAuthenticationService.Verify(a => a.InitializeAsync(privateKeyContent, "sandbox", "project-id"), Times.Once);
            _mockAuthenticationService.Verify(a => a.GetProject(), Times.Once);
            _mockClientGeneratorService.Verify(c => c.GenerateName(), Times.AtLeastOnce);
            _mockClientGeneratorService.Verify(c => c.GenerateCpf(), Times.AtLeastOnce);
            _mockInvoiceService.Verify(i => i.Create(It.IsAny<List<Invoice>>(), project), Times.Once);
            _mockLogger.Verify(l => l.LogError(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task FunctionHandler_Should_LogError_When_S3_BUCKET_NAME_NotSet()
        {
            Environment.SetEnvironmentVariable("S3_BUCKET_NAME", null);
            Environment.SetEnvironmentVariable("PRIVATE_KEY_NAME", "test-key-name");
            Environment.SetEnvironmentVariable("STARKBANK_ENVIRONMENT", "sandbox");
            Environment.SetEnvironmentVariable("STARKBANK_PROJECT_ID", "project-id");

            await _function.FunctionHandler(_mockLambdaContext.Object);

            _mockLogger.Verify(l => l.LogError(It.Is<string>(s => s.Contains("The environment variable S3_BUCKET_NAME is not set"))), Times.Once);

            _mockS3Service.Verify(s => s.GetTextFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _mockAuthenticationService.Verify(a => a.InitializeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _mockInvoiceService.Verify(i => i.Create(It.IsAny<List<Invoice>>(), It.IsAny<Project>()), Times.Never);
        }

        [Fact]
        public async Task FunctionHandler_Should_LogError_When_S3ServiceThrowsException()
        {
            Environment.SetEnvironmentVariable("S3_BUCKET_NAME", "test-bucket");
            Environment.SetEnvironmentVariable("PRIVATE_KEY_NAME", "test-key-name");
            Environment.SetEnvironmentVariable("STARKBANK_ENVIRONMENT", "sandbox");
            Environment.SetEnvironmentVariable("STARKBANK_PROJECT_ID", "project-id");

            _mockS3Service.Setup(s => s.GetTextFile("test-bucket", "test-key-name"))
                .ThrowsAsync(new Exception("S3 error"));

            await _function.FunctionHandler(_mockLambdaContext.Object);

            _mockLogger.Verify(l => l.LogError(It.Is<string>(s => s.Contains("Error:S3 error"))), Times.Once);

            _mockAuthenticationService.Verify(a => a.InitializeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _mockInvoiceService.Verify(i => i.Create(It.IsAny<List<Invoice>>(), It.IsAny<Project>()), Times.Never);
        }

        [Fact]
        public async Task FunctionHandler_Should_LogError_When_AuthenticationFails()
        {
            Environment.SetEnvironmentVariable("S3_BUCKET_NAME", "test-bucket");
            Environment.SetEnvironmentVariable("PRIVATE_KEY_NAME", "test-key-name");
            Environment.SetEnvironmentVariable("STARKBANK_ENVIRONMENT", "sandbox");
            Environment.SetEnvironmentVariable("STARKBANK_PROJECT_ID", "project-id");

            var privateKeyContent = "test-private-key";

            _mockS3Service.Setup(s => s.GetTextFile("test-bucket", "test-key-name"))
                .ReturnsAsync(privateKeyContent);

            _mockAuthenticationService.Setup(a => a.InitializeAsync(privateKeyContent, "sandbox", "project-id"))
                .ThrowsAsync(new Exception("Authentication error"));

            await _function.FunctionHandler(_mockLambdaContext.Object);

            _mockLogger.Verify(l => l.LogError(It.Is<string>(s => s.Contains("Error:Authentication error"))), Times.Once);

            _mockInvoiceService.Verify(i => i.Create(It.IsAny<List<Invoice>>(), It.IsAny<Project>()), Times.Never);
        }

        [Fact]
        public async Task FunctionHandler_Should_LogError_When_InvoiceServiceThrowsException()
        {
            Environment.SetEnvironmentVariable("S3_BUCKET_NAME", "test-bucket");
            Environment.SetEnvironmentVariable("PRIVATE_KEY_NAME", "test-key-name");
            Environment.SetEnvironmentVariable("STARKBANK_ENVIRONMENT", "sandbox");
            Environment.SetEnvironmentVariable("STARKBANK_PROJECT_ID", "project-id");

            const string privateKeyContent = """
                                             -----BEGIN EC PRIVATE KEY-----
                                             MHQCAQEEIAylf2v7MvJwCVB8ndn2x4jjkQoDCgAKMlH0CbEqbAajoAcGBSuBBAAK
                                             oUQDQgAEpAN+dsC3SDyKoQHEBz+56sL7jLXFrsV+3HoxOe9cQg36jZmse5nBBIyp
                                             LxXv5JqaGME9qq7VkaMntbwULS41bw==
                                             -----END EC PRIVATE KEY-----
                                             """;

            _mockS3Service.Setup(s => s.GetTextFile("test-bucket", "test-key-name"))
                .ReturnsAsync(privateKeyContent);

            var project = new Project(
                environment: "sandbox",
                id: "project-id",
                privateKey: privateKeyContent
            );

            _mockAuthenticationService.Setup(a => a.InitializeAsync(privateKeyContent, "sandbox", "project-id"))
                .Returns(Task.CompletedTask);
            _mockAuthenticationService.Setup(a => a.GetProject())
                .Returns(project);

            _mockClientGeneratorService.Setup(c => c.GenerateName()).Returns("John");
            _mockClientGeneratorService.Setup(c => c.GenerateCpf()).Returns("124.550.680-30");

            _mockInvoiceService.Setup(i => i.Create(It.IsAny<List<Invoice>>(), project))
                .Throws(new Exception("Invoice creation error"));

            await _function.FunctionHandler(_mockLambdaContext.Object);

            _mockLogger.Verify(l => l.LogError(It.Is<string>(s => s.Contains("Error:Invoice creation error"))), Times.Once);
        }
    }
}
