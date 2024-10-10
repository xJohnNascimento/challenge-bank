using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarkBank.Infrastructure.Tests.Infrastructure.Services
{
    public class S3ServiceTest
    {
        [Fact]
        public async Task GetTextFile_ReturnsFileContent()
        {
            // Arrange
            var bucketName = "test-bucket";
            var fileKey = "test-file.txt";
            var expectedContent = "Conteúdo do arquivo de teste.";

            // Cria um mock do IAmazonS3
            var mockS3Client = new Mock<IAmazonS3>();

            // Configura o método GetObjectAsync para retornar um GetObjectResponse simulado
            var getObjectResponse = new GetObjectResponse
            {
                ResponseStream = new MemoryStream(Encoding.UTF8.GetBytes(expectedContent))
            };

            mockS3Client.Setup(client => client.GetObjectAsync(
                It.IsAny<GetObjectRequest>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(getObjectResponse);

            // Instancia o S3Service com o cliente mockado
            var s3Service = new S3Service(mockS3Client.Object);

            // Act
            var result = await s3Service.GetTextFile(bucketName, fileKey);

            // Assert
            Assert.Equal(expectedContent, result);
        }

        [Fact]
        public async Task GetTextFile_ThrowsException_WhenGetObjectAsyncFails()
        {
            // Arrange
            var bucketName = "test-bucket";
            var fileKey = "test-file.txt";

            var mockS3Client = new Mock<IAmazonS3>();

            // Configura o GetObjectAsync para lançar uma exceção
            mockS3Client.Setup(client => client.GetObjectAsync(
                It.IsAny<GetObjectRequest>(),
                It.IsAny<CancellationToken>()
            )).ThrowsAsync(new AmazonS3Exception("Erro ao acessar o S3"));

            var s3Service = new S3Service(mockS3Client.Object);

            // Act & Assert
            await Assert.ThrowsAsync<AmazonS3Exception>(() => s3Service.GetTextFile(bucketName, fileKey));
        }
    }
}
