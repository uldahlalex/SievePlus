#nullable enable
using System;
using System.Linq;
using System.Reflection;

namespace Sieve.Plus.Services
{
    /// <summary>
    /// Use this interface to create SieveConfiguration (just like EntityTypeConfigurations are defined for EF)
    /// </summary>
    public interface ISievePlusConfiguration
    {
        /// <summary>
        ///  Configures sieve property mappings.
        /// </summary>
        /// <param name="mapper"> The mapper used to configure the sieve properties on. </param>
        void Configure(SievePlusPropertyMapper mapper);
    }

    /// <summary>
    /// Configuration extensions to the <see cref="SievePlusPropertyMapper" />
    /// </summary>
    public static class SieveConfigurationExtensions
    {
        /// <summary>
        ///     Applies configuration that is defined in an <see cref="ISievePlusConfiguration" /> instance.
        /// </summary>
        /// <param name="mapper"> The mapper to apply the configuration on. </param>
        /// <typeparam name="T">The configuration to be applied. </typeparam>
        /// <returns>
        ///     The same <see cref="SievePlusPropertyMapper" /> instance so that additional configuration calls can be chained.
        /// </returns>
        public static SievePlusPropertyMapper ApplyConfiguration<T>(this SievePlusPropertyMapper mapper) where T : ISievePlusConfiguration, new()
        {
            var configuration = new T();
            configuration.Configure(mapper);
            return mapper;
        }

        /// <summary>
        ///     Applies configuration from all <see cref="ISievePlusConfiguration" />
        ///     instances that are defined in provided assembly.
        /// </summary>
        /// <param name="mapper"> The mapper to apply the configuration on. </param>
        /// <param name="assembly"> The assembly to scan. </param>
        /// <returns>
        ///     The same <see cref="SievePlusPropertyMapper" /> instance so that additional configuration calls can be chained.
        /// </returns>
        public static SievePlusPropertyMapper ApplyConfigurationsFromAssembly(this SievePlusPropertyMapper mapper, Assembly assembly)
        {
            foreach (var type in assembly.GetTypes().Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition))
            {
                // Only accept types that contain a parameterless constructor, are not abstract.
                var noArgConstructor = type.GetConstructor(Type.EmptyTypes);
                if (noArgConstructor is null)
                {
                    continue;
                }

                if (type.GetInterfaces().Any(t => t == typeof(ISievePlusConfiguration)))
                {
                    var configuration = (ISievePlusConfiguration)noArgConstructor.Invoke(new object?[] { });
                    configuration.Configure(mapper);
                }
            }

            return mapper;
        }
    }
}
