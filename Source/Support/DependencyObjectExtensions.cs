using System.Windows;
using System.Windows.Media;

namespace FastBuild.Dashboard.Support
{
	internal static class DependencyObjectExtensions
	{
		public static T FindAncestor<T>(this DependencyObject child) where T : DependencyObject
		{
			while (true)
			{
				var parentObject = VisualTreeHelper.GetParent(child);
				
				if (parentObject == null)
				{
					return null;
				}
				
				if (parentObject is T parent)
				{
					return parent;
				}

				child = parentObject;
			}
		}
	}
}
