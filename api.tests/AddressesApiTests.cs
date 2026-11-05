using System.Net;
using System.Net.Http.Json;
using Addresses.Api.Controllers;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Addresses.Tests;

public class AddressesApiTests
{
    // Each test gets its own in-memory database with the sample data.
    private static HttpClient CreateClient() =>
        new WebApplicationFactory<Program>()
            .WithWebHostBuilder(b => b.UseSetting("DatabaseName", Guid.NewGuid().ToString()))
            .CreateClient();

    [Fact]
    public async Task GetAddresses_ReturnsTheCustomersAddresses()
    {
        var client = CreateClient();

        var addresses = await client.GetFromJsonAsync<List<AddressDto>>("/api/addresses");

        Assert.NotNull(addresses);
        Assert.Equal(new[] { 1, 2, 3 }, addresses.Select(a => a.Id));
    }

    [Fact]
    public async Task SetDefault_MarksTheAddressAsDefault()
    {
        var client = CreateClient();

        var response = await client.PutAsync("/api/addresses/2/default", null);
        var addresses = await client.GetFromJsonAsync<List<AddressDto>>("/api/addresses");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(2, addresses![0].Id);
        Assert.True(addresses[0].IsDefault);
        Assert.Single(addresses, a => a.IsDefault);
    }

    [Fact]
    public async Task SetDefault_CannotChangeAnotherCustomersAddress()
    {
        var client = CreateClient();

        var response = await client.PutAsync("/api/addresses/4/default", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        client.DefaultRequestHeaders.Add("X-Customer-Id", "2");
        var otherCustomersAddresses = await client.GetFromJsonAsync<List<AddressDto>>("/api/addresses");
        Assert.True(otherCustomersAddresses!.Single(a => a.Id == 4).IsDefault);
        Assert.Single(otherCustomersAddresses!, a => a.IsDefault);
    }

    [Fact]
    public async Task DeleteAddress_CannotDeleteAnotherCustomersAddress()
    {
        var client = CreateClient();

        var response = await client.DeleteAsync("/api/addresses/4");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        client.DefaultRequestHeaders.Add("X-Customer-Id", "2");
        var otherCustomersAddresses = await client.GetFromJsonAsync<List<AddressDto>>("/api/addresses");
        Assert.Contains(otherCustomersAddresses!, a => a.Id == 4);
    }
}
