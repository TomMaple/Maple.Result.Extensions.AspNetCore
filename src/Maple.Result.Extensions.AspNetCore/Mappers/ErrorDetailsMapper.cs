namespace Maple.Result.Extensions.AspNetCore.Mappers;

internal static class ErrorDetailsMapper
{
    internal static ErrorDetail Map(Maple.Result.ErrorDetail source)
    {
        var detailTemplated = source.DetailTemplated is null
            ? null
            : TemplatedMessageMapper.Map(source.DetailTemplated);

        return new ErrorDetail(source.PropertyPointer, source.Detail, detailTemplated);
    }
}