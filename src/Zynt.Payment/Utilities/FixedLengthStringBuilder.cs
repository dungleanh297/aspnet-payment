using System.Buffers;
using System.ComponentModel;

namespace Zynt.Payment.Utilities;

/// <summary>
/// A fixed-length string builder that can be used to build strings with a maximum length.
/// This is intended to be used for internal library dependencies only. Should not be used outside of the library.
/// It allows for efficient string building without the overhead of dynamic memory allocation, useful when the length of string is known in advance.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public ref struct FixedLengthStringBuilder
{
    public const int MaximumStackCharacterBufferSize = 256;

    private readonly Span<char> _charactersSpan;
    private readonly char[]? _rentedArrayByte;
    private int _length = 0;

    public int Length
    {
        get => _length;

        set
        {
            if (value > _charactersSpan.Length)
            {
                throw new ArgumentOutOfRangeException($"Length is too large. Initial length of this FixedLengthStringBuilder is {_charactersSpan.Length}");
            }
            _length = value;
        }
    }

    public FixedLengthStringBuilder(Span<char> characters)
    {
        _charactersSpan = characters;
    }

    public FixedLengthStringBuilder(int length)
    {
        _rentedArrayByte = ArrayPool<char>.Shared.Rent(length);
        _charactersSpan = _rentedArrayByte.AsSpan();
    }

    public void Append(string? str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return;
        }

        str.CopyTo(_charactersSpan.Slice(Length));
        Length += str.Length;
    }

    public void Append(char ch)
    {
        _charactersSpan[_length] = ch;
        Length += 1;
    }

    public void Append<T>(T value, string? format = null) where T : ISpanFormattable
    {
        if (value.TryFormat(_charactersSpan.Slice(_length), out int charWritten, format is null ? default : format.AsSpan(), null))
        {
            Length += charWritten;
            return;
        }

        // Fallback to Append(string str)
        Append(value.ToString());
    }

    public Span<char> AsSpan(int startIndex, int length)
    {
        return _charactersSpan.Slice(startIndex, length);
    }

    public override string ToString()
    {
        var result = new string(_charactersSpan);
        Dispose();
        return result;
    }

    public void Dispose()
    {
        if (_rentedArrayByte is not null)
        {
            ArrayPool<char>.Shared.Return(_rentedArrayByte);
        }
    }
}
