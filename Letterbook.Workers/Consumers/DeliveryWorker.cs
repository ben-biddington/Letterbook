using Letterbook.Core.Adapters;
using Letterbook.Core.Models;
using Letterbook.Workers.Contracts;
using MassTransit;

namespace Letterbook.Workers.Consumers;

/// <summary>
/// Deliver ActivityPub messages to their destination
/// </summary>
public class DeliveryWorker : IConsumer<ActivityMessage>
{
	private readonly ILogger<DeliveryWorker> _logger;
	private readonly IAccountProfileAdapter _profiles;
	private readonly IActivityPubClient _client;

	public DeliveryWorker(ILogger<DeliveryWorker> logger, IAccountProfileAdapter profiles, IActivityPubClient client)
	{
		_logger = logger;
		_profiles = profiles;
		_client = client;
	}

	public async Task Consume(ConsumeContext<ActivityMessage> context)
	{
		Profile? profile = default;
		if (context.Message.OnBehalfOf is { } id)
			profile = await _profiles.LookupProfile(id);
		else
			_logger.LogInformation("Sending {Subject} anonymously to {Inbox}", context.Message.Subject, context.Message.Inbox);

		// TODO: side effects from response
		// like permanent redirects, for example
		var response = await _client.As(profile).SendDocument(context.Message.Inbox, context.Message.NextData);
		_logger.LogDebug("Delivered message {Type}, got response {@Response}", context.Message.Type, response);
	}
}