using System;
using System.Windows.Input;

namespace FastBuild.Dashboard.Support
{
	internal class SimpleCommand : ICommand
	{
		private readonly Action<object> _execute;
		private readonly Predicate<object> _canExecute;

		public event EventHandler CanExecuteChanged;

		public SimpleCommand(Action<object> execute, Predicate<object> canExecute = null)
		{
			_execute = execute;
			_canExecute = canExecute;
		}

		public void Execute(object parameter)
		{
			_execute(parameter);
		}

		public bool CanExecute(object parameter)
		{
			return _canExecute?.Invoke(parameter) ?? true;
		}

		public void OnCanExecuteChanged()
		{
			this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
		}

		public static implicit operator SimpleCommand(Action<object> execute)
		{
			return new SimpleCommand(execute);
		}
	}
}
