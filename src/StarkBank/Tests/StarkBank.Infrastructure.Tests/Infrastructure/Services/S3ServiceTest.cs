using Moq;
using StarkBank.Domain.Interfaces.Infrastructure;
using StarkBank.Infrastructure.Services;
using System.Text;

namespace StarkBank.Infrastructure.Tests.Infrastructure.Services
{
    public class S3ServiceTest
    {
        [Fact]
        public async Task GetTextFile_ReturnsFileContent_WhenFileExists()
        {
            const string bucketName = "test-bucket";
            const string fileKey = "test-file.txt";
            const string expectedContent = "Conteúdo do arquivo de teste.";

            var mockS3Client = new Mock<IS3Client>();

            var responseStream = new MemoryStream(Encoding.UTF8.GetBytes(expectedContent));

            mockS3Client
                .Setup(client => client.GetObjectContentAsync(bucketName, fileKey))
                .ReturnsAsync(responseStream);

            var s3Service = new S3Service(mockS3Client.Object);

            var result = await s3Service.GetTextFile(bucketName, fileKey);

            Assert.Equal(expectedContent, result);
        }

        [Fact]
        public async Task GetTextFile_ThrowsException_WhenGetObjectContentAsyncThrowsException()
        {
            const string bucketName = "test-bucket";
            const string fileKey = "non-existent-file.txt";
            var expectedException = new Exception("Arquivo não encontrado.");

            var mockS3Client = new Mock<IS3Client>();

            mockS3Client
                .Setup(client => client.GetObjectContentAsync(bucketName, fileKey))
                .ThrowsAsync(expectedException);

            var s3Service = new S3Service(mockS3Client.Object);

            var exception = await Assert.ThrowsAsync<Exception>(() => s3Service.GetTextFile(bucketName, fileKey));
            Assert.Equal(expectedException.Message, exception.Message);
        }

        [Fact]
        public async Task GetTextFile_ReturnsEmptyString_WhenFileIsEmpty()
        {
            const string bucketName = "test-bucket";
            const string fileKey = "empty-file.txt";
            var expectedContent = string.Empty;

            var mockS3Client = new Mock<IS3Client>();

            var responseStream = new MemoryStream(Encoding.UTF8.GetBytes(expectedContent));

            mockS3Client
                .Setup(client => client.GetObjectContentAsync(bucketName, fileKey))
                .ReturnsAsync(responseStream);

            var s3Service = new S3Service(mockS3Client.Object);

            var result = await s3Service.GetTextFile(bucketName, fileKey);

            Assert.Equal(expectedContent, result);
        }

        [Fact]
        public async Task GetTextFile_DisposesStream_AfterReading()
        {
            const string bucketName = "test-bucket";
            const string fileKey = "test-file.txt";
            const string expectedContent = "Teste de disposição de stream.";

            var mockS3Client = new Mock<IS3Client>();

            var responseStream = new MemoryStream(Encoding.UTF8.GetBytes(expectedContent));

            mockS3Client
                .Setup(client => client.GetObjectContentAsync(bucketName, fileKey))
                .ReturnsAsync(responseStream);

            var s3Service = new S3Service(mockS3Client.Object);

            var result = await s3Service.GetTextFile(bucketName, fileKey);

            Assert.Equal(expectedContent, result);
            Assert.False(responseStream.CanRead);
        }
    }
}
