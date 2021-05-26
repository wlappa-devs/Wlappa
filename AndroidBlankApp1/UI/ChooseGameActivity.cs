using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;

namespace AndroidBlankApp1
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = false)]
    public class ChooseGameActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.choose_game);
            FindViewById<Button>(Resource.Id.next_to_configure_game_btn).Click +=
                (sender, args) => StartActivity(typeof(CreateServerActivity));
        }
    }
}