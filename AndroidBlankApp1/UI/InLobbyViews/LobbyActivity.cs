using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidBlankApp1.ViewModels;
using AndroidX.RecyclerView.Widget;
using Java.Lang;
using Shared.Protos;
using Unity;
using RecyclerView = Android.Support.V7.Widget.RecyclerView;

namespace AndroidBlankApp1
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = false)]
    public class LobbyActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lobby);

            var viewModel = (Application as App)!.Container.Resolve<LobbyViewModel>();

            var startBtn = FindViewById<Button>(Resource.Id.start_game_btn);

            var idView = FindViewById<EditText>(Resource.Id.lobby_game_id);
            idView!.Text = viewModel.LobbyId.ToString();
            idView.Click += (sender, args) =>
            {
                Log.Info(nameof(LobbyActivity), "Copying GUID");
                var clipboard = (ClipboardManager) GetSystemService(ClipboardService);
                var clip = ClipData.NewPlainText(viewModel.LobbyId.ToString(), viewModel.LobbyId.ToString());
                clipboard.PrimaryClip = clip;
                Snackbar.Make(idView, "Copied to clipboard", 2000).Show();
            };
            Log.Info(nameof(LobbyActivity), "GUID IS " + idView!.Text);

            if (viewModel.AmHost.HasValue && !viewModel.AmHost.Value)
            {
                Log.Info(nameof(LobbyActivity), "HIDING BUTTON CUZ U R LOX XD");
                Log.Info(nameof(LobbyActivity), viewModel.AmHost.ToString());
                startBtn!.Visibility = ViewStates.Gone;
            }

            startBtn!.Click +=
                async (sender, args) =>
                {
                    Log.Info(nameof(LobbyActivity), "StartBtnPressed");
                    await viewModel.HandleGameStartButtonPressing(sender as View);
                };

            var recyclerView = FindViewById<RecyclerView>(Resource.Id.recycler);

            var adapter = new PlayersListAdapter(viewModel.LastLobbyStatus, viewModel.Roles, viewModel.AmHost!.Value);
            viewModel.LobbyUpdate += () =>
            {
                Log.Info(nameof(LobbyActivity), $"Got lobby update {viewModel.LastLobbyStatus.Count}");
                RunOnUiThread(() => adapter.Players = viewModel.LastLobbyStatus);
            };

            viewModel.GameStarted += () =>
            {
                Log.Info(nameof(LobbyActivity), "Got game started");
                StartActivity(ActivityForGameType.GameTypeToActivity[viewModel.LobbyGameType]);
            };

            adapter.PlayerRoleChanged += async (guid, s) =>
            {
                Log.Info(nameof(LobbyActivity), "Got player role changed");
                await viewModel.HandlePlayerRoleChange(guid, s);
            };

            recyclerView!.SetAdapter(adapter);
            viewModel.StartProcessingEvents(idView);
        }
    }
}