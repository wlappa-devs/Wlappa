using System.Collections.Generic;
using Android.Support.V7.Widget;
using Android.Views;

namespace AndroidClient.UI.GamesViews.Hat.GameScore
{
    public class HatGameScoreAdapter : RecyclerView.Adapter
    {
        private List<(string Name, int Value)>? _score;

        public List<(string Name, int Value)>? Score
        {
            get => _score;
            set
            {
                _score = value;
                NotifyDataSetChanged();
            }
        }

        public HatGameScoreAdapter(List<(string Name, int Value)> score) => Score = score;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var viewHolder = holder as HatGameScoreViewHolder;
            viewHolder!.ScoreInputLayout.Text = Score![position].Name;
            viewHolder!.ScoreInputField.Text = Score![position].Value.ToString();
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var view = LayoutInflater.From(parent.Context)
                ?.Inflate(Resource.Layout.score_list_item, parent, false);
            var instance = new HatGameScoreViewHolder(view!);
            return instance;
        }

        public override int ItemCount => Score?.Count ?? 0;
    }
}