using System.ComponentModel.DataAnnotations;
using System.IO;

namespace FastBuilder.ViewModels.Settings
{
	public class SettingsValidator
	{
		public static ValidationResult ValidateBrokeragePath(string brokeragePath, ValidationContext context)
		{
			if (!Directory.Exists(brokeragePath))
			{
				return new ValidationResult("brokerage path not existed", new[] { nameof(SettingsViewModel.BrokeragePath) });
			}

			return ValidationResult.Success;
		}
	}
}
