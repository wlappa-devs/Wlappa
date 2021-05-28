using System;
using Android.App;
using Android.Runtime;
using AndroidBlankApp1.ViewModels;
using AndroidBlankApp1.ViewModels.GameViewModels;
using Client_lib;
using Unity;

namespace AndroidBlankApp1
{
    [Application]
    public class App : Application
    {
        // TODO Deactivate buttons when waiting for responses 
        // TODO Store all state in viewmodels to be able to handle configurationChange
        public UnityContainer Container { get; } = new UnityContainer();
        
        public App(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
        {
            Container.RegisterSingleton<Client>();

            Container.RegisterSingleton<LobbyProvider>();
            Container.RegisterSingleton<GameInstanceProvider>();
            
            Container.RegisterSingleton<PreLobbyViewModel>();
            Container.RegisterSingleton<LobbyViewModel>();
            Container.RegisterSingleton<HatViewModel>();
        }
        
        public override void OnCreate()
        {
            base.OnCreate();
            
        }
    }
}