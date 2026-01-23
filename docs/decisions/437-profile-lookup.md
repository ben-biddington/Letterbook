# Profile lookup

Lookup profiles by handle. Check the local data set and fallback to webfinger if we don't have it.

https://github.com/Letterbook/Letterbook/issues/437

* What does the web request look like
* What does it mean when a profile is found?
* How does the fallback to webfinger work?

## What does the web request look like?

It looks like this is taken care of in `Letterbook.IntegrationTests.ProfileTests`:

```shell
/lb/v1/profiles/{profileId}
```

### Letterbook.Web.Tests.ProfileControllerTests (display rendering)

This one exercises the `Profile` page model which is used by `Source/Letterbook.Web/Pages/Profile.cshtml`.

So these tests are about display rendering of the profile screen.

### Letterbook.IntegrationTests.ProfileTests

These exercise with network requests against in-memory hosting. Dependencies faked.

```csharp
[Fact(DisplayName = "Should get a profile by ID")]
public async Task CanGetProfile()
{
    var response = await _client.GetAsync($"/lb/v1/profiles/{_profiles[0].GetId25()}");

    Assert.NotNull(response);
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var actual = Assert.IsType<FullProfileDto>(await response.Content.ReadFromJsonAsync<FullProfileDto>(_json));
    Assert.Equal(_profiles[0].Handle, actual.Handle);
}
```

### Letterbook.Api.Tests.ProfilesControllerTests

This one exercises the controller directly.

```csharp
// Tests/Letterbook.Web.Tests/ProfileControllerTests.cs

// ...

public ProfilesControllerTests(ITestOutputHelper output)
{
    _output = output;
    _output.WriteLine($"Bogus seed: {Init.WithSeed()}");
    _accountId = new Faker().Random.Guid();
    Auth(_accountId);
    _controller = new ProfilesController(Mock.Of<ILogger<ProfilesController>>(), CoreOptionsMock, ProfileServiceMock.Object,
        new MappingConfigProvider(CoreOptionsMock), AuthorizationServiceMock.Object)
    {
        ControllerContext = new ControllerContext()
        {
            HttpContext = MockHttpContext.Object
        }
    };

    _fakeProfile = new FakeProfile(CoreOptionsMock.Value.BaseUri().Authority);
    _profile = _fakeProfile.Generate();
}

[Fact(DisplayName = "Should get a profile by ID")]
public async Task CanGetProfile()
{
    ProfileServiceAuthMock.Setup(m => m.LookupProfile(_profile.Id, (Models.ProfileId?)It.IsAny<Models.ProfileId?>())).ReturnsAsync(_profile);

    var result = await _controller.Get(_profile.GetId());

    var response = Assert.IsType<OkObjectResult>(result);
    var actual = Assert.IsType<FullProfileDto>(response.Value);
    Assert.Equal(_profile.Handle, actual.Handle);
}
```

## What does the behaviour look like?

It makes sense to me that we would like to push the fallback behaviour into somewhere core.

> Check the local data set and fallback to webfinger if we don't have it.

So I would try and get it into `Letterbook.Core.ProfileService.LookupProfile`. 

```csharp
public async Task<Profile?> LookupProfile(ProfileId profileId, ProfileId? relatedProfile)
{
    var result = await _data.Profiles(profileId).WithRelation(relatedProfile).FirstOrDefaultAsync();
    if (result?.FollowersCollection.FirstOrDefault() is {} relation && relation.State == FollowState.Blocked)
    {
        return Redact(result);
    }

    return result;
}
```

The tests go in `Letterbook.Core.Tests.ProfileServiceTests`.

## Log

### Introduce webfinger as first-class idea

I am unsure how to represent it yet, so I have added it as new standalone astraction:

```csharp
public interface IWebFingerProfileLookup
{
	public Task<Profile?> LookupProfileByUri(Uri fediId, ProfileId? relatedProfile);
	public Task<Profile?> LookupProfileById(ProfileId profileId, ProfileId? relatedProfile);
}
```

And then bound `ProfileService` to it.

All unit tests pass even though I have not threaded it into the real dependencies -- expect integration tests to fail.

I know that there are some other webfinger things floating around, so I'd like to figure out whether they can/should be coalesced.

Yes, this failed well:

```shell
./scripts/integration-test.sh

Unable to resolve service for type 'Letterbook.Core.Adapters.IWebFingerProfileLookup' while attempting to activate 'Letterbook.Core.ProfileService'.
```

First step is to make one that does nothing.

We need to add it to the real program (`Source/Letterbook.Api/Program.cs) as well as places like `HostFixture` that do configuration as prt of tests.

`HostFixture.ConfigureWebHost` does a `ConfigureServices`. Is that overwriting what we have already in `Program.cs`?

`Letterbook.Api.Program` does call `builder.ConfigureHostBuilder()`, which hits the dependency injection, but `Letterbook.Program` does not.

To get the integration tests to pass I need to do this:

```diff
$ git diff  Source/Letterbook/Program.cs
diff --git a/Source/Letterbook/Program.cs b/Source/Letterbook/Program.cs
index 9fed613..5eb9d9d 100644
--- a/Source/Letterbook/Program.cs
+++ b/Source/Letterbook/Program.cs
@@ -7,6 +7,7 @@ using Letterbook.Api.Authentication.HttpSignature.DependencyInjection;
 using Letterbook.Api.Swagger;
 using Letterbook.AspNet;
 using Letterbook.Core;
+using Letterbook.Core.Adapters;
 using Letterbook.Core.Extensions;
 using Letterbook.Core.Models;
 using Letterbook.Web;
@@ -79,6 +80,8 @@ public class Program
                builder.Services.AddMassTransit(bus => bus.AddWorkerBus(builder.Configuration)
                        .AddWorkers(builder.Configuration));

+               builder.Services.AddSingleton<IWebFingerProfileLookup, DevNullWebFingerProfileLookup>();
+
                // builder.WebHost.UseUrls(coreOptions.BaseUri().ToString());

                var app = builder.Build();
```

We do really need this one for the API as well because it references the same `IProfileService`. (Even though no tests fail without it.)

```diff
AzureAD+BenBiddington@TIM-6TYPBD4 MINGW64 ~/sauce/Letterbook (437-profile-lookup)
$ git diff Source/Letterbook.Api/DependencyInjectionExtensions.cs
diff --git a/Source/Letterbook.Api/DependencyInjectionExtensions.cs b/Source/Letterbook.Api/DependencyInjectionExtensions.cs
index 1761c30..527cd4c 100644
--- a/Source/Letterbook.Api/DependencyInjectionExtensions.cs
+++ b/Source/Letterbook.Api/DependencyInjectionExtensions.cs
@@ -116,6 +116,7 @@ public static class DependencyInjectionExtensions
                // Register Adapters
                services.AddSingleton<IActivityPubDocument, Document>();
                services.AddDbAdapter(configuration);
+               services.AddSingleton<IWebFingerProfileLookup, DevNullWebFingerProfileLookup>();
                services.AddFeedsAdapter(configuration);
                services.TryAddTypesModule();


```

### Try and implementation of IWebFingerProfileLookup

We already have `WebFingerClient` which knows how to search for profiles.

Its protocol takes text queries:

```csharp
public Task<IEnumerable<Profile>> SearchProfiles(string query, CancellationToken cancellationToken);
```

So we have to translate from ids to text.

(PC is freezing at times and when I check CPU it reads 0.)

### Webfinger implementation problems

These works:

```
curl -vk https://mastodon.social/.well-known/webfinger?resource=acct%3Aben@mastodon.social
curl -vk https://mastodon.social/.well-known/webfinger?resource=acct%3Aben%40mastodon.social
```

Based on [the notes](https://docs.joinmastodon.org/spec/webfinger/).

So it needs the full `acct` part.

This test fails:

```csharp
// https://docs.joinmastodon.org/spec/webfinger/
[Fact(DisplayName = "Should parse query into correct URL")]
public async Task ParsesQueryIntoUrl()
{
    HttpMessageHandlerMock.SetupResponse(m =>
    {
        m.StatusCode = HttpStatusCode.OK;
        m.Content = new StringContent("{}", new UTF8Encoding(), new MediaTypeHeaderValue("application/jrd+json"));
    });

    ActivityPubClientMock.Setup(m => m.Fetch<Models.Profile>(_profile.FediId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(_profile);

    await _webfinger.SearchProfiles($"@{_profile.Handle}@{_profile.FediId.Authority}", _cancel.Token);

    HttpMessageHandlerMock.Verify(
        it => it.SendMessageAsync(It.Is<HttpRequestMessage>(
            message => message.RequestUri == new Uri($"https://{_profile.FediId.Authority}/.well-known/webfinger?resource=acct%3A${_profile.Handle}@{_profile.FediId.Authority}")
    ), It.IsAny<CancellationToken>()));
}
```

```
Performed invocations:

   Mock<MockableMessageHandler:1> (it):

      MockableMessageHandler.SendMessageAsync(Method: GET, RequestUri: 'https://peer.example/.well-known/webfinger?resource=acct%3AFaustino.Treutel64', Version: 1.1, Content: <null>, Headers:
{
}, CancellationToken)
```

So I am curious about that.

### WebFingerClient and IActivityPubClient

In practice `WebFingerClient` depends on `ActivityPubClient` which makes network calls to fetch data.

In order to skewer `WebFingerClient` you have to either fake it or provide a real one which is hard to do because the ctor is like this:

```csharp
public Client(ILogger<Client> logger, HttpClient httpClient, IJsonLdSerializer jsonLdSerializer, IActivityPubDocument document)
{
    _logger = logger;
    _httpClient = httpClient;
    _jsonLdSerializer = jsonLdSerializer;
    _document = document;
}
```

Providing a fake means you can't make a real integration test that for example queries for a known profile.

Creating one is complected by `JsonLdSerializer` which is very difficult to construct. It seems it is tightly bound to
dependency injection framework.

# Notes

## docs/IntegrationTests.Readme.md is misleading

The solution file does not exist?

```shell
$ find . -name Letterbook.IntegrationTests.sln
```

```shell
$ find . -name *.sln
./Letterbook.sln
```