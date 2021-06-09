using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using AndroidClient.ViewModels;
using Unity;

namespace AndroidClient.UI.PreLobbyViews
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private PreLobbyViewModel _viewModel = null!;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            var app = Application as App;
            _viewModel = app!.Container.Resolve<PreLobbyViewModel>();
            SetContentView(Resource.Layout.activity_main);
            var joinLobbyButton = FindViewById<Button>(Resource.Id.join_server_btn);
            joinLobbyButton!.Click +=
                (sender, args) => _viewModel.HandleJoinLobbyButton();

            FindViewById<Button>(Resource.Id.choose_game_btn)!.Click +=
                (sender, args) => _viewModel.HandleCreateLobbyButton();

            FindViewById<EditText>(Resource.Id.nickname_tf)!.TextChanged +=
                (sender, args) => _viewModel.Name = string.Concat(args.Text!);
        }

        protected override void OnStart()
        {
            base.OnStart();
            _viewModel.ShouldSelectLobby = OnViewModelShouldJoinLobby;
            _viewModel.LobbyCreated = OnViewModelOnLobbyCreated;
            _viewModel.ShowNotification += ShowNotification;
        }
        
        private void ShowNotification(string text)
        {
            Toast.MakeText(ApplicationContext, text, ToastLength.Long)?.Show();
        }

        protected override void OnStop()
        {
            base.OnStop();
            _viewModel.ShouldSelectLobby = null;
            _viewModel.LobbyCreated = null;
            _viewModel.ShowNotification -= ShowNotification;
        }

        private void OnViewModelOnLobbyCreated() => StartActivity(typeof(ChooseGameActivity));

        private void OnViewModelShouldJoinLobby() => StartActivity(typeof(JoinServerActivity));
    }
}