using System.Net;
using System.Net.Http.Headers;
using Letterbook.Core.Models.Dto;

namespace Letterbook.Web.Tests.E2E.Support.Letterbook;

public static class Profiles
{
	public static async Task<FullProfileDto?> New(Account account, string handle)
	{
		if (null == account.IdentityToken.AccessToken)
			throw new ArgumentException("The access token cannot be null");

		var requestUri = $"{Settings.BaseUrl}lb/v1/profiles/new/{account.Id}?handle={Uri.EscapeDataString(handle)}";

		var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri);

		httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", account.IdentityToken.AccessToken);

		var reply = await Network.HttpClient.SendAsync(httpRequestMessage);

		if (reply.StatusCode == HttpStatusCode.OK)
		{
			return JsonFormat.From<FullProfileDto>(await reply.Content.ReadAsStringAsync());
		}

		if (reply.StatusCode == HttpStatusCode.Found)
		{
			throw new Exception($"Unexpected redirect to <{reply.Headers.Location}>");
		}

		throw new Exception($"Post request to <{requestUri}> failed with status <{reply.StatusCode}> " +
		                    $"and message <{await reply.Content.ReadAsStringAsync()}>");
	}
}