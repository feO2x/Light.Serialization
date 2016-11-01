using System;
using System.Collections.Generic;
using Light.DependencyInjection;
using Light.DependencyInjection.TypeConstruction;
using Light.GuardClauses;
using Light.Serialization.Abstractions;
using Light.Serialization.Json.ComplexTypeConstruction;
using Light.Serialization.Json.LowLevelReading;
using Light.Serialization.Json.ObjectMetadata;
using Light.Serialization.Json.TokenParsers;

namespace Light.Serialization.Json.WithDependencyInjection
{
    public sealed class ComplexObjectParserUsingDi : IJsonTokenParser
    {
        private readonly DependencyInjectionContainer _container;
        private readonly IObjectMetadataParser _metadataParser;
        private readonly ITypeDescriptionService _typeDescriptionService;

        public ComplexObjectParserUsingDi(DependencyInjectionContainer container, IObjectMetadataParser metadataParser, ITypeDescriptionService typeDescriptionService)
        {
            container.MustNotBeNull(nameof(container));
            metadataParser.MustNotBeNull(nameof(metadataParser));
            typeDescriptionService.MustNotBeNull(nameof(typeDescriptionService));

            _container = container;
            _metadataParser = metadataParser;
            _typeDescriptionService = typeDescriptionService;
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
            var searchTargetContext = new SearchTargetContext(_typeDescriptionService, dependencyOverrides);
            List<DeferredReferenceCandidate> deferredReferences = null;
            // Run through the remaining key-value pairs of the complex JSON object and deserialize them
            while (true)
            {
                var key = context.DeserializeToken<string>(currentToken);
                var targetDescription = searchTargetContext.FindTargetDependencyForChildValue(key);

                currentToken = reader.ReadAndExpectPairDelimiterToken()
                                     .ReadNextToken();

                currentToken.MustBeBeginOfValue();
                var parseResult = context.DeserializeToken(currentToken, targetDescription.DependencyType);
                if (parseResult.IsDeferredReference)
                {
                    var injectableValueDescription = searchTargetContext.TypeCreationDescription.GetInjectableValueDescriptionFromNormalizedName(_typeDescriptionService.NormalizeName(key));
                    if (injectableValueDescription.PropertyInfo == null && injectableValueDescription.FieldInfo == null)
                        throw new DeserializationException($"\"{key}\" is a deferred reference, but cannot be set via property or field injection.");
                    if (deferredReferences == null)
                        deferredReferences = new List<DeferredReferenceCandidate>();

                    deferredReferences.Add(new DeferredReferenceCandidate(injectableValueDescription, parseResult.ReferenceId));
                }
                else
                    targetDescription.SetValue(ref dependencyOverrides, parseResult.ParsedValue);

                if (reader.ReadAndExpectEndOfObjectOrValueDelimiter() == JsonTokenType.EndOfObject)
                    break;

                currentToken = reader.ReadNextToken();
                currentToken.MustBeComplexObjectKey();
            }

            // Create the object using the container
            var createdObject = _container.Resolve(metadataParseResult.TypeToBeConstructed, dependencyOverrides);

            // Add this object to the object reference preserver if there was a $id entry in the metadata section
            if (metadataParseResult.ReferencePreservationInfo.IsEmpty == false)
                context.ObjectReferencePreserver.AddDeserializedObject(metadataParseResult.ReferencePreservationInfo.Id, createdObject);

            // Register all deferred references with the Object Reference Preserver if necessary
            if (deferredReferences != null)
            {
                foreach (var deferredReference in deferredReferences)
                {
                    // Check if the object has been deserialized in the recursive algorithm until now
                    object retrievedValue;
                    if (context.ObjectReferencePreserver.TryGetDeserializedObject(deferredReference.ReferenceId, out retrievedValue))
                    {
                        deferredReference.InjectableValueDescription.SetPropertyOrField(createdObject, retrievedValue);
                        continue;
                    }

                    // If not, create a deferred reference
                    context.ObjectReferencePreserver.AddDeferredReference(new DeferredReferenceForObject(deferredReference.ReferenceId, deferredReference.InjectableValueDescription, createdObject));
                }
            }

            return ParseResult.FromParsedValue(createdObject);
        }

        private struct TargetDependency
        {
            private readonly string _name;
            public readonly Type DependencyType;
            private readonly SetValueDelegete _setValue;

            private TargetDependency(string name, Type dependencyType, SetValueDelegete setValue)
            {
                _setValue = setValue;
                DependencyType = dependencyType;
                _name = name;
            }

            public static TargetDependency FromInstantiationDependency(InstantiationDependency instantiationDependency)
            {
                return new TargetDependency(instantiationDependency.TargetParameter.Name,
                                            instantiationDependency.ParameterType,
                                            (string name, object value, ref DependencyOverrides overrides) => overrides.OverrideInstantiationParameter(name, value));
            }

            public static TargetDependency FromInstanceInjection(InstanceInjection instanceInjection)
            {
                return new TargetDependency(instanceInjection.MemberName,
                                            instanceInjection.MemberType,
                                            (string name, object value, ref DependencyOverrides overrides) => overrides.OverrideMember(name, value));
            }

            public static TargetDependency FromUnregisteredMember(string memberName, Type memberType)
            {
                return new TargetDependency(memberName,
                                            memberType,
                                            (string name, object value, ref DependencyOverrides overrides) => overrides.OverrideMember(name, value));
            }

            public void SetValue(ref DependencyOverrides overrides, object value)
            {
                _setValue(_name, value, ref overrides);
            }
        }

        private delegate void SetValueDelegete(string targetName, object value, ref DependencyOverrides dependencyOverrides);

        private struct SearchTargetContext
        {
            private readonly ITypeDescriptionService _typeDescriptionService;
            private readonly DependencyOverrides _dependencyOverrides;
            private TypeCreationDescription _typeCreationDescription;

            public SearchTargetContext(ITypeDescriptionService typeDescriptionService, DependencyOverrides dependencyOverrides)
            {
                _typeDescriptionService = typeDescriptionService;
                _dependencyOverrides = dependencyOverrides;
                _typeCreationDescription = null;
            }

            public TargetDependency FindTargetDependencyForChildValue(string key)
            {
                var instantiationDependencies = _dependencyOverrides.TypeCreationInfo.InstantiationInfo.InstantiationDependencies;
                if (instantiationDependencies != null && instantiationDependencies.Count > 0)
                {
                    for (var i = 0; i < instantiationDependencies.Count; i++)
                    {
                        var instantiationDependency = instantiationDependencies[i];
                        if (string.Equals(instantiationDependency.TargetParameter.Name, key, StringComparison.OrdinalIgnoreCase))
                            return TargetDependency.FromInstantiationDependency(instantiationDependency);
                    }
                }

                var instanceInjections = _dependencyOverrides.TypeCreationInfo.InstanceInjections;
                if (instanceInjections != null && instanceInjections.Count > 0)
                {
                    for (var i = 0; i < instanceInjections.Count; i++)
                    {
                        var instanceInjection = instanceInjections[i];
                        if (string.Equals(instanceInjection.MemberName, key, StringComparison.OrdinalIgnoreCase))
                            return TargetDependency.FromInstanceInjection(instanceInjection);
                    }
                }

                var injectableValueDescription = TypeCreationDescription.GetInjectableValueDescriptionFromNormalizedName(_typeDescriptionService.NormalizeName(key));
                if (injectableValueDescription != null)
                {
                    var memberName = injectableValueDescription.PropertyInfo?.Name ?? injectableValueDescription.FieldInfo?.Name;
                    if (memberName != null)
                        return TargetDependency.FromUnregisteredMember(memberName, injectableValueDescription.Type);
                }

                // TODO: we must be able to handle values that cannot be mapped to target types.
                throw new DeserializationException($"There is no dependency with name {key} on type {_dependencyOverrides.TypeCreationInfo.TargetType}. Cannot deserialize without losing information.");
            }

            public TypeCreationDescription TypeCreationDescription => _typeCreationDescription ?? (_typeCreationDescription = _typeDescriptionService.GetTypeCreationDescription(_dependencyOverrides.TypeCreationInfo.TargetType));
        }

        private struct DeferredReferenceCandidate
        {
            public readonly InjectableValueDescription InjectableValueDescription;
            public readonly int ReferenceId;

            public DeferredReferenceCandidate(InjectableValueDescription injectableValueDescription, int referenceId)
            {
                InjectableValueDescription = injectableValueDescription;
                ReferenceId = referenceId;
            }
        }
    }
}