using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidBlankApp1.ViewModels;
using Unity;

namespace AndroidBlankApp1
{
    //TODO Unsucsribe from events at the end of lifecycle
    
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var app = Application as App;

            var viewModel = app.Container.Resolve<PreLobbyViewModel>();
            SetContentView(Resource.Layout.activity_main);

            FindViewById<Button>(Resource.Id.join_server_btn)!.Click +=
                (sender, args) => viewModel.HandleJoinLobbyButton(sender as View);
            viewModel.JoinLobbyButtonPressed += () => StartActivity(typeof(JoinServerActivity));


            FindViewById<Button>(Resource.Id.choose_game_btn)!.Click +=
                (sender, args) => viewModel.HandleCreateLobbyButton(sender as View);
            viewModel.LobbyCreateButtonPressed += () => StartActivity(typeof(ChooseGameActivity));
            

            FindViewById<EditText>(Resource.Id.nickname_tf)!.TextChanged +=
                (sender, args) => viewModel.Name = string.Concat(args.Text!);
        }
    }
}