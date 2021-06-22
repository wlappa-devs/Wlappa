using System;
using Android.App;
using Android.Runtime;
using Android.Support.V7.App;
using AndroidClient.ViewModels;
using AndroidClient.ViewModels.GameViewModels;
using AndroidClient.ViewModels.Providers;
using Client_lib;
using Unity;

namespace AndroidClient
{
    [Application]
    public class App : Application
    {
        // TODO Store all state in viewmodels to be able to handle configurationChange
        public UnityContainer Container { get; } = new UnityContainer();

        public App(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
        {
            Container.RegisterSingleton<Client>();

            Container.RegisterSingleton<LobbyProvider>();

            Container.RegisterSingleton<PreLobbyViewModel>();

            Container.RegisterSingleton<GameViewModelMappingProvider>();
            Container.RegisterSingleton<GameViewModelFactory>();
            Container.RegisterFactory<HatViewModel>(container =>
                container.Resolve<GameViewModelFactory>().GetViewModel<HatViewModel>());
            Container.RegisterSingleton<LobbyViewModel>();
        }

        public override void OnCreate()
        {
            base.OnCreate();
            AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightNo;
        }
    }
}