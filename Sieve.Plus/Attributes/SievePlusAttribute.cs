using System;
using Sieve.Plus.Models;

namespace Sieve.Plus.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SievePlusAttribute : Attribute, ISievePropertyMetadata
    {
        /// <summary>
        /// Override name used 
        /// </summary>
        public string Name { get; set; }

        public string FullName => Name;

        public bool CanSort { get; set; }
        public bool CanFilter { get; set; }
    }
}
