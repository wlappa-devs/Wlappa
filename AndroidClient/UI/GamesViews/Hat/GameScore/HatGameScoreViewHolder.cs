using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace AndroidClient.UI.GamesViews.Hat.GameScore
{
    public class HatGameScoreViewHolder : RecyclerView.ViewHolder
    {
        public TextView ScoreInputLayout { get; }
        public TextView ScoreInputField { get; }

        public HatGameScoreViewHolder(View itemView) : base(itemView)
        {
            Log.Info(nameof(HatGameScoreViewHolder), "Creating viewHolder");
            ScoreInputLayout = itemView.FindViewById<TextView>(Resource.Id.score_lt)!;
            ScoreInputField = itemView.FindViewById<TextView>(Resource.Id.score_tf)!;
        }
    }
}