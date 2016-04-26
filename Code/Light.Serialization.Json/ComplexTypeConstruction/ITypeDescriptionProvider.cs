using System;

namespace Light.Serialization.Json.ComplexTypeConstruction
{
    public interface ITypeDescriptionProvider
    {
        TypeCreationDescription GetTypeCreationDescription(Type typeToAnalyze);
    }
}