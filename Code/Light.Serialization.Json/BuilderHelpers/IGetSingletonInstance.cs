namespace Light.Serialization.Json.BuilderHelpers
{
    public interface IGetSingletonInstance<out T>
    {
        T Instance { get; }
    }
}