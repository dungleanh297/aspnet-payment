/// <summary>
/// A decorating type that can be unwrapped
/// </summary>
/// <typeparam name="T">Decorated type</typeparam>

internal interface IDecorating<T> where T : class
{
    /// <summary>
    /// Get the decorated object inside decoration type
    /// </summary>
    /// <returns>Original object that been wrapped inside</returns>
    T Unwrap();
}