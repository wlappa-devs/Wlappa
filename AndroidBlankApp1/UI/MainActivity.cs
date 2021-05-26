using System;
using System.Collections.Generic;
using System.Threading.Channels;
using Android;
using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidBlankApp1.ViewModels;
using Grpc.Core;
using ProtoBuf.Grpc;
using ProtoBuf.Grpc.Client;
using Shared.Protos;
using Unity;
using Channel = System.Threading.Channels.Channel;

namespace AndroidBlankApp1
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            var app = Application as App;

            var viewModel = app.Container.Resolve<PreLobbyViewModel>();
            SetContentView(Resource.Layout.activity_main);
            FindViewById<Button>(Resource.Id.join_server_btn).Click +=
                (sender, args) => StartActivity(typeof(JoinServerActivity));

            FindViewById<Button>(Resource.Id.choose_game_btn).Click +=
                (sender, args) => viewModel.HandleCreateLobbyButton(sender as View);
                // (sender, args) => 
            viewModel.LobbyCreateButtonPressed += () => StartActivity(typeof(ChooseGameActivity)); 
                
            
            FindViewById<Button>(Resource.Id.save_nickname_btn).Click +=
                (sender, args) => Log.Info(nameof(MainActivity), FindViewById<TextView>(Resource.Id.nickname_tf).Text);
            var rootView = FindViewById<LinearLayout>(Resource.Id.main_layout);
        }

        private async IAsyncEnumerable<ClientMessage> Generator()
        {
            yield return new Greeting()
            {
                Name = "Test"
            };
        }

        private async void HandleResponse(IAsyncEnumerable<ServerMessage> response)
        {
            await foreach (var m in response)
            {
                if (m is GreetingSuccessful e)
                    Log.Info(nameof(MainActivity), e.Guid.ToString());
            }
        }
    }
}