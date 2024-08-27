using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Letterbook.Api.Dto;

namespace Letterbook.Web.Tests.E2E.Support.Letterbook;

public class Account
{
	public string Email { get; set; }
	public string Id { get; set; }
	public TokenResponse IdentityToken { get; set; }
}

public class SignupResponse {
	[JsonPropertyName("token")]
	public TokenResponse Token { get; set; }
	[JsonPropertyName("accountId")]
	public string AccountId { get; set; }
}

public static class Accounts
{
	public static async Task<Account> Signup(RegistrationRequest signup)
	{
		var requestUri = $"{Settings.BaseUrl}lb/v2/user_account/register";

		var reply = await Network.HttpClient.PostAsync(
			requestUri,
			JsonContent.Create(signup));

		var body = await reply.Content.ReadAsStringAsync();

		if (reply.StatusCode == HttpStatusCode.OK)
		{
			var signupResponse = JsonFormat.From<SignupResponse>(body);
			return new Account
			{
				Email = signup.Email,
				Id = signupResponse.AccountId,
				IdentityToken = signupResponse.Token
			};
		}

		throw new Exception($"Post request to <{requestUri}> failed with status <{reply.StatusCode}> " +
		                    $"and message <{body}>");
	}
}