using System;
using System.Collections;
using System.Collections.Generic;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;

namespace AndroidClient.UI.GamesVIews.Hat.WordsChooser
{
    public class HatWordsChooserAdapter : RecyclerView.Adapter
    {
        public event Action<int, string>? WordErrored;
        private string[] _words;

        public string[] Words
        {
            set
            {
                _words = value;
                NotifyDataSetChanged();
            }
        }

        public HatWordsChooserAdapter(string[] words)
        {
            Words = words;
        }
        
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var viewHolder = holder as HatWordsChooserViewHolder;
            viewHolder!.Index = position;
            viewHolder!.WordInputLayout.Hint = $"Word {position + 1}";
            WordErrored += (index, errorText) =>
            {
                if (index == position)
                    viewHolder.WordInputLayout.Error = errorText;
            };
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            Log.Info(nameof(HatWordsChooserViewHolder), "Creating viewHolder in Adapter");
            var view = LayoutInflater.From(parent.Context)
                ?.Inflate(Resource.Layout.words_list_item, parent, false);
            var instance = new HatWordsChooserViewHolder(view!);
            instance.WordUpdated += (index, word) => _words[index] = word;
            return instance;
        }

        public void SendErroredWords(ICollection<int> words)
        {
            Log.Info(nameof(HatWordsChooserAdapter), $"Incorrect words: {string.Join(", ", words)}");
            foreach (var index in words) 
                WordErrored!.Invoke(index, "Incorrect word");
        }

        public override int ItemCount => _words.Length;
    }
}