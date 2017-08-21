using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Caliburn.Micro.Validation
{
	internal abstract class ValidatingScreen<TViewModel> : Screen, IDataErrorInfo
		where TViewModel : ValidatingScreen<TViewModel>
	{
		/// <summary>
		/// A dictionary of property getters for all validated properties
		/// </summary>
		private static readonly Dictionary<string, Func<TViewModel, object>> propertyGetters;

		/// <summary>
		/// A dictionary of the validation groups including 'default'
		/// </summary>
		private static readonly Dictionary<string, ValidationGroupAttribute> propertyValidationGroups;

		/// <summary>
		/// A dictionary of the validators (if any) associated with each class property
		/// </summary>
		private static readonly Dictionary<string, ValidationAttribute[]> propertyValidators;

		/// <summary>
		/// Initializes the <see cref="ValidatingScreen{TViewModel}"/> class.
		/// </summary>
		static ValidatingScreen()
		{
			List<PropertyInfo> validatedProperties = (from propertyInfo in typeof(TViewModel).GetProperties()
			                                          where propertyInfo.GetAttributes<ValidationAttribute>(true).Any()
			                                          select propertyInfo).ToList();

			propertyGetters = (from propertyInfo in validatedProperties
			                   select propertyInfo).ToDictionary(p => p.Name, GetValueGetter);

			propertyValidators = (from propertyInfo in validatedProperties
			                      let validators = propertyInfo.GetAttributes<ValidationAttribute>(true).ToArray()
			                      select new {
			                      	propertyInfo.Name,
			                      	Attributes = validators
			                      }).ToDictionary(p => p.Name, p => p.Attributes);

			propertyValidationGroups = (from propertyInfo in validatedProperties
			                            let @group = propertyInfo.GetAttributes<ValidationGroupAttribute>(true).FirstOrDefault()
			                            select new {
			                            	propertyInfo.Name,
			                            	Attribute = @group ?? new ValidationGroupAttribute()
			                            }).ToDictionary(p => p.Name, p => p.Attribute);
		}

		/// <summary>
		/// Gets an error message indicating what is wrong with this object.
		/// </summary>
		/// <returns>
		/// An error message indicating what is wrong with this object. The default is an empty string ("").
		/// </returns>
		public string Error
		{
			get
			{
				// Run the validation for all properties that participate in global validation
				List<string> errorList = (from i in propertyValidators
				                          let groupAttribute = propertyValidationGroups[i.Key]
				                          let value = propertyGetters[i.Key]((TViewModel)this)
				                          from validator in i.Value
				                          where groupAttribute.ParticipateInGlobalValidation && !validator.IsValid(value)
				                          select (validator.FormatErrorMessage(i.Key))).ToList();
				OnError(errorList);
				return string.Join(Environment.NewLine, errorList.ToArray());
			}
		}

		/// <summary>
		/// Returns True if any of the property values generate a validation error
		/// </summary>
		public bool HasErrors
		{
			get { return !string.IsNullOrEmpty(Error); }
		}

		/// <summary>
		/// Gets the error message for the property with the given name.
		/// </summary>
		/// <returns>
		/// The error message for the property. The default is an empty string ("").
		/// </returns>
		/// <param name="propertyName">The name of the property whose error message to get. </param>
		public string this[string propertyName]
		{
			get
			{
				if (propertyGetters.ContainsKey(propertyName))
				{
					object value = propertyGetters[propertyName]((TViewModel)this);
					List<string> errors = (from validator in propertyValidators[propertyName]
					                       where !validator.IsValid(value)
					                       select validator.FormatErrorMessage(propertyName)).ToList();
					OnPropertyError(propertyName, errors);
					return string.Join(Environment.NewLine, errors.ToArray());
				}
				return string.Empty;
			}
		}

		/// <summary>
		/// Test all validators for all properties for a named group and returns a string containing 
		/// messages if any need to be reported.
		/// </summary>
		public string ErrorByGroup(string groupName)
		{
			// Run the validation but only for groups which should be included
			List<string> errorList = (from i in propertyValidators
			                          let groupAttribute = propertyValidationGroups[i.Key]
			                          let value = propertyGetters[i.Key]((TViewModel)this)
			                          from validator in i.Value
			                          where (string.Compare(groupAttribute.GroupName, groupName, true) == 0) && !validator.IsValid(value)
			                          select (validator.FormatErrorMessage(i.Key))).ToList();
			OnError(errorList);
			return string.Join(Environment.NewLine, errorList.ToArray());
		}

		/// <summary>
		/// Returns True if any of the property values in the named group generate a validation error
		/// </summary>
		public bool HasErrorsByGroup(string groupName = ValidationGroupAttribute.DefaultGroupName)
		{
			return !string.IsNullOrEmpty(ErrorByGroup(groupName));
		}

		/// <summary>
		/// Executes the validators for a property
		/// </summary>
		/// <typeparam name="TProperty">An expression that returns the name of a property</typeparam>
		/// <param name="property">The property to be tested</param>
		/// <returns>True if all validators are valid</returns>
		public virtual bool IsValid<TProperty>(Expression<Func<TProperty>> property)
		{
			string name = property.GetMemberInfo().Name;
			object value = propertyGetters[name]((TViewModel)this);
			return propertyValidators[name].All(v => v.IsValid(value));
		}

		/// <summary>
		/// This protected method is called when WPF evaluates the Error property and gives the class a chance
		/// to extend the list of reported errors
		/// </summary>
		/// <param name="errors">The list of errors generated by validators</param>
		protected virtual void OnError(List<string> errors)
		{
			// Does nothing
		}

		/// <summary>
		/// This protected method is called when WPF evaluates the Error method and gives the class a chance 
		/// to extend the list of reported errors
		/// </summary>
		/// <param name="propertyName">The name of the column being tested</param>
		/// <param name="errors">The list of errors generated by validators</param>
		protected virtual void OnPropertyError(string propertyName, List<string> errors)
		{
			// Does nothing
		}

		private static Func<TViewModel, object> GetValueGetter(PropertyInfo property)
		{
			ParameterExpression instance = Expression.Parameter(typeof(TViewModel), "i");
			UnaryExpression cast = Expression.TypeAs(Expression.Property(instance, property), typeof(object));
			return (Func<TViewModel, object>)Expression.Lambda(cast, instance).Compile();
		}
	}
}