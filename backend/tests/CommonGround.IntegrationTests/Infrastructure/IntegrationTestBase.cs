namespace CommonGround.IntegrationTests.Infrastructure;

[Collection("Integration")]
public abstract class IntegrationTestBase
{
    protected HttpClient Client { get; }
    protected IntegrationTestFactory Factory { get; }

    protected IntegrationTestBase(IntegrationTestFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
    }
}

[CollectionDefinition("Integration")]
public sealed class IntegrationTestGroup : ICollectionFixture<IntegrationTestFactory> { }
