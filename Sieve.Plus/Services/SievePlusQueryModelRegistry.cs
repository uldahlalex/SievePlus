using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sieve.Plus.Models;

namespace Sieve.Plus.Services
{
    /// <summary>
    /// Registry that holds all query model configurations.
    /// Similar to how EF Core's ModelBuilder works, this centralizes all query model mappings.
    /// </summary>
    public class SievePlusQueryModelRegistry
    {
        private readonly Dictionary<(Type queryModelType, Type entityType), object> _mappers =
            new Dictionary<(Type, Type), object>();

        /// <summary>
        /// Add a specific configuration for a query model.
        /// </summary>
        /// <typeparam name="TConfiguration">The configuration class implementing ISievePlusQueryConfiguration</typeparam>
        public SievePlusQueryModelRegistry AddConfiguration<TConfiguration>()
            where TConfiguration : new()
        {
            var configurationType = typeof(TConfiguration);
            var interfaceType = configurationType.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType &&
                                   i.GetGenericTypeDefinition() == typeof(ISievePlusQueryConfiguration<,>));

            if (interfaceType == null)
            {
                throw new ArgumentException(
                    $"Type {configurationType.Name} does not implement ISievePlusQueryConfiguration<,>");
            }

            var queryModelType = interfaceType.GetGenericArguments()[0];
            var entityType = interfaceType.GetGenericArguments()[1];

            // Create the mapper
            var mapperType = typeof(SievePlusQueryMapper<,>).MakeGenericType(queryModelType, entityType);
            var mapper = Activator.CreateInstance(mapperType);

            // Create and invoke the configuration
            var configuration = new TConfiguration();
            var configureMethod = interfaceType.GetMethod("Configure");
            configureMethod?.Invoke(configuration, new[] { mapper });

            // Store the mapper
            _mappers[(queryModelType, entityType)] = mapper;

            return this;
        }

        /// <summary>
        /// Scan an assembly and add all configurations found.
        /// </summary>
        /// <param name="assembly">The assembly to scan for ISievePlusQueryConfiguration implementations</param>
        public SievePlusQueryModelRegistry AddConfigurationsFromAssembly(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes()
                .Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition && !t.IsInterface))
            {
                // Check if type implements ISievePlusQueryConfiguration<,>
                var interfaceType = type.GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType &&
                                       i.GetGenericTypeDefinition() == typeof(ISievePlusQueryConfiguration<,>));

                if (interfaceType == null)
                    continue;

                // Only accept types with parameterless constructor
                var constructor = type.GetConstructor(Type.EmptyTypes);
                if (constructor == null)
                    continue;

                var queryModelType = interfaceType.GetGenericArguments()[0];
                var entityType = interfaceType.GetGenericArguments()[1];

                // Create the mapper
                var mapperType = typeof(SievePlusQueryMapper<,>).MakeGenericType(queryModelType, entityType);
                var mapper = Activator.CreateInstance(mapperType);

                // Create and invoke the configuration
                var configuration = constructor.Invoke(null);
                var configureMethod = interfaceType.GetMethod("Configure");
                configureMethod?.Invoke(configuration, new[] { mapper });

                // Store the mapper
                _mappers[(queryModelType, entityType)] = mapper;
            }

            return this;
        }

        /// <summary>
        /// Get the mapper for a specific query model and entity type.
        /// </summary>
        internal SievePlusQueryMapper<TQueryModel, TEntity> GetMapper<TQueryModel, TEntity>()
            where TQueryModel : ISievePlusQueryModel
        {
            var key = (typeof(TQueryModel), typeof(TEntity));
            if (_mappers.TryGetValue(key, out var mapper))
            {
                return mapper as SievePlusQueryMapper<TQueryModel, TEntity>;
            }

            return null;
        }

        /// <summary>
        /// Check if a mapper exists for the given query model and entity type.
        /// </summary>
        internal bool HasMapper<TQueryModel, TEntity>()
            where TQueryModel : ISievePlusQueryModel
        {
            return _mappers.ContainsKey((typeof(TQueryModel), typeof(TEntity)));
        }
    }
}
