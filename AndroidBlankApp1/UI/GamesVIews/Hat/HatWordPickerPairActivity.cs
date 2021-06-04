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
        private bool _canceledTimer = false;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.hat_word_picker_pair);
            var viewModel = (Application as App).Container.Resolve<HatViewModel>();

            var wordTextView = FindViewById<TextView>(Resource.Id.word_to_guess);
            var scores = FindViewById<TextView>(Resource.Id.in_game_scores);
            scores!.Text = viewModel.LastScoresConcated;
            var guesBtn = FindViewById<Button>(Resource.Id.guess_btn);
            var cancelBtn = FindViewById<Button>(Resource.Id.cancel_btn);
            var gameTimer = FindViewById<TextView>(Resource.Id.game_timer);
            wordTextView!.Text = viewModel.CurrentWord;
            viewModel.GetWord = () => RunOnUiThread(() => wordTextView!.Text = viewModel.CurrentWord);

            if (!viewModel.AmControllingExplanation)
            {
                guesBtn!.Visibility = ViewStates.Gone;
                cancelBtn!.Visibility = ViewStates.Gone;
            }
            
            guesBtn!.Click += async (sender, args) => await viewModel.GuessWord();
            cancelBtn!.Click += async (sender, args) => await viewModel.CancelExplanation();

            viewModel.TimeIsUp = () =>
            {
                RunOnUiThread(() => 
                {
                    guesBtn.Visibility = ViewStates.Gone;
                    Snackbar.Make(guesBtn, "Time is up", 2000);
                });
            };

            viewModel.AnnouncedNextPair = () =>
            {
                StartActivity(typeof(HatPairChoosenActivity));
                Finish();
            };

            viewModel.ScoresUpdated = () => { RunOnUiThread(() => scores!.Text = viewModel.LastScoresConcated); };

            viewModel.GameOver = () =>
            {
                StartActivity(typeof(EndHatGameActivity));
                _canceledTimer = true;
                Finish();
            };

            ExecuteEvery(TimeSpan.FromMilliseconds(100),
                () => RunOnUiThread(() => gameTimer!.Text = viewModel.TimerString));
        }

        public async void ExecuteEvery(TimeSpan duration, Action callback)
        {
            while (!_canceledTimer)
            {
                await Task.Delay(duration);
                callback();
            }

            _canceledTimer = false;
        }
    }
}