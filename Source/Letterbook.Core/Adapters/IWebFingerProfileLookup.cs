using Letterbook.Core.Models;

namespace Letterbook.Core.Adapters;

public interface IWebFingerProfileLookup
{
	public Task<Profile?> LookupProfileByUri(Uri fediId, ProfileId? relatedProfile);
	public Task<Profile?> LookupProfileById(ProfileId profileId, ProfileId? relatedProfile);
}

/// <summary>
/// Implementation that always returns null meaning "I don't have it".
/// </summary>
public class DevNullWebFingerProfileLookup : IWebFingerProfileLookup
{
	public Task<Profile?> LookupProfileByUri(Uri fediId, ProfileId? relatedProfile)
	{
		return Task.FromResult<Profile?>(null);
	}

	public Task<Profile?> LookupProfileById(ProfileId profileId, ProfileId? relatedProfile)
	{
		return Task.FromResult<Profile?>(null);
	}
}