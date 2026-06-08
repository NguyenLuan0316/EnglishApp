using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace WordWave.Api.Models.Requests;

public sealed class IeltsAttemptRequest
{
    [MaxLength(120)]
    public string LearnerId { get; init; } = "default";

    [Required]
    public JsonElement StateData { get; init; }
}
