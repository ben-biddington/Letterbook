using ActivityPub.Types.Conversion;
using Letterbook.Core.Adapters;
using Letterbook.Core.Tests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace Letterbook.Adapter.ActivityPub.Test;

public class WebFingerProfileLookupTests
{
	[Fact]
	public async Task CanFindProfiles()
	{
		/*

			JsonLdSerializer makes Letterbook.Adapter.ActivityPub.Client very difficult to construct and so using a fake here which does not
			feel great.

			Now we have a test which is part integration and part unit.

			All we're trying to do is verify that `LookupProfileByUri` works for real.

		*/
		var activityPubClient = new Mock<IActivityPubClient>();

		activityPubClient.Setup(it => it.Fetch<Models.Profile>(It.IsAny<Uri>(), It.IsAny<CancellationToken>()))
			.Returns(Task.FromResult(Models.Profile.CreateIndividual(new Uri("https://mastodon.social"), "ben")));

		var webFingerProfileLookup = new WebFingerProfileLookup(
			new NullLogger<WebFingerClient>(),
			new HttpClient(),
			activityPubClient.Object);

		var result = await webFingerProfileLookup.LookupProfileByUri(new Uri("https://mastodon.social/@ben"), null);

		Assert.Equal("ben", result?.Handle);
	}
}