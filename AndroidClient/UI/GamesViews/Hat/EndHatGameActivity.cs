using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Widget;
using AndroidClient.UI.GamesViews.Hat.GameScore;

namespace AndroidClient.UI.GamesViews.Hat
{
    // TODO add adequate finish handling with scoreboard created from lobby
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = false)]
    public class EndHatGameActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.end_hat_game);

            var scoreRecyclerView = FindViewById<RecyclerView>(Resource.Id.final_score)!;
            var scoreName = Intent?.GetStringArrayExtra("score_name")!;
            var scoreValue = Intent?.GetIntArrayExtra("score_value")!;
            var scoreAdapter =
                new HatGameScoreAdapter(scoreName.Zip(scoreValue, (name, value) => (Name: name, Value: value))
                    .ToList());
            scoreRecyclerView.SetAdapter(scoreAdapter);
            FindViewById<Button>(Resource.Id.to_lobby_btn)!.Click += (sender, args) => Finish();
        }

        public static void Launch(ICollection<(string Name, int Value)> lastScores, Action<Intent> start,
            Context context)
        {
            var intent = new Intent(context, typeof(EndHatGameActivity));
            intent.PutExtra("score_name", lastScores.Select(kv => kv.Name).ToArray());
            intent.PutExtra("score_value", lastScores.Select(kv => kv.Value).ToArray());
            start(intent);
        }
    }
}