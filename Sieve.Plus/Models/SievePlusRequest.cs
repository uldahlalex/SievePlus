using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Sieve.Plus.Models
{
    /// <summary>
    /// Strongly-typed Sieve request that includes the query model type for OpenAPI/Swagger generation.
    /// Use this in controller methods to expose the available query properties to API clients.
    /// </summary>
    /// <typeparam name="TQueryModel">The query model that defines available filter and sort properties</typeparam>
    /// <example>
    /// <code>
    /// [HttpGet]
    /// public async Task&lt;List&lt;Computer&gt;&gt; GetComputers([FromQuery] SievePlusRequest&lt;ComputerQueryModel&gt; request)
    /// {
    ///     return await service.GetComputers(request);
    /// }
    /// </code>
    /// </example>
    [DataContract]
    public class SievePlusRequest<TQueryModel> where TQueryModel : ISievePlusQueryModel
    {
        /// <summary>
        /// Filter string (e.g., "price&gt;=1000,inStock==true")
        /// Properties available for filtering are defined in <typeparamref name="TQueryModel"/>.
        /// </summary>
        [DataMember]
        public string? Filters { get; set; }

        /// <summary>
        /// Sort string (e.g., "price,-rating")
        /// Properties available for sorting are defined in <typeparamref name="TQueryModel"/>.
        /// </summary>
        [DataMember]
        public string? Sorts { get; set; }

        /// <summary>
        /// Page number (1-based)
        /// </summary>
        [DataMember, Range(1, int.MaxValue)]
        public int? Page { get; set; }

        /// <summary>
        /// Page size
        /// </summary>
        [DataMember, Range(1, int.MaxValue)]
        public int? PageSize { get; set; }

        /// <summary>
        /// The query model that defines all available properties for filtering and sorting.
        /// This property is never used at runtime - it's only here to expose the query model structure to OpenAPI/Swagger.
        /// </summary>
        [DataMember]
        public TQueryModel? QueryModel { get; set; }

        /// <summary>
        /// Implicitly convert to SievePlusModel for use with the processor.
        /// </summary>
        public static implicit operator SievePlusModel(SievePlusRequest<TQueryModel> request)
        {
            return new SievePlusModel
            {
                Filters = request.Filters,
                Sorts = request.Sorts,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }
    }
}
