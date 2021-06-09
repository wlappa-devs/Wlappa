using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using AndroidClient.UI.InLobbyViews;
using AndroidClient.ViewModels;
using Client_lib;
using Unity;

namespace AndroidClient.UI.PreLobbyViews
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = false)]
    public class JoinServerActivity : AppCompatActivity
    {
        private PreLobbyViewModel _viewModel = null!;

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
                        await _viewModel.JoinLobby();
                    }
                    catch (LobbyNotFoundException)
                    {
                        Toast.MakeText((sender as View)?.Context, "Invalid id, dumbass", ToastLength.Short)?.Show();
                    }
                    catch (GameAlreadyStartedException)
                    {
                        Toast.MakeText((sender as View)?.Context, "Game is going without you LOX", ToastLength.Short)
                            ?.Show();
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
            Toast.MakeText(ApplicationContext, text, ToastLength.Long)?.Show();

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