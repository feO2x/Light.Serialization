namespace Light.Serialization.Json.BuilderInterfaces
{
    public interface IGetSingletonInstance<out T>
    {
        T Instance { get; }
    }
}