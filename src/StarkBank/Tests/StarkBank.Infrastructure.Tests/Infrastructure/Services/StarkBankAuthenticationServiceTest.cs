using StarkBank.Infrastructure.Services;

namespace StarkBank.Infrastructure.Tests.Infrastructure.Services
{
    public class StarkBankAuthenticationServiceTest
    {
        [Fact]
        public async Task InitializeAsync_SetsProjectCorrectly()
        {
            const string privateKey = """
                                      -----BEGIN EC PRIVATE KEY-----
                                                      MHQCAQEEICBm4cS13ZF4Z8L3+Tzl/8NQb1Z9O9QwmlIEnowClOZ1oAcGBSuBBAAK
                                                      oUQDQgAE/qjF/BSI6E0t23qK4zy0YjkA2nbV+J2FBhjBTb8ENjB/Y4Oi3Zf0u2Pr
                                                      XgOx5nGZlO7B5M+6SJX4iFUkV+nSog==
                                                      -----END EC PRIVATE KEY-----
                                      """;
            const string environment = "sandbox";
            const string projectId = "proj-test";

            var authService = new StarkBankAuthenticationService();

            await authService.InitializeAsync(privateKey, environment, projectId);
            var project = authService.GetProject();

            Assert.NotNull(project);
            Assert.Equal(environment, project.Environment);
            Assert.Equal(projectId, project.ID);
            Assert.Equal(privateKey, project.Pem);
        }

        [Fact]
        public void GetProject_ReturnsNull_WhenNotInitialized()
        {
            var authService = new StarkBankAuthenticationService();

            var project = authService.GetProject();

            Assert.Null(project);
        }

        [Fact]
        public async Task InitializeAsync_CanBeCalledMultipleTimes()
        {
            const string privateKey1 = """
                                       -----BEGIN EC PRIVATE KEY-----
                                                       MHQCAQEEICBm4cS13ZF4Z8L3+Tzl/8NQb1Z9O9QwmlIEnowClOZ1oAcGBSuBBAAK
                                                       oUQDQgAE/qjF/BSI6E0t23qK4zy0YjkA2nbV+J2FBhjBTb8ENjB/Y4Oi3Zf0u2Pr
                                                       XgOx5nGZlO7B5M+6SJX4iFUkV+nSog==
                                                       -----END EC PRIVATE KEY-----
                                       """;
            const string environment1 = "sandbox";
            const string projectId1 = "proj-1";

            const string privateKey2 = """
                                       -----BEGIN EC PRIVATE KEY-----
                                                       MHQCAQEEICBm4cS13ZF4Z8L3+Tzl/8NQb1Z9O9QwmlIEnowClOZ1oAcGBSuBBAAK
                                                       oUQDQgAE/qjF/BSI6E0t23qK4zy0YjkA2nbV+J2FBhjBTb8ENjB/Y4Oi3Zf0u2Pr
                                                       XgOx5nGZlO7B5M+6SJX4iFUkV+nSog==
                                                       -----END EC PRIVATE KEY-----
                                       """;
            const string environment2 = "production";
            const string projectId2 = "proj-2";

            var authService = new StarkBankAuthenticationService();

            await authService.InitializeAsync(privateKey1, environment1, projectId1);
            var project1 = authService.GetProject();

            await authService.InitializeAsync(privateKey2, environment2, projectId2);
            var project2 = authService.GetProject();

            Assert.NotNull(project1);
            Assert.Equal(environment1, project1.Environment);
            Assert.Equal(projectId1, project1.ID);
            Assert.Equal(privateKey1, project1.Pem);

            Assert.NotNull(project2);
            Assert.Equal(environment2, project2.Environment);
            Assert.Equal(projectId2, project2.ID);
            Assert.Equal(privateKey2, project1.Pem);
        }

        [Fact]
        public async Task InitializeAsync_DoesNotThrowException_WithValidParameters()
        {
            const string privateKey = @"-----BEGIN EC PRIVATE KEY-----
                MHQCAQEEICBm4cS13ZF4Z8L3+Tzl/8NQb1Z9O9QwmlIEnowClOZ1oAcGBSuBBAAK
                oUQDQgAE/qjF/BSI6E0t23qK4zy0YjkA2nbV+J2FBhjBTb8ENjB/Y4Oi3Zf0u2Pr
                XgOx5nGZlO7B5M+6SJX4iFUkV+nSog==
                -----END EC PRIVATE KEY-----";
            const string environment = "sandbox";
            const string projectId = "proj-test";

            var authService = new StarkBankAuthenticationService();

            var exception = await Record.ExceptionAsync(() => authService.InitializeAsync(privateKey, environment, projectId));
            Assert.Null(exception);
        }

        [Fact]
        public async Task InitializeAsync_ThrowsException_WithInvalidParameters()
        {
            const string privateKey = "";
            const string environment = "";
            const string projectId = "";

            var authService = new StarkBankAuthenticationService();

            await Assert.ThrowsAsync<ArgumentException>(() => authService.InitializeAsync(privateKey, environment, projectId));
        }
    }
}
