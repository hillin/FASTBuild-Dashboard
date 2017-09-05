using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Caliburn.Micro;
using FastBuild.Dashboard.Configuration;
using FastBuild.Dashboard.Services.Build.SourceEditor;
using Ookii.Dialogs.Wpf;

namespace FastBuild.Dashboard.ViewModels.Settings
{
	internal partial class SettingsViewModel
	{
		public IEnumerable<ExternalSourceEditorMetadata> ExternalSourceEditors =>
			IoC.Get<IExternalSourceEditorService>().ExternalSourceEditors;

		public ExternalSourceEditorMetadata SelectedExternalSourceEditor
		{
			get => IoC.Get<IExternalSourceEditorService>().SelectedEditor;
			set
			{
				IoC.Get<IExternalSourceEditorService>().SelectedEditor = value;
				this.NotifyOfPropertyChange();
				this.NotifyOfPropertyChange(nameof(this.ExternalSourceEditorPath));
				this.NotifyOfPropertyChange(nameof(this.ExternalSourceEditorArgs));
				this.NotifyOfPropertyChange(nameof(this.ExternalSourceEditorAdditionalArgs));
			}
		}

		[CustomValidation(typeof(SettingsValidator), "ValidateExternalSourceEditorPath")]
		public string ExternalSourceEditorPath
		{
			get => AppSettings.Default.ExternalSourceEditorPath;
			set
			{
				AppSettings.Default.ExternalSourceEditorPath = value;
				AppSettings.Default.Save();
				this.NotifyOfPropertyChange();
			}
		}

		public string ExternalSourceEditorArgs
		{
			get => AppSettings.Default.ExternalSourceEditorArgs;
			set
			{
				AppSettings.Default.ExternalSourceEditorArgs = value;
				AppSettings.Default.Save();
				this.NotifyOfPropertyChange();
			}
		}

		public string ExternalSourceEditorAdditionalArgs
		{
			get => AppSettings.Default.ExternalSourceEditorAdditionalArgs;
			set
			{
				AppSettings.Default.ExternalSourceEditorAdditionalArgs = value;
				AppSettings.Default.Save();
				this.NotifyOfPropertyChange();
			}
		}

		public void BrowseExternalSourceEditor()
		{
			var dialog = new VistaOpenFileDialog
			{
				Title = "Browse External Source Editor",
				Filter = "Editor Executable|*.exe",
				FileName = this.ExternalSourceEditorPath
			};

			if (dialog.ShowDialog(App.Current.MainWindow) == true)
			{
				this.ExternalSourceEditorPath = dialog.FileName;
			}
		}

	}
}
