using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Overview.Server.Infrastructure.Persistence.Converters;

internal static class JsonValueConverterExtensions
{
    public static PropertyBuilder<TValue> HasJsonbConversion<TValue>(this PropertyBuilder<TValue> propertyBuilder)
    {
        var converter = new ValueConverter<TValue, string>(
            value => ServerJsonSerializer.Serialize(value),
            json => ServerJsonSerializer.Deserialize<TValue>(json));

        var comparer = new ValueComparer<TValue>(
            (left, right) => ServerJsonSerializer.Serialize(left).Equals(ServerJsonSerializer.Serialize(right), StringComparison.Ordinal),
            value => ServerJsonSerializer.Serialize(value).GetHashCode(StringComparison.Ordinal),
            value => ServerJsonSerializer.Deserialize<TValue>(ServerJsonSerializer.Serialize(value)));

        propertyBuilder.HasConversion(converter);
        propertyBuilder.HasColumnType("jsonb");
        propertyBuilder.Metadata.SetValueComparer(comparer);

        return propertyBuilder;
    }
}
