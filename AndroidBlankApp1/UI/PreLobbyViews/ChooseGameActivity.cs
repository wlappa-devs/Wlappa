using System;
using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidBlankApp1.ViewModels;
using Unity;

namespace AndroidBlankApp1
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = false)]
    public class ChooseGameActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.choose_game);
            var app = Application as App;
            var viewModel = app!.Container.Resolve<PreLobbyViewModel>();

            FindViewById<Button>(Resource.Id.next_to_configure_game_btn)!.Click +=
                (sender, args) => viewModel.HandleGameSelected(sender as View);

            viewModel.JoinedLobby += () =>
            {
                // TODO move to event handling
                var radioGroup = FindViewById<RadioGroup>(Resource.Id.game_choosing_group);
                var selected = radioGroup?.CheckedRadioButtonId;
                Log.Info(nameof(ChooseGameActivity), selected.ToString());
                switch (selected)
                {
                    case Resource.Id.game_choice_hat:
                        StartActivity(typeof(HatConfigurationActivity));
                        break;
                    default:
                        throw new NotImplementedException();
                }
            };
        }
    }
}