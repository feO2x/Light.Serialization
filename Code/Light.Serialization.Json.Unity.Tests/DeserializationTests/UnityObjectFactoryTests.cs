using System;
using System.Collections.Generic;
using Domain;
using FluentAssertions;
using Light.Serialization.Abstractions;
using Light.Serialization.Json.Unity;
using Microsoft.Practices.Unity;
using Xunit;

namespace Light.Serialization.UnityContainerIntegration.Tests
{
    public sealed class UnityObjectFactoryTests
    {
        [Fact(DisplayName = "The deserializer must be able to use the unity container to inject values into deserialized objects that are not contained within the JSON document.")]
        public void ResolveTest()
        {
            var diContainer = new UnityContainer().RegisterDefaultDeserializationTypes()
                                                  .UseDomainFriendlyNames(options => options.AllTypesFromAssemblies(typeof(AssemblyMarker))
                                                                                            .UseOnlyNamespaces(nameof(Domain)))
                                                  .RegisterType<Ship>(new InjectionFactory(c => new Ship(Guid.NewGuid())))
                                                  .RegisterType<IClock, UtcDateTimeClock>(new ContainerControlledLifetimeManager());

            var ship = diContainer.Resolve<Ship>();
            var deserializer = diContainer.Resolve<IDeserializer>();
            const string json = @"{
                                      ""$type"": ""AddCargoEvent"",
                                      ""cargoName"": ""Excellent beer from Regensburg""
                                  }";

            var @event = deserializer.Deserialize<IEvent<Ship>>(json);
            @event.Play(ship);

            ship.Cargo.Should().ContainSingle(cargoItem => cargoItem == "Excellent beer from Regensburg");
        }
    }
}

namespace Domain
{
    public static class AssemblyMarker { }

    public class Ship
    {
        private readonly List<string> _cargo = new List<string>();

        public Ship(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("The ID must not be empty.");

            Id = id;
        }

        public Guid Id { get; }

        public IReadOnlyList<string> Cargo => _cargo;

        public void AddCargo(string cargoName)
        {
            if (string.IsNullOrWhiteSpace(cargoName))
                throw new ArgumentException($"{nameof(cargoName)} must be a non-empty string.");
            if (_cargo.Count == 4)
                throw new InvalidOperationException("The ship is already full");

            _cargo.Add(cargoName);
        }

        public void RemoveCargo(string cargoName)
        {
            if (_cargo.Contains(cargoName) == false)
                throw new ArgumentException($"This ship does not contain cargo with the name {cargoName}.");

            _cargo.Remove(cargoName);
        }
    }

    public class AddCargoEvent : IEvent<Ship>
    {
        private readonly string _cargoName;
        private readonly IClock _clock;
        private DateTime? _executedOn;

        public AddCargoEvent(string cargoName, IClock clock)
        {
            if (cargoName == null) throw new ArgumentNullException(nameof(cargoName));
            if (clock == null) throw new ArgumentNullException(nameof(clock));

            _cargoName = cargoName;
            _clock = clock;
        }

        public bool HasExecuted => _executedOn.HasValue;
        public DateTime? ExecutedOn => _executedOn;

        public void Play(Ship ship)
        {
            if (_executedOn == null)
                _executedOn = _clock.GetTime();

            ship.AddCargo(_cargoName);
        }
    }

    public interface IEvent<in TAggregateRoot>
    {
        void Play(TAggregateRoot aggregateRoot);
    }

    public interface IClock
    {
        DateTime GetTime();
    }

    public sealed class UtcDateTimeClock : IClock
    {
        public DateTime GetTime()
        {
            return DateTime.UtcNow;
        }
    }
}