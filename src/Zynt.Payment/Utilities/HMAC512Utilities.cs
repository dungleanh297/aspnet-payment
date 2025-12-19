using System.Buffers;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace Zynt.Payment.Utilities;

/// <summary>
/// Provides efficient methods for hashing and verifying data using HMAC-SHA512.
/// This class is designed to be used internally within the library for hashing operations.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class HMAC512Utilities
{
    public const int HMAC512Length = 64;
    public const int HMAC512HexadecimalLength = HMAC512Length * 2;
    private const string HexadecimalFormat = "x2";
    public static bool VerifyHashedData(in ReadOnlySpan<byte> key, in ReadOnlySpan<byte> data, in ReadOnlySpan<char> hexadecimalChecksum)
    {
        Span<byte> requestChecksum = stackalloc byte[HMAC512Length];
        Span<byte> calculatedChecksum = stackalloc byte[HMAC512Length];

        for (int i = 0; i < HMAC512Length; ++i)
        {
            if (!byte.TryParse(hexadecimalChecksum.Slice(i * 2, 2), NumberStyles.HexNumber, provider: null, out byte val))
            {
                return false;
            }
            requestChecksum[i] = val;
        }

        HMACSHA512.HashData(key, data, calculatedChecksum);

        return requestChecksum.SequenceEqual(calculatedChecksum);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void HashToHexadecimal(in ReadOnlySpan<byte> key, in ReadOnlySpan<char> data, in Span<char> destination)
        => HashToHexadecimal(key, data, destination, Encoding.ASCII);

    /// <summary>
    /// Hashes the provided data using HMAC-SHA512 and formats the result as a hexadecimal string.
    /// This method is intended for internal use within the library to ensure consistent hashing behavior.
    /// <param name="key">The key used for HMAC hashing.</param>
    /// <param name="data">The data to be hashed, provided as a span of characters.</param>
    /// <param name="destination">The span where the resulting hexadecimal string will be written.</param>
    /// <param name="encoding">The encoding to use for converting the character data to bytes.</param>
    /// <exception cref="ArgumentException">Thrown if the destination span is not large enough to hold the hexadecimal representation of the hash.</exception>
    /// </summary>
    public static void HashToHexadecimal(in ReadOnlySpan<byte> key, in ReadOnlySpan<char> data, in Span<char> destination, Encoding encoding)
    {
        if (destination.Length < HMAC512HexadecimalLength)
        {
            throw new ArgumentException($"Required length is {HMAC512HexadecimalLength}", nameof(destination));
        }

        byte[] parametersAsBytes = ArrayPool<byte>.Shared.Rent(data.Length);
        Span<byte> checksum = stackalloc byte[HMAC512Length];

        encoding.GetBytes(data, parametersAsBytes);

        ReadOnlySpan<char> format = HexadecimalFormat.AsSpan();

        HMACSHA512.HashData(key, parametersAsBytes, checksum);

        for (int i = 0; i < HMAC512Length; ++i)
        {
            checksum[i].TryFormat(destination.Slice(i * 2, 2), out var _, format);
        }
        
        ArrayPool<byte>.Shared.Return(parametersAsBytes);
    }
}
