using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.SyndicationFeed.ReaderWriter.Tests.Extentions;

internal static class SerializationExtensions
{
  public static string Serialize<T>(this T value, bool writeIndented = false)
  {
    if (value == null)
    {
      return null;
    }

    var options = new JsonSerializerOptions { WriteIndented = writeIndented, IncludeFields = true };
    return value.Serialize(options);
  }

  public static string Serialize<T>(this T value, JsonSerializerOptions options)
  {
    if (value == null)
    {
      return null;
    }

    return JsonSerializer.Serialize(value, options);
  }

  public static T DeserializeAnonymousType<T>(this string json, T anonymousTypeObject, JsonSerializerOptions options = default)
    => JsonSerializer.Deserialize<T>(json, options);

  public static ValueTask<TValue> DeserializeAnonymousTypeAsync<TValue>(this Stream stream, TValue anonymousTypeObject, JsonSerializerOptions options = default, CancellationToken cancellationToken = default)
    => JsonSerializer.DeserializeAsync<TValue>(stream, options, cancellationToken); // Method to deserialize from a stream added for completeness

}