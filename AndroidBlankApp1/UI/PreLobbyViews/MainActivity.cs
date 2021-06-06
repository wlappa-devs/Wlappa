using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using AndroidBlankApp1.ViewModels;
using Unity;

namespace AndroidBlankApp1.UI.PreLobbyViews
{
    //TODO Unsubscribe from events at the end of lifecycle

    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private PreLobbyViewModel _viewModel;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            var app = Application as App;
            _viewModel = app!.Container.Resolve<PreLobbyViewModel>();
            SetContentView(Resource.Layout.activity_main);
            FindViewById<Button>(Resource.Id.join_server_btn)!.Click +=
                (sender, args) => _viewModel.HandleJoinLobbyButton(sender as View);

            FindViewById<Button>(Resource.Id.choose_game_btn)!.Click +=
                (sender, args) => _viewModel.HandleCreateLobbyButton(sender as View);

            FindViewById<EditText>(Resource.Id.nickname_tf)!.TextChanged +=
                (sender, args) => _viewModel.Name = string.Concat(args.Text!);
        }

        protected override void OnStart()
        {
            base.OnStart();
            _viewModel.ShouldSelectLobby = OnViewModelShouldJoinLobby;
            _viewModel.LobbyCreated = OnViewModelOnLobbyCreated;
        }

        protected override void OnStop()
        {
            base.OnStop();
            _viewModel.ShouldSelectLobby = null;
            _viewModel.LobbyCreated = null;
        }

        private void OnViewModelOnLobbyCreated() => StartActivity(typeof(ChooseGameActivity));

        private void OnViewModelShouldJoinLobby() => StartActivity(typeof(JoinServerActivity));
    }
}