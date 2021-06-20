using Android.Support.Design.Widget;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;

namespace AndroidClient.UI.GamesViews.Hat.GameScore
{
    public class HatGameScoreViewHolder : RecyclerView.ViewHolder
    {
        public TextInputLayout ScoreInputLayout { get; }
        public TextInputEditText ScoreInputField { get; }

        public HatGameScoreViewHolder(View itemView) : base(itemView)
        {
            Log.Info(nameof(HatGameScoreViewHolder), "Creating viewHolder");
            ScoreInputLayout = itemView.FindViewById<TextInputLayout>(Resource.Id.score_lt)!;
            ScoreInputField = itemView.FindViewById<TextInputEditText>(Resource.Id.score_tf)!;
        }
    }
}