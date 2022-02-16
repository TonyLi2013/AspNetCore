// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net.Http;
using System.Text.Json;
using Grpc.Core;
using IntegrationTestsWebsite;
using Microsoft.AspNetCore.Grpc.HttpApi.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Grpc.HttpApi.Tests.Infrastructure;
using Xunit.Abstractions;

namespace Microsoft.AspNetCore.Grpc.HttpApi.IntegrationTests;

public class UnaryTests : IntegrationTestBase
{
    public UnaryTests(GrpcTestFixture<Startup> fixture, ITestOutputHelper outputHelper)
        : base(fixture, outputHelper)
    {
    }

    [Fact]
    public async Task GetWithRouteParameter_MatchUrl_SuccessResult()
    {
        // Arrange
        Task<HelloReply> UnaryMethod(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply { Message = $"Hello {request.Name}!" });
        }
        var method = Fixture.DynamicGrpc.AddUnaryMethod<HelloRequest, HelloReply>(
            UnaryMethod,
            Greeter.Descriptor.FindMethodByName("SayHello"));

        var client = new HttpClient(Fixture.Handler) { BaseAddress = new Uri("http://localhost") };

        // Act
        var response = await client.GetAsync("/v1/greeter/test").DefaultTimeout();
        var responseStream = await response.Content.ReadAsStreamAsync();
        using var result = await JsonDocument.ParseAsync(responseStream);

        // Assert
        Assert.Equal("Hello test!", result.RootElement.GetProperty("message").GetString());
    }
}
