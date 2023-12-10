using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using SolarWatch.Contacts;

namespace SolarIntegrationTest;

public class UnitTest1 : IDisposable
{
    private SolarWebFactory _factory;
    private HttpClient _client;
    public UnitTest1()
    {
        _factory = new SolarWebFactory();
        _client = _factory.CreateClient();
    }
    
    public void Dispose()
    {
        _factory.Dispose();
        _client.Dispose();
    }
    
    [Fact]
    public async Task TestRegistration()
    {
        // Arrange
        var client = _factory.CreateClient();
        var registerRequest = new RegistrationRequest("test@example.com", "testuser", "Test123!", "User");

        // Act
        var registerResponse = await client.PostAsync("/Auth/Register",
            new StringContent(JsonConvert.SerializeObject(registerRequest), Encoding.UTF8, "application/json"));
        registerResponse.EnsureSuccessStatusCode();

        var loginRequest = new AuthRequest("test@example.com", "Test123!");

        var loginResponse = await client.PostAsync("/Auth/Login",
            new StringContent(JsonConvert.SerializeObject(loginRequest), Encoding.UTF8, "application/json"));
        loginResponse.EnsureSuccessStatusCode();

        var authResponse =
            JsonConvert.DeserializeObject<AuthResponse>(await loginResponse.Content.ReadAsStringAsync());

        // Assert
        Assert.NotNull(authResponse.Token);
        Assert.Equal("test@example.com", authResponse.Email);
        Assert.Equal("testuser", authResponse.UserName);
    }
    
}