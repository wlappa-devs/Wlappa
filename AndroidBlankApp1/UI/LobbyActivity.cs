using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;

namespace AndroidBlankApp1
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = false)]
    public class LobbyActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lobby);
            FindViewById<Button>(Resource.Id.start_game_btn).Click +=
                (sender, args) => StartActivity(typeof(HatWordsChooserActivity));
        }
    }
}