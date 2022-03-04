using CustomValidator.Extensions;
using System;

namespace CustomValidator.Helpers
{
    internal class ExceptionHelper
    {
		/// <summary>
		/// Throws NullReferenceException if true with message.
		/// </summary>
		internal static void ThrowIfNull(bool throwEx, string exMessage)
		{
			if (throwEx)
			{
				throw new NullReferenceException(exMessage);
			}
		}

		/// <summary>
		/// Throws ArgumentNullException if true with message.
		/// </summary>
		internal static void ThrowIfArgumentNull(bool throwEx, string exMessage)
		{
			if (throwEx)
			{
				throw new ArgumentNullException(exMessage);
			}
		}

		/// <summary>
		/// Throws EmptyException if true with message.
		/// </summary>
		internal static void ThrowIfEmpty(bool throwEx, string exMessage)
		{
			if (throwEx)
			{
				throw new EmptyException(exMessage);
			}
		}

		/// <summary>
		/// Throws ArgumentOutOfRangeException if true with message.
		/// </summary>
		internal static void ThrowIfArgumentOutOfRange(bool throwEx, string exMessage)
		{
			if (throwEx)
			{
				throw new ArgumentOutOfRangeException(exMessage);
			}
		}

		/// <summary>
		/// Throws an ArgumentNullException if argument is null.
		/// </summary>
		/// <param name="argumentName">The argument name.</param>
		/// <param name="argument">The argument.</param>
		internal static void ThrowIfNull(string argumentName, object argument)
		{
			if (argument == null)
			{
				throw new ArgumentNullException(argumentName);
			}
		}

		/// <summary>
		/// Throws an ArgumentNullException if argument is null or an ArgumentException if string is empty.
		/// </summary>
		/// <param name="argumentName">The argument name.</param>
		/// <param name="argument">The argument.</param>
		internal static void ThrowIfNullOrEmpty(string argumentName, string argument)
		{
			ThrowIfNull(argumentName, argument);

			if (string.IsNullOrEmpty(argument))
			{
				throw new ArgumentException("Argument '{0}' can't be empty.", argumentName);
			}
		}
	}
}
