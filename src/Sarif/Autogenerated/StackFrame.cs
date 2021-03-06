// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// A function call within a stack trace.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.49.0.0")]
    public partial class StackFrame : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<StackFrame> ValueComparer => StackFrameEqualityComparer.Instance;

        public bool ValueEquals(StackFrame other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.StackFrame;
            }
        }

        /// <summary>
        /// The location to which this stack frame refers.
        /// </summary>
        [DataMember(Name = "location", IsRequired = false, EmitDefaultValue = false)]
        public Location Location { get; set; }

        /// <summary>
        /// The name of the module that contains the code of this stack frame.
        /// </summary>
        [DataMember(Name = "module", IsRequired = false, EmitDefaultValue = false)]
        public string Module { get; set; }

        /// <summary>
        /// The thread identifier of the stack frame.
        /// </summary>
        [DataMember(Name = "threadId", IsRequired = false, EmitDefaultValue = false)]
        public int ThreadId { get; set; }

        /// <summary>
        /// The address of the method or function that is executing.
        /// </summary>
        [DataMember(Name = "address", IsRequired = false, EmitDefaultValue = false)]
        public int Address { get; set; }

        /// <summary>
        /// The offset from the method or function that is executing.
        /// </summary>
        [DataMember(Name = "offset", IsRequired = false, EmitDefaultValue = false)]
        public int Offset { get; set; }

        /// <summary>
        /// The parameters of the call that is executing.
        /// </summary>
        [DataMember(Name = "parameters", IsRequired = false, EmitDefaultValue = false)]
        public IList<string> Parameters { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the stack frame.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StackFrame" /> class.
        /// </summary>
        public StackFrame()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StackFrame" /> class from the supplied values.
        /// </summary>
        /// <param name="location">
        /// An initialization value for the <see cref="P: Location" /> property.
        /// </param>
        /// <param name="module">
        /// An initialization value for the <see cref="P: Module" /> property.
        /// </param>
        /// <param name="threadId">
        /// An initialization value for the <see cref="P: ThreadId" /> property.
        /// </param>
        /// <param name="address">
        /// An initialization value for the <see cref="P: Address" /> property.
        /// </param>
        /// <param name="offset">
        /// An initialization value for the <see cref="P: Offset" /> property.
        /// </param>
        /// <param name="parameters">
        /// An initialization value for the <see cref="P: Parameters" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P: Properties" /> property.
        /// </param>
        public StackFrame(Location location, string module, int threadId, int address, int offset, IEnumerable<string> parameters, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(location, module, threadId, address, offset, parameters, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StackFrame" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public StackFrame(StackFrame other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Location, other.Module, other.ThreadId, other.Address, other.Offset, other.Parameters, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public StackFrame DeepClone()
        {
            return (StackFrame)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new StackFrame(this);
        }

        private void Init(Location location, string module, int threadId, int address, int offset, IEnumerable<string> parameters, IDictionary<string, SerializedPropertyInfo> properties)
        {
            if (location != null)
            {
                Location = new Location(location);
            }

            Module = module;
            ThreadId = threadId;
            Address = address;
            Offset = offset;
            if (parameters != null)
            {
                var destination_0 = new List<string>();
                foreach (var value_0 in parameters)
                {
                    destination_0.Add(value_0);
                }

                Parameters = destination_0;
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}