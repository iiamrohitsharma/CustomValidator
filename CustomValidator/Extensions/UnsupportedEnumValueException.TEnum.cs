using System;
using System.Runtime.Serialization;

namespace CustomValidator.Extensions
{
    /// <summary>
    /// The exception that is thrown when the enum value is not supported.
    /// </summary>
    public class UnsupportedEnumValueException<TEnum> : Exception
	{
		public UnsupportedEnumValueException(string message)
			: base(message) { }
		public UnsupportedEnumValueException(string message, Exception inner)
			: base(message, inner) { }
		protected UnsupportedEnumValueException(SerializationInfo info, StreamingContext context)
			: base(info, context) { }

		public UnsupportedEnumValueException(TEnum enumValue)
			: base($"Value {enumValue} of enum {typeof(TEnum).Name} is not supported.")
		{ }
	}
}
