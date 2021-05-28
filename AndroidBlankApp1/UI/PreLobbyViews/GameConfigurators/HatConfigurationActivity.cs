using System;
using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using AndroidBlankApp1.ViewModels;
using Shared.Protos;
using Shared.Protos.HatSharedClasses;
using Unity;

namespace AndroidBlankApp1
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = false)]
    public class HatConfigurationActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.hat_configuration);
            var viewModel = (Application as App)!.Container.Resolve<PreLobbyViewModel>();
            FindViewById<Button>(Resource.Id.create_server_btn)!.Click +=
                async (sender, args) => await viewModel.CreateLobby(sender as View);
            var config = new HatConfiguration();
            viewModel.Configuration = config;
            viewModel.JoinedLobby += () => StartActivity(typeof(LobbyActivity));
            FindViewById<RadioGroup>(Resource.Id.hat_game_mode_choice)!.CheckedChange += (sender, args) =>
            {
                config.HatGameModeConfiguration = args.CheckedId switch
                {
                    Resource.Id.hat_game_mode_choice_pairs => new HatPairChoosingModeConfiguration(),
                    Resource.Id.hat_game_mode_choice_circle => new HatCircleChoosingModeConfiguration(),
                    _ => throw new ArgumentException()
                };
            };
            FindViewById<EditText>(Resource.Id.time_to_explain_field)!.TextChanged += (sender, args) =>
            {
                var str = string.Concat(args.Text!);
                if (str.Length == 0) return;
                config.TimeToExplain = TimeSpan.FromSeconds(int.Parse(str));
            };
            FindViewById<EditText>(Resource.Id.words_to_write_field)!.TextChanged += (sender, args) =>
            {
                var str = string.Concat(args.Text!);
                if (str.Length == 0) return;
                config.WordsToBeWritten = int.Parse(str);
            };
        }
    }
}