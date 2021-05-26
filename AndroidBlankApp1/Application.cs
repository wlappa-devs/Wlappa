using System;
using Android.App;
using Android.Runtime;
using AndroidBlankApp1.ViewModels;
using Client_lib;
using Unity;

namespace AndroidBlankApp1
{
    [Application]
    public class App : Application
    {
        public UnityContainer Container { get; } = new UnityContainer();
        
        public App(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
        {
            Container.RegisterSingleton<Client>();

            Container.RegisterSingleton<LobbyProvider>();
            
            Container.RegisterSingleton<PreLobbyViewModel>();
            Container.RegisterSingleton<LobbyViewModel>();
        }
        
        public override void OnCreate()
        {
            base.OnCreate();
            
        }
    }
}