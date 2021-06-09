using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;

namespace AndroidClient.UI.GamesVIews.Hat
{
    // TODO add adequate finish handling with scoreboard created from lobby
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = false)]
    public class EndHatGameActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.end_hat_game);

            var score = FindViewById<TextView>(Resource.Id.final_scores);
            score!.Text = Intent?.GetStringExtra("scores");
            FindViewById<Button>(Resource.Id.to_lobby_btn)!.Click += (sender, args) => Finish();
        }

        public static void Launch(string lastScoresConcatenated, Action<Intent> start, Context context)
        {
            var intent = new Intent(context, typeof(EndHatGameActivity));
            intent.PutExtra("scores", lastScoresConcatenated);
            start(intent);
        }
    }
}