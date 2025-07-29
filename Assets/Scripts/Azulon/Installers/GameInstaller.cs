using Zenject;
using Azulon.Services;

namespace Azulon.Installers
{
	public class GameInstaller : MonoInstaller
	{
		public override void InstallBindings()
		{
			// Bind ItemService as singleton
			Container.Bind<IItemService>()
					 .To<ItemService>()
					 .AsSingle()
					 .NonLazy();

			// Initialize the ItemService with saved data
			Container.Resolve<IItemService>().LoadData();
		}
	}
}
