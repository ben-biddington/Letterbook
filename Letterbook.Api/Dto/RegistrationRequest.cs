using System.ComponentModel.DataAnnotations;
using Letterbook.Core.Models;
using Microsoft.AspNetCore.Identity;

namespace Letterbook.Api.Dto;

public class RegistrationRequest
{
	public required string Handle { get; set; }
	[EmailAddress] public required string Email { get; set; }
	public required string Password { get; set; }
	[Compare(nameof(Password))] public required string ConfirmPassword { get; set; }
}

public class RegistrationResult
{
	public TokenResponse? Token { get; set; }
	public string? AccountId { get; set; }
}