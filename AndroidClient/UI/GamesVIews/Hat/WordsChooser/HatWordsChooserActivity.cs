using System.Collections.Generic;
using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using AndroidClient.ViewModels.GameViewModels;
using Unity;

namespace AndroidClient.UI.GamesVIews.Hat.WordsChooser
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = false, WindowSoftInputMode = SoftInput.AdjustResize)]
    public class HatWordsChooserActivity : AppCompatActivity
    {
        private HatViewModel _viewModel = null!;
        private Button _addWordsButton = null!;
        private HatWordsChooserAdapter _wordsAdapter = null! ;
        private TextView _numberOfPlayersRemaining = null!;
        private RecyclerView _wordsRecyclerView = null!;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.hat_words_chooser);
            _viewModel = (Application as App)!.Container.Resolve<HatViewModel>();

            _addWordsButton = FindViewById<Button>(Resource.Id.start_choose_pairs_btn)!;
            _wordsAdapter = new HatWordsChooserAdapter(_viewModel.WordsInput!);
            _numberOfPlayersRemaining = FindViewById<TextView>(Resource.Id.number_of_players_ready)!;
            _wordsRecyclerView = FindViewById<RecyclerView>(Resource.Id.words_input_recycler);
            
            if (_viewModel.MyRole != Shared.Protos.HatSharedClasses.HatRolePlayer.Value)
            {
                _addWordsButton.Visibility = ViewStates.Gone;
                _wordsRecyclerView!.Visibility = ViewStates.Gone;
            }

            _addWordsButton.Click +=
                async (sender, args) => await _viewModel.SendWords();


            _numberOfPlayersRemaining.Text = _viewModel.RemainingPlayersToWriteWords.ToString();
            _wordsRecyclerView!.SetAdapter(_wordsAdapter);
        }

        private void OnViewModelWordsSuccessfullyAddedByMe()
        {
            RunOnUiThread(() =>
            {
                _addWordsButton.Visibility = ViewStates.Gone;
                _wordsRecyclerView.Enabled = false;
            });
        }

        private void OnViewModelWordsSuccessfullyAddedBySomeOne()
        {
            RunOnUiThread(() =>
            {
                _numberOfPlayersRemaining!.Text = _viewModel.RemainingPlayersToWriteWords.ToString();
            });
        }

        private void OnViewModelAnnouncedNextPair()
        {
            StartActivity(typeof(HatPairChosenActivity));
            Finish();
        }

        private void OnViewModelInvalidWordSet(ICollection<int> invalidWords)
        {
            RunOnUiThread(() =>
            {
                _addWordsButton.Visibility = ViewStates.Visible;
                _wordsRecyclerView.Enabled = true;
                _wordsAdapter.SendErroredWords(invalidWords);
            });
        }

        protected override void OnStart()
        {
            base.OnStart();
            _viewModel.WordsSuccessfullyAddedBySomeOne += OnViewModelWordsSuccessfullyAddedBySomeOne;
            _viewModel.AnnouncedNextPair += OnViewModelAnnouncedNextPair;
            _viewModel.InvalidWordSet += OnViewModelInvalidWordSet;
            _viewModel.WordsSuccessfullyAddedByMe += OnViewModelWordsSuccessfullyAddedByMe;
        }

        protected override void OnStop()
        {
            base.OnStop();
            _viewModel.WordsSuccessfullyAddedBySomeOne -= OnViewModelWordsSuccessfullyAddedBySomeOne;
            _viewModel.AnnouncedNextPair -= OnViewModelAnnouncedNextPair;
            _viewModel.InvalidWordSet -= OnViewModelInvalidWordSet;
            _viewModel.WordsSuccessfullyAddedByMe -= OnViewModelWordsSuccessfullyAddedByMe;
        }
    }
}