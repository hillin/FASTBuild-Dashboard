using System;
using System.ComponentModel.DataAnnotations;

namespace Caliburn.Micro.Validation
{
	/// <summary>
	/// Validates the entry of a single email address
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	internal class EmailAttribute : RegularExpressionAttribute
	{
		/// <summary>
		/// RegEx expression used
		/// </summary>
		public const string EmailValidationExpression =
			@"([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)";

		/// <summary>
		/// Default constructor
		/// </summary>
		public EmailAttribute()
			: base(string.Format("^{0}$", EmailValidationExpression)) {}
	}
}