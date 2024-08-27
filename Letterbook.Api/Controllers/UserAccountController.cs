using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Letterbook.Api.Dto;
using Letterbook.Api.Swagger;
using Letterbook.Core;
using Letterbook.Core.Exceptions;
using Letterbook.Core.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Letterbook.Api.Controllers;

[Authorize(Policy = Constants.ApiPolicy)]
[ApiExplorerSettings(GroupName = Docs.LetterbookV1)]
[Route("/lb/v1/[controller]/[action]")]
public class UserAccountController : ControllerBase
{
	private readonly ILogger<UserAccountController> _logger;
	private readonly CoreOptions _coreOptions;
	private readonly string _hostSecret;
	private readonly IAccountService _accountService;

	public UserAccountController(ILogger<UserAccountController> logger, IConfiguration config, IOptions<CoreOptions> coreOptions, IAccountService accountService)
	{
		_logger = logger;
		_coreOptions = coreOptions.Value;
		_hostSecret = config.GetValue<string>("HostSecret")!;
		_accountService = accountService;
	}

	private static string MintToken(SecurityTokenDescriptor descriptor)
	{
		var handler = new JwtSecurityTokenHandler();

		return handler.WriteToken(handler.CreateToken(descriptor));
	}

	[AllowAnonymous]
	[HttpPost]
	[ProducesResponseType<TokenResponse>(StatusCodes.Status200OK)]
	public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
	{
		try
		{
			var identity = await _accountService.AuthenticatePassword(loginRequest.Email, loginRequest.Password);
			if (!identity.Authenticated) return Unauthorized();
			// TODO: 2FA

			return Ok(NewToken(identity));
		}
		catch (RateLimitException e)
		{
			return StatusCode(429, new { e.Expiration, e.Message });
		}
	}

	private TokenResponse NewToken(AccountIdentity identity)
	{
		// TODO: asymmetric signing key
		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_hostSecret));
		var tokenDescriptor = new SecurityTokenDescriptor
		{
			Subject = identity,
			Issuer = _coreOptions.BaseUri().ToString(),
			Audience = _coreOptions.BaseUri().ToString(),
			NotBefore = DateTime.UtcNow,
			Expires = DateTime.UtcNow.AddDays(28),
			SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
		};

		var token = MintToken(tokenDescriptor);

		return new TokenResponse
		{
			AccessToken = token,
			ExpiresIn = (int)(tokenDescriptor.Expires - DateTime.UtcNow).Value.TotalSeconds,
			TokenType = "Bearer"
		};
	}

	[HttpPost]
	[Authorize]
	public IActionResult Logout()
	{
		var controller = nameof(Logout);
		_logger.LogInformation("{Controller}", controller);

		return SignOut();
	}

	[AllowAnonymous]
	[HttpPost]
	[Route("/lb/v2/user_account/register")]
	public async Task<IActionResult> RegisterVersionTwo([FromBody] RegistrationRequest registration)
	{
		try
		{
			var (registrationResult, account) = await _accountService
				.RegisterAccount(registration.Email, registration.Handle, registration.Password);

			if (!registrationResult.Succeeded) return BadRequest(registrationResult.Errors);

			var identity = await _accountService.AuthenticatePassword(registration.Email, registration.Password);
			if (!identity.Authenticated) return Unauthorized();

			return Ok(new RegistrationResult { Token = NewToken(identity), AccountId = account.Id.ToString() });
		}
		catch (Exception e)
		{
			return BadRequest(e);
		}
	}

	[AllowAnonymous]
	[HttpPost]
	[ProducesResponseType<TokenResponse>(StatusCodes.Status200OK)]
	public async Task<IActionResult> Register([FromBody] RegistrationRequest registration)
	{
		try
		{
			var (registerAccount, _) = await _accountService
				.RegisterAccount(registration.Email, registration.Handle, registration.Password);

			if (registerAccount is null) return Forbid();
			if (!registerAccount.Succeeded) return BadRequest(registerAccount.Errors);

			return await Login(new LoginRequest { Email = registration.Email, Password = registration.Password });
		}
		catch (Exception e)
		{
			return BadRequest(e);
		}
	}
}