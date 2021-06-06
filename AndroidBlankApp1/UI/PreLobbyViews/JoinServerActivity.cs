using System;
using Android.App;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using AndroidBlankApp1.UI.InLobbyViews;
using AndroidBlankApp1.ViewModels;
using Unity;

namespace AndroidBlankApp1.UI.PreLobbyViews
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = false)]
    public class JoinServerActivity : AppCompatActivity
    {
        private PreLobbyViewModel _viewModel;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            _viewModel = (Application as App)!.Container.Resolve<PreLobbyViewModel>();

            SetContentView(Resource.Layout.join_server);

            var idInput = FindViewById<EditText>(Resource.Id.server_code_tf);

            FindViewById<Button>(Resource.Id.join_btn)!.Click +=
                async (sender, args) =>
                {
                    try
                    {
                        _viewModel.Id = idInput?.Text;
                        await _viewModel.JoinLobby(sender as View);
                    }
                    catch (Exception e)
                    {
                        Snackbar.Make(sender as View, "Invalid id, dumbass", 2000).Show();
                    }
                };

            // FindViewById<EditText>(Resource.Id.server_code_tf)!.TextChanged +=
            // (sender, args) => viewModel.Id = string.Concat(args.Text!);
        }

        protected override void OnStart()
        {
            base.OnStart();
            _viewModel.JoinedLobby = OnViewModelJoinedLobby;
        }

        protected override void OnStop()
        {
            base.OnStop();
            _viewModel.JoinedLobby = null;
        }

        private void OnViewModelJoinedLobby() => StartActivity(typeof(LobbyActivity));
    }
}