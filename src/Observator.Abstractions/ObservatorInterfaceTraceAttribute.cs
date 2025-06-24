using System;

namespace Observator.Abstractions
{
    /// <summary>
    /// Specifies that an interface or interface method should be traced by Observator.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class ObservatorInterfaceTraceAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets a value indicating whether method parameters should be included in the trace.
        /// </summary>
        public bool IncludeParameters { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the method's return value should be included in the trace.
        /// </summary>
        public bool IncludeReturnValue { get; set; } = true;
    }
}