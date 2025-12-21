internal interface IWrapped<T> where T : class
{
    T Unwrap();
}