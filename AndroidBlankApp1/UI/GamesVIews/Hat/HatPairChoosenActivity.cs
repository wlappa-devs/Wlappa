using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidBlankApp1.ViewModels.GameViewModels;
using Unity;

namespace AndroidBlankApp1.UI.GamesViews.Hat
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = false)]
    public class HatPairChoosenActivity : AppCompatActivity
    {
        private HatViewModel _viewModel;
        private TextView? _scores;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            _viewModel = (Application as App)!.Container.Resolve<HatViewModel>();
            SetContentView(Resource.Layout.hat_pair_choosen);

            FindViewById<TextView>(Resource.Id.explainer_name)!.Text = _viewModel.GetName(_viewModel.Explainer);
            FindViewById<TextView>(Resource.Id.understander_name)!.Text = _viewModel.GetName(_viewModel.Understander);
            var btn = FindViewById<Button>(Resource.Id.start_explaining_btn);
            _scores = FindViewById<TextView>(Resource.Id.scores);

            _scores!.Text = _viewModel.LastScoresConcated;

            if (!_viewModel.AmInPair)
            {
                btn!.Visibility = ViewStates.Gone;
            }

            btn!.Click += async (sender, args) =>
            {
                RunOnUiThread(() => btn.Visibility = ViewStates.Gone);
                await _viewModel.GetReady(sender, args);
            };
        }

        private void OnViewModelStartExplanation()
        {
            StartActivity(typeof(HatWordPickerPairActivity));
            Finish();
        }

        private void OnViewModelGameOver()
        {
            StartActivity(typeof(EndHatGameActivity));
            Finish();
        }

        private void OnViewModelScoresUpdated()
        {
            RunOnUiThread(() => _scores!.Text = _viewModel.LastScoresConcated);
        }

        protected override void OnStart()
        {
            Log.Info(nameof(HatPairChoosenActivity),"Started");
            base.OnStart();
            _viewModel.StartExplanation += OnViewModelStartExplanation;
            _viewModel.ScoresUpdated += OnViewModelScoresUpdated;
            _viewModel.GameOver += OnViewModelGameOver;
        }

        protected override void OnStop()
        {
            Log.Info(nameof(HatPairChoosenActivity),"Stopped");
            base.OnStop();
            _viewModel.StartExplanation -= OnViewModelStartExplanation;
            _viewModel.ScoresUpdated -= OnViewModelScoresUpdated;
            _viewModel.GameOver -= OnViewModelGameOver;
        }
    }
}