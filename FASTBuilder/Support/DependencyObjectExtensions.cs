using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace FastBuilder.Support
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
