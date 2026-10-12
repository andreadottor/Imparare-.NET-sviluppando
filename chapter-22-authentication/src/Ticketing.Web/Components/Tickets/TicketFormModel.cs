using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Validation;

namespace Ticketing.Web.Components.Tickets;

[ValidatableType]
public sealed class TicketFormModel
{
    [Required(ErrorMessage = "Code is required.")]
    [StringLength(30, ErrorMessage = "Code cannot exceed 30 characters.")]
    [RegularExpression(@"\A(?!\.{1,2}\z)[A-Za-z0-9.-]+\z")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    public string Description { get; set; } = string.Empty;

    [Range(1, 3, ErrorMessage = "Priority must be between 1 and 3.")]
    public int Priority { get; set; } = 2;

    [Required(ErrorMessage = "Affected service is required.")]
    [StringLength(
        100,
        ErrorMessage = "Affected service cannot exceed 100 characters.")]
    public string AffectedService { get; set; } = string.Empty;
}