namespace HrAspire.Web.Common;

using Google.Protobuf.WellKnownTypes;

public static class ProtobufTimestampExtensions
{
    public static DateOnly ToDateOnly(this Timestamp timestamp) => DateOnly.FromDateTime(timestamp.ToDateTime());

    public static Timestamp ToTimestamp(this DateOnly dateOnly) => dateOnly.ToDateTime(default, DateTimeKind.Utc).ToTimestamp();
}
