using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;

namespace AndroidBlankApp1
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = false)]
    public class HatPairChoosenActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.hat_pair_choosen);
            // FindViewById<Button>(Resource.Id.start_explaining_btn).Click += 
            //     (sender, args) => StartActivity(typeof(HatWordPickerPairActivity));
            FindViewById<Button>(Resource.Id.start_explaining_btn).Click += 
                (sender, args) => StartActivity(typeof(HatWordPickerRestActivity));
        }
    }
}