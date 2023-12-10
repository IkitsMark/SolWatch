// 

using System.ComponentModel.DataAnnotations;

namespace SolarWatch.Contacts;

public record RegistrationRequest(
    [Required]string Email, 
    [Required]string Username, 
    [Required]string Password,
    [Required]string Role);