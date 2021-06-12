using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidClient.ViewModels;
using Unity;
using RecyclerView = Android.Support.V7.Widget.RecyclerView;

namespace AndroidClient.UI.InLobbyViews
{
    // TODO add ready check
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = false)]
    public class LobbyActivity : AppCompatActivity
    {
        private static readonly TimeSpan HostLeftSnackBarLength = TimeSpan.FromSeconds(2);
        private LobbyViewModel _viewModel = null!;
        private PlayersListAdapter _adapter = null!;
        private EditText? _idView;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lobby);

            _viewModel = (Application as App)!.Container.Resolve<LobbyViewModel>();
            _viewModel.InitializeLobby();
            _adapter = new PlayersListAdapter(_viewModel.LastLobbyStatus, _viewModel.Roles, _viewModel.AmHost!.Value);

            var startBtn = FindViewById<Button>(Resource.Id.start_game_btn);

            _idView = FindViewById<EditText>(Resource.Id.lobby_game_id);
            _viewModel.MakeSnackBar = msg => Snackbar.Make(_idView, msg, 2000).Show();
            _idView!.Text = _viewModel.LobbyId.ToString();

            _idView.Click += (sender, args) =>
            {
                Log.Info(nameof(LobbyActivity), "Copying GUID");
                var clipboard = (ClipboardManager?) GetSystemService(ClipboardService);
                var clip = ClipData.NewPlainText(_viewModel.LobbyId.ToString(), _viewModel.LobbyId.ToString());
                clipboard!.PrimaryClip = clip;
                Snackbar.Make(_idView, "Copied to clipboard", 2000).Show();
            };
            Log.Info(nameof(LobbyActivity), "GUID IS " + _idView!.Text);

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
                    await _viewModel.HandleGameStartButtonPressing();
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
            _viewModel.LobbyDestroyed = OnViewModelLobbyDestroyed;
            _viewModel.StartProcessingEvents();
        }

        private async void OnViewModelLobbyDestroyed(string msg)
        {
            Snackbar.Make(_idView, msg, HostLeftSnackBarLength.Milliseconds).Show();
            await Task.Delay(HostLeftSnackBarLength).ConfigureAwait(true);
            Finish();
        }

        protected override void OnStop()
        {
            base.OnStop();
            _viewModel.LobbyUpdate = null;
            _viewModel.GameStarted = null;
        }

        protected override async void OnDestroy()
        {
            base.OnDestroy();
            await _viewModel.DisconnectFromLobby();
        }

        private void OnViewModelGameStarted()
        {
            Log.Info(nameof(LobbyActivity), "Got game started");
            StartActivity(ActivityForGameType.GameTypeToActivity[_viewModel.LobbyGameType]);
        }

        private void OnViewModelLobbyUpdate()
        {
            Log.Info(nameof(LobbyActivity), $"Got lobby update {_viewModel.LastLobbyStatus?.Count}");
            RunOnUiThread(() => _adapter.Players = _viewModel.LastLobbyStatus);
        }
    }
}