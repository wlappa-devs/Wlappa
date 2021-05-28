using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using AndroidBlankApp1.ViewModels.GameViewModels;
using Unity;

namespace AndroidBlankApp1
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = false)]
    public class HatPairChoosenActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            var viewModel = (Application as App)!.Container.Resolve<HatViewModel>();
            SetContentView(Resource.Layout.hat_pair_choosen);

            FindViewById<TextView>(Resource.Id.explainer_name)!.Text = viewModel.GetName(viewModel.Explainer);
            FindViewById<TextView>(Resource.Id.understander_name)!.Text = viewModel.GetName(viewModel.Understander);
            var btn = FindViewById<Button>(Resource.Id.start_explaining_btn);
            var scores = FindViewById<TextView>(Resource.Id.scores);

            scores!.Text = viewModel.LastScoresConcated;

            if (!viewModel.AmInPair)
            {
                btn!.Visibility = ViewStates.Gone;
            }

            btn!.Click += async (sender, args) =>
            {
                RunOnUiThread(() => btn.Visibility = ViewStates.Gone);
                await viewModel.GetReady(sender, args);
            };
            viewModel.StartExplanation += () =>
            {
                StartActivity(typeof(HatWordPickerPairActivity));
                Finish();
            };
            
            viewModel.ScoresUpdated += () => RunOnUiThread(() => scores!.Text = viewModel.LastScoresConcated);
            
            viewModel.GameOver += () =>
            {
                StartActivity(typeof(EndHatGameActivity));
                Finish();
            };
        }
    }
}