using Nop.Core.Infrastructure;
using Nop.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AllSystemsGo.Plugin.DownloadableProductOrderStatus
{
	public class Plugin : BasePlugin
	{
		public Plugin() : base()
		{
		}

		/// <summary>
		/// Check to see if this plugin is installed
		/// </summary>
		public static bool IsInstalled(ITypeFinder typeFinder)
		{
			IEnumerable<Type> types = typeFinder.FindClassesOfType<IPluginFinder>(true);

			if (types.Count() == 1)
			{
				IPluginFinder plugins = Activator.CreateInstance(types.First()) as IPluginFinder;
				PluginDescriptor descriptor = plugins.GetPluginDescriptorBySystemName("AllSystemsGo.FixDownloadableProductOrderStatus");

				if (descriptor != null && descriptor.Installed)
				{
					return true;
				}
			}

			return false;
		}
	}
}
