using System;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using AndroidBlankApp1.ViewModels.GameViewModels;
using Unity;

namespace AndroidBlankApp1
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = false)]
    public class HatWordPickerPairActivity : AppCompatActivity
    {
        private bool _canceled = false;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.hat_word_picker_pair);
            var viewModel = (Application as App).Container.Resolve<HatViewModel>();

            var wordTextView = FindViewById<TextView>(Resource.Id.word_to_guess);
            var scores = FindViewById<TextView>(Resource.Id.in_game_scores);
            scores!.Text = viewModel.LastScoresConcated;
            var btn = FindViewById<Button>(Resource.Id.guess_btn);
            var gameTimer = FindViewById<TextView>(Resource.Id.game_timer);
            wordTextView!.Text = viewModel.CurrentWord;
            viewModel.GetWord += () => RunOnUiThread(() => wordTextView!.Text = viewModel.CurrentWord);

            if (!viewModel.AmExplainer) btn!.Visibility = ViewStates.Gone;
            btn!.Click += async (sender, args) => await viewModel.GuessWord();

            viewModel.TimeIsUp += () =>
            {
                RunOnUiThread(() =>
                {
                    btn.Visibility = ViewStates.Gone;
                    Snackbar.Make(btn, "Time is up", 2000);
                });
            };

            viewModel.AnnouncedNextPair += () =>
            {
                StartActivity(typeof(HatPairChoosenActivity));
                Finish();
            };

            viewModel.ScoresUpdated += () => { RunOnUiThread(() => scores!.Text = viewModel.LastScoresConcated); };

            viewModel.GameOver += () =>
            {
                StartActivity(typeof(EndHatGameActivity));
                _canceled = true;
                Finish();
            };

            ExecuteEvery(TimeSpan.FromMilliseconds(100),
                () => RunOnUiThread(() => gameTimer!.Text = viewModel.TimerString));
        }

        public async void ExecuteEvery(TimeSpan duration, Action callback)
        {
            while (!_canceled)
            {
                await Task.Delay(duration);
                callback();
            }

            _canceled = false;
        }
    }
}