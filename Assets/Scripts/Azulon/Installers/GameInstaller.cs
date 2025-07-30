using Zenject;
using Azulon.Services;
using System;
using UnityEngine;
using Azulon.UI;

namespace Azulon.Installers
{
	public class GameInstaller : MonoInstaller
	{
		[SerializeField] private UIColorSettingsSO UIColorSettings;

        public override void InstallBindings()
		{
			// Bind ItemService as singleton
			Container.Bind<IItemService>()
					 .To<ItemService>()
					 .AsSingle()
					 .NonLazy();

			Container.Bind<UIColorSettingsSO>()
                     .FromInstance(UIColorSettings)
                     .AsSingle()
                     .NonLazy();

            // Initialize the ItemService with saved data
            Container.Resolve<IItemService>().LoadData();
		}
	}
}
