using System;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace Caliburn.Micro.Validation
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	internal class FileExistsAttribute : ValidationAttribute
	{
		/// <summary>
		/// Applies formatting to an error message, based on the data field where the error occurred.
		/// </summary>
		/// <param name="name">The name to include in the formatted message.</param>
		/// <returns>
		/// An instance of the formatted error message.
		/// </returns>
		public override string FormatErrorMessage(string name)
		{
			return string.Format("The {0} field specifies a file that does not exist.", name);
		}

		/// <summary>
		/// Determines whether the specified value of the object is valid.
		/// </summary>
		/// <param name="value">The value of the object to validate.</param>
		/// <returns>
		/// true if the specified value is valid; otherwise, false.
		/// </returns>
		public override bool IsValid(object value)
		{
			// The value should be a string representing a valid file path
			string path = value as string;
			return string.IsNullOrWhiteSpace(path) || File.Exists(path);
		}
	}
}