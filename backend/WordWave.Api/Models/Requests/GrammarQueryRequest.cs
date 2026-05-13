using System.ComponentModel.DataAnnotations;

namespace WordWave.Api.Models.Requests;

public sealed class GrammarQueryRequest
{
    [RegularExpression("^(A1|A2|B1|B2|C1)$", ErrorMessage = "Level must be one of: A1, A2, B1, B2, C1.")]
    public string? Level { get; init; }
}
