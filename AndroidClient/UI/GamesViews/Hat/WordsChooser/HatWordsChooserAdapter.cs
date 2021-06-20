using System;
using System.Collections.Generic;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;

namespace AndroidClient.UI.GamesViews.Hat.WordsChooser
{
    public class HatWordsChooserAdapter : RecyclerView.Adapter
    {
        private event Action<int, string>? WordErrored;
        private event Action? InputLocked;
        private event Action? InputUnlocked;
        private string[]? _words;
        private readonly Dictionary<int, string> _positionToError = new Dictionary<int, string>();
        private bool _doingBinding;

        public string[]? Words
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
            Log.Info(nameof(HatWordsChooserViewHolder), $"Position: {position}, Error: {_positionToError.GetValueOrDefault(position)}");
            _doingBinding = true;
            var viewHolder = holder as HatWordsChooserViewHolder;
            viewHolder!.Index = position;
            viewHolder!.WordInputField.Text = _words![position];
            viewHolder!.WordInputLayout.Hint = $"Word {position + 1}";
            viewHolder!.WordInputLayout.Error = _positionToError.GetValueOrDefault(position);
            _doingBinding = false;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            Log.Info(nameof(HatWordsChooserViewHolder), "Creating viewHolder in Adapter");
            var view = LayoutInflater.From(parent.Context)
                ?.Inflate(Resource.Layout.words_list_item, parent, false);
            var instance = new HatWordsChooserViewHolder(view!);
            instance.WordUpdated += (index, word) =>
            {
                if (_doingBinding) return;
                _words![index] = word;
                instance.WordInputLayout.Error = null;
                _positionToError.Remove(index);
            };
            //TODO POSSIBLE MEMORY LEAK
            InputLocked += () => instance.ChangeInputState(false);
            InputUnlocked += () => instance.ChangeInputState(true);
            WordErrored += (index, errorText) =>
            {
                if (index == instance.Index)
                    instance.WordInputLayout.Error = errorText;
            };
            return instance;
        }

        public void SendErroredWords(IReadOnlyCollection<int> words)
        {
            Log.Info(nameof(HatWordsChooserAdapter), $"Incorrect words: {string.Join(", ", words)}");
            const string errorText = "Incorrect word";
            _positionToError.Clear();
            foreach (var index in words)
            {
                WordErrored!.Invoke(index, errorText);
                _positionToError[index] = errorText;
            }
        }

        public void LockInput() => InputLocked?.Invoke();
        public void UnlockInput() => InputUnlocked?.Invoke();
        public override int ItemCount => _words?.Length ?? 0;
    }
}