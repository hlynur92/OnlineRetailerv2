namespace SharedModels
{
    public interface IConverter<T,U>
    {
        T Convert(U model);
        U Convert(T model);
    }
}
