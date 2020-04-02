using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using API.Data.Interfaces;
using API.Dtos.Auth;
using API.Models;
using API.Services;
using API.Services.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Tests.ServiceTests
{
    public class AuthServiceTests
    {
        private readonly IAuthService _authService;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<IConfiguration> _config;
        private readonly Mock<IAuthRepository> _repo;
        private readonly Mock<HttpClient> _httpClient;
        public AuthServiceTests()
        {
            _mapper = new Mock<IMapper>();
            _config = new Mock<IConfiguration>();
            _repo = new Mock<IAuthRepository>();
            _httpClient = new Mock<HttpClient>();
            _authService = new AuthService(_mapper.Object, _config.Object, _repo.Object, _httpClient.Object);
        }

        [Fact]
        public async Task CreateJWTForClient_GivenJsonString_ReturnsJWTAsync()
        {
            string email = "a@b.com";
            int id = 1226;
            string tokenSource = "AppSettings:Token";
            string issuerSource = "AppSettings:Issuer";
            string key = "Super Secret Key";
            string issuer = "excelmec.org";
            UserFromAuth0Dto userFromAuth0 = Mock.Of<UserFromAuth0Dto>(x => x.email == email);
            string responseFromAuth0 = JsonSerializer.Serialize(userFromAuth0);
            _repo.Setup(x => x.UserExists(email)).ReturnsAsync(true);
            User user = Mock.Of<User>(x => x.Email == email && x.Id == id);
            _repo.Setup(x => x.GetUser(email)).ReturnsAsync(user);
            _config.Setup(x => x.GetSection(tokenSource).Value).Returns(key);
            _config.Setup(x => x.GetSection(issuerSource).Value).Returns(issuer);
            var jwt = await _authService.CreateJwtForClient(responseFromAuth0);
            var validatedEmail = JwtValidator.Validate(jwt, key, issuer);
            Assert.IsType<string>(jwt);
            Assert.Equal(email, validatedEmail);
        }
    }
}