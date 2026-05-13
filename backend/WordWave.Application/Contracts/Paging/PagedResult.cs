namespace WordWave.Application.Contracts.Paging;

public sealed record PagedResult<T>(int Total, int Page, int Limit, IReadOnlyList<T> Data);
