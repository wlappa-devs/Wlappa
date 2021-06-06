using Android.App;
using Android.OS;
using Android.Support.V7.App;

namespace AndroidBlankApp1.UI.GamesViews.Hat
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = false)]
    public class HatWordPickerRestActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.hat_word_picker_rest);
        }
    }
}