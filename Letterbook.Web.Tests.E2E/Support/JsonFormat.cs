namespace Letterbook.Web.Tests.E2E.Support;

public static class JsonFormat
{
	public static T? From<T>(string json)
	{
		return System.Text.Json.JsonSerializer.Deserialize<T>(json);
	}
}