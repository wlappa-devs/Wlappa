using System;
using Android.App;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using AndroidClient.UI.InLobbyViews;
using AndroidClient.ViewModels;
using Client_lib;
using Unity;
using ZXing.Mobile;

namespace AndroidClient.UI.PreLobbyViews
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = false,
        WindowSoftInputMode = SoftInput.AdjustResize)]
    public class JoinServerActivity : AppCompatActivity
    {
        private PreLobbyViewModel _viewModel = null!;
        private TextInputLayout _servercodeLayout = null!;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            _viewModel = (Application as App)!.Container.Resolve<PreLobbyViewModel>();
            Xamarin.Essentials.Platform.Init(Application);

            SetContentView(Resource.Layout.join_server);

            var idInput = FindViewById<EditText>(Resource.Id.server_code_tf);
            var scanQrButton = FindViewById<Button>(Resource.Id.scan_qr_code_btn);
            var scanner = new MobileBarcodeScanner();

            scanQrButton!.Click += async (_, __) =>
            {
                var result = await scanner!.Scan();
                idInput!.Text = new Guid(Convert.FromBase64String(result.Text)).ToString();
            };

            _servercodeLayout = FindViewById<TextInputLayout>(Resource.Id.server_code_lt)!;

            FindViewById<EditText>(Resource.Id.server_code_tf)!.TextChanged +=
                (sender, args) => _servercodeLayout.Error = null;

            var startedJoining = false;
            FindViewById<Button>(Resource.Id.join_btn)!.Click +=
                async (sender, args) =>
                {
                    if (startedJoining) return;
                    startedJoining = true;
                    try
                    {
                        _viewModel.Id = idInput?.Text;
                        if (_viewModel.Id == "")
                        {
                            _servercodeLayout.Error = "Enter Game ID";
                            return;
                        }

                        await _viewModel.JoinLobby();
                    }
                    catch (FormatException)
                    {
                        _servercodeLayout.Error = "Invalid Game ID";
                    }
                    catch (LobbyNotFoundException)
                    {
                        _servercodeLayout.Error = "Lobby not found";
                    }
                    catch (GameAlreadyStartedException)
                    {
                        _servercodeLayout.Error = "Game is already in process";
                    }
                    finally
                    {
                        startedJoining = false;
                    }
                };
        }

        protected override void OnStart()
        {
            base.OnStart();
            _viewModel.JoinedLobby = OnViewModelJoinedLobby;
            _viewModel.ShowNotification += ShowNotification;
        }

        private void ShowNotification(string text) =>
            _servercodeLayout.Error = text;

        protected override void OnStop()
        {
            base.OnStop();
            _viewModel.JoinedLobby = null;
            _viewModel.ShowNotification -= ShowNotification;
        }

        private void OnViewModelJoinedLobby()
        {
            StartActivity(typeof(LobbyActivity));
            Finish();
        }
    }
}