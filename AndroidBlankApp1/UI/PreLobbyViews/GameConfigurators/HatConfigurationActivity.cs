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
        private static int _defaultTimeToExplain = 25;
        private static int _defaultNumberOfWordsToBeWrittem = 10;
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.hat_configuration);
            var viewModel = (Application as App)!.Container.Resolve<PreLobbyViewModel>();
            FindViewById<Button>(Resource.Id.create_server_btn)!.Click +=
                async (sender, args) => await viewModel.CreateLobby(sender as View);
            var config = new HatConfiguration();

            var timeInput = FindViewById<EditText>(Resource.Id.time_to_explain_field);
            var wordsInput = FindViewById<EditText>(Resource.Id.words_to_write_field);
            
            config.TimeToExplain = TimeSpan.FromSeconds(_defaultTimeToExplain);
            config.WordsToBeWritten = _defaultNumberOfWordsToBeWrittem;
            config.HatGameModeConfiguration = new HatCircleChoosingModeConfiguration();

            timeInput!.Hint = _defaultTimeToExplain.ToString();
            wordsInput!.Hint = _defaultNumberOfWordsToBeWrittem.ToString();
            
            viewModel.Configuration = config;
            viewModel.JoinedLobby += () => StartActivity(typeof(LobbyActivity));
            FindViewById<RadioGroup>(Resource.Id.hat_game_mode_choice)!.CheckedChange += (sender, args) =>
            {
                config.HatGameModeConfiguration = args.CheckedId switch
                {
                    // Resource.Id.hat_game_mode_choice_pairs => new HatPairChoosingModeConfiguration(),
                    Resource.Id.hat_game_mode_choice_circle => new HatCircleChoosingModeConfiguration(),
                    _ => throw new ArgumentException()
                };
            };
            timeInput!.TextChanged += (sender, args) =>
            {
                var str = string.Concat(args.Text!);
                if (str.Length == 0) return;
                config.TimeToExplain = TimeSpan.FromSeconds(int.Parse(str));
            };
            wordsInput!.TextChanged += (sender, args) =>
            {
                var str = string.Concat(args.Text!);
                if (str.Length == 0) return;
                config.WordsToBeWritten = int.Parse(str);
            };
        }
    }
}