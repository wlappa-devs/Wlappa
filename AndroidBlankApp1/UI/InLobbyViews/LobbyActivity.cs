using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidBlankApp1.ViewModels;
using Unity;
using RecyclerView = Android.Support.V7.Widget.RecyclerView;

namespace AndroidBlankApp1.UI.InLobbyViews
{
    // TODO add ready check
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = false)]
    public class LobbyActivity : AppCompatActivity
    {
        private LobbyViewModel _viewModel;
        private PlayersListAdapter _adapter;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lobby);

            _viewModel = (Application as App)!.Container.Resolve<LobbyViewModel>();
            _adapter = new PlayersListAdapter(_viewModel.LastLobbyStatus, _viewModel.Roles, _viewModel.AmHost!.Value);

            var startBtn = FindViewById<Button>(Resource.Id.start_game_btn);

            var idView = FindViewById<EditText>(Resource.Id.lobby_game_id);
            _viewModel.MakeSnackBar = msg => Snackbar.Make(idView, msg, 2000).Show();
            idView!.Text = _viewModel.LobbyId.ToString();
            idView.Click += (sender, args) =>
            {
                Log.Info(nameof(LobbyActivity), "Copying GUID");
                var clipboard = (ClipboardManager) GetSystemService(ClipboardService);
                var clip = ClipData.NewPlainText(_viewModel.LobbyId.ToString(), _viewModel.LobbyId.ToString());
                clipboard!.PrimaryClip = clip;
                Snackbar.Make(idView, "Copied to clipboard", 2000).Show();
            };
            Log.Info(nameof(LobbyActivity), "GUID IS " + idView!.Text);

            if (_viewModel.AmHost.HasValue && !_viewModel.AmHost.Value)
            {
                Log.Info(nameof(LobbyActivity), "HIDING BUTTON CUZ U R LOX XD");
                Log.Info(nameof(LobbyActivity), _viewModel.AmHost.ToString());
                startBtn!.Visibility = ViewStates.Gone;
            }

            startBtn!.Click +=
                async (sender, args) =>
                {
                    Log.Info(nameof(LobbyActivity), "StartBtnPressed");
                    await _viewModel.HandleGameStartButtonPressing(sender as View);
                };

            var recyclerView = FindViewById<RecyclerView>(Resource.Id.recycler);

            _adapter.PlayerRoleChanged += async (guid, s) =>
            {
                Log.Info(nameof(LobbyActivity), "Got player role changed");
                await _viewModel.HandlePlayerRoleChange(guid, s);
            };

            recyclerView!.SetAdapter(_adapter);
        }

        protected override void OnStart()
        {
            base.OnStart();
            _viewModel.LobbyUpdate = OnViewModelLobbyUpdate;
            _viewModel.GameStarted = OnViewModelGameStarted;
            _viewModel.StartProcessingEvents();
        }

        protected override void OnStop()
        {
            base.OnStop();
            _viewModel.LobbyUpdate = null;
            _viewModel.GameStarted = null;
        }

        private void OnViewModelGameStarted()
        {
            Log.Info(nameof(LobbyActivity), "Got game started");
            StartActivity(ActivityForGameType.GameTypeToActivity[_viewModel.LobbyGameType]);
        }

        private void OnViewModelLobbyUpdate()
        {
            Log.Info(nameof(LobbyActivity), $"Got lobby update {_viewModel.LastLobbyStatus.Count}");
            RunOnUiThread(() => _adapter.Players = _viewModel.LastLobbyStatus);
        }
    }
}