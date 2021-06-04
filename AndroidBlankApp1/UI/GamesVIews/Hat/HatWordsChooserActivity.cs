using Android.App;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using AndroidBlankApp1.ViewModels.GameViewModels;
using Unity;

namespace AndroidBlankApp1
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = false)]
    public class HatWordsChooserActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.hat_words_chooser);
            var viewModel = (Application as App)!.Container.Resolve<HatViewModel>();

            var addWordsButton = FindViewById<Button>(Resource.Id.start_choose_pairs_btn);
            var wordsTextInput = FindViewById<EditText>(Resource.Id.words_input);
            var numberOfPlayersRemaining = FindViewById<TextView>(Resource.Id.number_of_players_ready);

            if (viewModel.MyRole != Shared.Protos.HatSharedClasses.HatRolePlayer.Value)
            {
                addWordsButton!.Visibility = ViewStates.Gone;
                wordsTextInput!.Visibility = ViewStates.Gone;
            }
            
            addWordsButton!.Click +=
                async (sender, args) => await viewModel.SendWords(sender as View);
            

            viewModel.WordsSuccessfullyAddedByMe = () =>
            {
                RunOnUiThread(() =>
                {
                    addWordsButton.Visibility = ViewStates.Gone;
                    wordsTextInput!.Enabled = false;
                });
            };
            viewModel.WordsSuccessfullyAddedBySomeOne = () =>
            {
                RunOnUiThread(() => numberOfPlayersRemaining!.Text = viewModel.RemainingPlayersToWriteWords.ToString());
            };
            viewModel.AnnouncedNextPair = () =>
            {
                StartActivity(typeof(HatPairChoosenActivity));
                Finish();
            };
            viewModel.InvalidWordSet = () =>
            {
                RunOnUiThread(() =>
                {
                    addWordsButton.Visibility = ViewStates.Visible;
                    wordsTextInput!.Enabled = true;
                    Snackbar.Make(addWordsButton, "Invalid word set", 2000).Show();
                });
            };

            wordsTextInput!.TextChanged += (sender, args) =>
                viewModel.WordsInput = string.Concat(args.Text!);
        }
    }
}