using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;
using AndroidBlankApp1.ViewModels.GameViewModels;
using Unity;

namespace AndroidBlankApp1
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = false)]
    public class EndHatGameActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.end_hat_game);
            var viewModel = (Application as App).Container.Resolve<HatViewModel>();
            
            var score = FindViewById<TextView>(Resource.Id.final_scores);
            score!.Text = viewModel.LastScoresConcated;
            FindViewById<Button>(Resource.Id.to_lobby_btn).Click += 
                (sender, args) => Finish();
        }
    }
}