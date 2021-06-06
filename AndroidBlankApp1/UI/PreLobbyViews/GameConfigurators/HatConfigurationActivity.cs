using System;
using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using AndroidBlankApp1.UI.InLobbyViews;
using AndroidBlankApp1.ViewModels;
using Shared.Protos.HatSharedClasses;
using Unity;

namespace AndroidBlankApp1.UI.PreLobbyViews.GameConfigurators
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = false)]
    public class HatConfigurationActivity : AppCompatActivity
    {
        private PreLobbyViewModel _viewModel;
        private const int DefaultTimeToExplain = 25;
        private const int DefaultNumberOfWordsToBeWritten = 10;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.hat_configuration);
            _viewModel = (Application as App)!.Container.Resolve<PreLobbyViewModel>();
            FindViewById<Button>(Resource.Id.create_server_btn)!.Click +=
                async (sender, args) => await _viewModel.CreateLobby(sender as View);
            var config = new HatConfiguration();

            var timeInput = FindViewById<EditText>(Resource.Id.time_to_explain_field);
            var wordsInput = FindViewById<EditText>(Resource.Id.words_to_write_field);

            config.TimeToExplain = TimeSpan.FromSeconds(DefaultTimeToExplain);
            config.WordsToBeWritten = DefaultNumberOfWordsToBeWritten;
            config.HatGameModeConfiguration = new HatCircleChoosingModeConfiguration();

            timeInput!.Hint = DefaultTimeToExplain.ToString();
            wordsInput!.Hint = DefaultNumberOfWordsToBeWritten.ToString();

            _viewModel.Configuration = config;
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

        protected override void OnStart()
        {
            base.OnStart();
            _viewModel.JoinedLobby = OnViewModelShouldConfigureGame;
        }

        protected override void OnStop()
        {
            base.OnStop();
            _viewModel.JoinedLobby = null;
        }

        private void OnViewModelShouldConfigureGame() => StartActivity(typeof(LobbyActivity));
    }
}