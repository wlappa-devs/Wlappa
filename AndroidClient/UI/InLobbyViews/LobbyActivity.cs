using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidClient.ViewModels;
using Unity;
using ZXing;
using ZXing.Common;

namespace AndroidClient.UI.InLobbyViews
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = false,
        WindowSoftInputMode = SoftInput.AdjustResize)]
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
            var qrCodeView = FindViewById<ImageView>(Resource.Id.qr_code);

            qrCodeView!.SetImageBitmap(GenerateQrCodeForGuid(_viewModel.LobbyId));

            _idView = FindViewById<EditText>(Resource.Id.lobby_game_id);
            _viewModel.MakeSnackBar = msg => Snackbar.Make(_idView, msg, 2000).Show();
            _idView!.Text = _viewModel.LobbyId.ToString();
            _idView!.Focusable = false;
            _idView!.Clickable = true;
            var clipboard = (ClipboardManager?) GetSystemService(ClipboardService);
            _idView.Click += (sender, args) =>
            {
                Log.Info(nameof(LobbyActivity), "Copying GUID");

                var clip = ClipData.NewPlainText(_viewModel.LobbyId.ToString(), _viewModel.LobbyId.ToString());
                clipboard!.PrimaryClip = clip;
                Snackbar.Make(_idView, "Copied to clipboard", 2000).Show();
            };
            Log.Info(nameof(LobbyActivity), "GUID IS " + _idView!.Text);

            if (_viewModel.AmHost.HasValue)
                if (!_viewModel.AmHost.Value)
                {
                    Log.Info(nameof(LobbyActivity), "BUTTON START IZ NAU JOIN CUZ U R LOX XD");
                    Log.Info(nameof(LobbyActivity), _viewModel.AmHost.ToString());
                    startBtn!.Text = "Ready";
                    startBtn!.Click +=
                        async (sender, args) =>
                        {
                            Log.Info(nameof(LobbyActivity), "ReadyBtnPressed");
                            await _viewModel.HandlePlayerReadinessChange(false);
                        };
                }
                else
                {
                    startBtn!.Click +=
                        async (sender, args) =>
                        {
                            Log.Info(nameof(LobbyActivity), "StartBtnPressed");
                            await _viewModel.HandlePlayerReadinessChange(true);
                            await _viewModel.HandleGameStartButtonPressing();
                        };
                }


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
            SupportActionBar.Title = "Lobby";
            if (!(_viewModel.LastLobbyStatus is null))
                OnViewModelLobbyUpdate();
            _viewModel.StartProcessingEvents();
            _viewModel.InvalidateGameInstance();
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

        private static Bitmap GenerateQrCodeForGuid(Guid guid)
        {
            var text = Convert.ToBase64String(guid.ToByteArray());
            var options = new EncodingOptions
            {
                Width = 512,
                Height = 512,
                Margin = 0
            };
            var writer = new BarcodeWriterGeneric
            {
                Format = BarcodeFormat.QR_CODE,
                Options = options
            };
            var bitMatrix = writer.Encode(text);
            var bitmap = Bitmap.CreateBitmap(512, 512, Bitmap.Config.Argb8888!);
            for (var x = 0; x < 512; x++)
            for (var y = 0; y < 512; y++)
                bitmap!.SetPixel(x, y, bitMatrix[x, y] ? Color.Black : Color.White);
            return bitmap!;
        }
    }
}