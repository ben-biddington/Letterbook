using Bogus;
using Letterbook.Api.Dto;
using Letterbook.Web.Tests.E2E.Support;
using Letterbook.Web.Tests.E2E.Support.Letterbook;
using Newtonsoft.Json;

namespace Letterbook.Web.Tests.E2E;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class Posting : PageTest
{
	[Test]
	public async Task CanListTimeline()
	{
		var handle = new Faker<string>().CustomInstantiator(f => f.Internet.UserName()).Generate();
		var email = new Faker<string>().CustomInstantiator(f => f.Internet.Email()).Generate();

		var account = await Accounts.Signup(new RegistrationRequest
		{
			Handle = handle,
			Email = email,
			Password = "pAssword1#",
			ConfirmPassword = "pAssword1#"
		});

		Assert.That(account.Email, Is.EqualTo(email));
		Assert.That(account.IdentityToken.AccessToken, Is.Not.Empty);

		var profile = await Profiles.New(account, handle);
	}

	[Test]
	public void ParsingJson()
	{
		var json =
			"{\"access_token\":\"eyJhbG\",\"token_type\":\"Bearer\",\"expires_in\":2419199,\"refresh_token\":null,\"scope\":null}";

		Assert.That(JsonFormat.From<TokenResponse>(json).AccessToken, Is.EqualTo("eyJhbG"));
		Assert.That(JsonConvert.DeserializeObject<TokenResponse>(json).AccessToken, Is.Null);
	}
}