namespace Letterbook.Web.Tests.E2E.Support;

public static class Network
{
	public static readonly HttpClient HttpClient = new(new HttpClientHandler { AllowAutoRedirect = false });
}