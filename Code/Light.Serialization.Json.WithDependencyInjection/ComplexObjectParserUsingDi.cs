using System;
using Light.DependencyInjection;
using Light.DependencyInjection.TypeConstruction;
using Light.GuardClauses;
using Light.Serialization.Abstractions;
using Light.Serialization.Json.LowLevelReading;
using Light.Serialization.Json.ObjectMetadata;
using Light.Serialization.Json.TokenParsers;

namespace Light.Serialization.Json.WithDependencyInjection
{
    public sealed class ComplexObjectParserUsingDi : IJsonTokenParser
    {
        private readonly DependencyInjectionContainer _container;
        private readonly IObjectMetadataParser _metadataParser;

        public ComplexObjectParserUsingDi(DependencyInjectionContainer container, IObjectMetadataParser metadataParser)
        {
            container.MustNotBeNull(nameof(container));
            metadataParser.MustNotBeNull(nameof(metadataParser));

            _container = container;
            _metadataParser = metadataParser;
        }

        public bool CanBeCached => true;

        public bool IsSuitableFor(JsonDeserializationContext context)
        {
            return context.Token.JsonType == JsonTokenType.BeginOfArray;
        }

        public ParseResult ParseValue(JsonDeserializationContext context)
        {
            context.Token.JsonType.MustBe(JsonTokenType.BeginOfObject);

            var reader = context.JsonReader;
            var currentToken = reader.ReadNextToken();

            // If the first token is the end of the complex object, then there are no child values
            if (currentToken.JsonType == JsonTokenType.EndOfObject)
                return ParseResult.FromParsedValue(_container.Resolve(context.RequestedType));

            // Else parse the metadata section
            var metadataParseResult = _metadataParser.ParseMetadataSection(ref currentToken, context);
            // Check if this JSON object is a reference to another object that was deserialized before
            if (metadataParseResult.ReferencePreservationInfo.WasObjectRetrieved)
                return ParseResult.FromParsedValue(metadataParseResult.ReferencePreservationInfo.RetrievedObject);
            // Else check if this object is a deferred reference to another object
            if (metadataParseResult.ReferencePreservationInfo.IsDeferredReference)
                return ParseResult.FromDeferredReference(metadataParseResult.ReferencePreservationInfo.Id);

            // Check if the type to be constructed should actually be handled by another parser
            if (context.RequestedType != metadataParseResult.TypeToBeConstructed)
            {
                var otherParser = context.GetParserCorrespondingToType(metadataParseResult.TypeToBeConstructed);
                if (otherParser != null)
                    return otherParser.PerformSwitch(metadataParseResult, context, currentToken);
            }

            // Check if there is any data left to be deserialized
            if (currentToken.JsonType == JsonTokenType.EndOfObject)
                return ParseResult.FromParsedValue(_container.Resolve(metadataParseResult.TypeToBeConstructed));

            currentToken.MustBeComplexObjectKey();
            var dependencyOverrides = _container.OverrideDependenciesFor(metadataParseResult.TypeToBeConstructed);
            while (true)
            {
                var key = context.DeserializeToken<string>(currentToken);
                var targetDescription = FindTargetDependencyForChildValue(key, dependencyOverrides);

                currentToken = reader.ReadAndExpectPairDelimiterToken()
                                     .ReadNextToken();

                currentToken.MustBeBeginOfValue();
                var parseResult = context.DeserializeToken(currentToken, targetDescription.DependencyType);
                parseResult.IsDeferredReference.MustBeFalse(message: "ComplexObjectParserUsingDi does not support deferred references yet.");
                targetDescription.SetValue(ref dependencyOverrides, parseResult.ParsedValue);

                if (reader.ReadAndExpectEndOfObjectOrValueDelimiter() == JsonTokenType.EndOfObject)
                    break;
            }

            return ParseResult.FromParsedValue(_container.Resolve(metadataParseResult.TypeToBeConstructed, dependencyOverrides));
        }

        private static TargetDependency FindTargetDependencyForChildValue(string key, DependencyOverrides dependencyOverrides)
        {
            var instantiationDependencies = dependencyOverrides.TypeCreationInfo.InstantiationInfo.InstantiationDependencies;
            if (instantiationDependencies != null && instantiationDependencies.Count > 0)
            {
                for (var i = 0; i < instantiationDependencies.Count; i++)
                {
                    var instantiationDependency = instantiationDependencies[i];
                    if (string.Equals(instantiationDependency.TargetParameter.Name, key, StringComparison.OrdinalIgnoreCase))
                        return TargetDependency.FromInstantiationDependency(instantiationDependency);
                }
            }

            var instanceInjections = dependencyOverrides.TypeCreationInfo.InstanceInjections;
            if (instanceInjections != null && instanceInjections.Count > 0)
            {
                for (var i = 0; i < instanceInjections.Count; i++)
                {
                    var instanceInjection = instanceInjections[i];
                    if (string.Equals(instanceInjection.MemberName, key, StringComparison.OrdinalIgnoreCase))
                        return TargetDependency.FromInstanceInjection(instanceInjection);
                }
            }
            throw new DeserializationException($"There is no dependency with name {key} on type {dependencyOverrides.TypeCreationInfo.TargetType}. Cannot deserialize without losing information.");
        }


        private struct TargetDependency
        {
            public readonly string Name;
            public readonly Type DependencyType;
            private readonly SetValueDelegete _setValue;

            private TargetDependency(string name, Type dependencyType, SetValueDelegete setValue)
            {
                _setValue = setValue;
                DependencyType = dependencyType;
                Name = name;
            }

            public static TargetDependency FromInstantiationDependency(InstantiationDependency instantiationDependency)
            {
                return new TargetDependency(instantiationDependency.TargetParameter.Name,
                                            instantiationDependency.ParameterType,
                                            (string name, object value, ref DependencyOverrides overrides) => overrides.OverrideInstantiationParameter(name, value));
            }

            public void SetValue(ref DependencyOverrides overrides, object value)
            {
                _setValue(Name, value, ref overrides);
            }

            public static TargetDependency FromInstanceInjection(InstanceInjection instanceInjection)
            {
                return new TargetDependency(instanceInjection.MemberName,
                                            instanceInjection.MemberType,
                                            (string name, object value, ref DependencyOverrides overrides) => overrides.OverrideMember(name, value));
            }
        }

        private delegate void SetValueDelegete(string targetName, object value, ref DependencyOverrides dependencyOverrides);
    }
}