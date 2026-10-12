using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Validation;

namespace Ticketing.Web.Components.Tickets;

[ValidatableType]
public sealed class RequesterFormModel
{
    [Required(ErrorMessage = "Requester name is required.")]
    [StringLength(100, ErrorMessage = "Requester name cannot exceed 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Requester email is required.")]
    [StringLength(200, ErrorMessage = "Requester email cannot exceed 200 characters.")]
    [EmailAddress(ErrorMessage = "Requester email must be a valid email address.")]
    public string Email { get; set; } = string.Empty;
}
