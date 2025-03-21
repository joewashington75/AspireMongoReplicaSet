using Shouldly;

namespace AspireMongoReplicaSet.API.IntegrationTests;

public class EndPointTests(ApiFixture fixture) : IClassFixture<ApiFixture>
{
    private readonly HttpClient _httpClient = fixture.CreateClient();
    private const string BaseUrl = "/Test";

    [Fact]
    public async Task GivenCallingTest_WhenCallingMultipleTimes_ShouldReturnCount()
    {
        var response = await CreateRequestAsync();
        response.ShouldBe("1");
        response = await CreateRequestAsync();
        response.ShouldBe("2");
    }


    private async Task<string> CreateRequestAsync()
    {
        var response = await _httpClient.PostAsync(BaseUrl, null);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}