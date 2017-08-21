using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
