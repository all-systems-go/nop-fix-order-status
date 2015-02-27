using Autofac;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Services.Orders;

namespace AllSystemsGo.Plugin.FixDownloadableProductOrderStatus
{
	public class DependencyRegistrar : IDependencyRegistrar
	{
		public void Register(ContainerBuilder builder, ITypeFinder typeFinder)
		{
			if (!Plugin.IsInstalled(typeFinder))	// only register dependencies if the plugin is installed
				return;

			builder.RegisterType<OrderProcessingService>().As<IOrderProcessingService>().InstancePerLifetimeScope();
		}

		public int Order
		{
			get { return int.MaxValue; }
		}
	}
}
