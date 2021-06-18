using System;
using Android.Support.Design.Widget;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;

namespace AndroidClient.UI.GamesVIews.Hat.WordsChooser
{
    public class HatWordsChooserViewHolder : RecyclerView.ViewHolder
    {
        public event Action<int, string>? WordUpdated;
        public int Index { get; set; }
        public TextInputLayout WordInputLayout { get; }
        public TextInputEditText WordInputField { get; }

        public HatWordsChooserViewHolder(View itemView) : base(itemView)
        {
            Log.Info(nameof(HatWordsChooserViewHolder), "Creating viewHolder");
            WordInputLayout = itemView.FindViewById<TextInputLayout>(Resource.Id.word_input_lt)!;
            WordInputField = itemView.FindViewById<TextInputEditText>(Resource.Id.word_input_tf)!;
            WordInputField.TextChanged += (sender, args) =>
            {
                var textResult = string.Concat(args.Text!).Trim();
                WordInputLayout.Error = null;
                WordUpdated?.Invoke(Index, textResult);
            };
        }

        public void ChangeInputState(bool isEnabled) => WordInputField.Enabled = isEnabled;
        
    }
}