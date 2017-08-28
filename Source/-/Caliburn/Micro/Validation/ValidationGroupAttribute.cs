using System;

namespace Caliburn.Micro.Validation
{
	/// <summary>
	/// Allows a group of properties to be validated together. If a validation group is not defined 
	/// then the property is assumed to be in the 'default' group. A property can only be in one validation group,
	/// as such, the ParticipateInGlobalValidation can be used to suppress global IDataErrorInfo.Error evaluation.
	/// </summary>
	/// <remarks>
	/// This attribute is intended to be used by a 'validating controller' class which 
	/// implements IDataErrorInfo.  The ValidatingScreen is an example of such a controller.
	/// </remarks>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	internal class ValidationGroupAttribute : Attribute
	{
		public const string DefaultGroupName = "Default";

		public ValidationGroupAttribute(string groupName = DefaultGroupName, bool participateInGlobalValidation = true)
		{
			GroupName = groupName;
			ParticipateInGlobalValidation = participateInGlobalValidation;
		}

		/// <summary>
		/// A name which defines a group of properties which should be validated together 
		/// </summary>
		public string GroupName { get; set; }

		/// <summary>
		/// Get/Set as flag to indicate whether validation for a field should be applied when Error/HasError is applied
		/// </summary>
		public bool ParticipateInGlobalValidation { get; set; }
	}
}