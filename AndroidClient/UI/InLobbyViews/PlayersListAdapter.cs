using System;
using System.Collections.Generic;
using System.Linq;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Views;
using Shared.Protos;
using Shared.Utils;

namespace AndroidClient.UI.InLobbyViews
{
    public class PlayersListAdapter : RecyclerView.Adapter
    {
        public event Action<Guid, string>? PlayerRoleChanged;
        private readonly IReadOnlyList<string> _roles;
        private readonly bool _amHost;
        private IReadOnlyCollection<PlayerInLobby>? _players;

        public IReadOnlyCollection<PlayerInLobby>? Players
        {
            set
            {
                _players = value;
                NotifyDataSetChanged();
            }
        }

        public PlayersListAdapter(IReadOnlyCollection<PlayerInLobby>? players, IReadOnlyList<string> roles, bool amHost)
        {
            _roles = roles;
            _amHost = amHost;
            Players = players;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var viewHolder = holder as PlayerListViewHolder;
            var player = _players!.ElementAt(position);
            viewHolder!.PlayerName.Text = player.Name;
            viewHolder!.RoleSelector.SetSelection(_roles.IndexOf(player.Role));
            viewHolder!.PlayerId = player.Id;
            viewHolder!.ReadyCheck.Checked = player.IsReady;
            if (!_amHost)
            {
                viewHolder!.RoleSelector.Enabled = false;
                viewHolder!.RoleSelector.Background!.SetColorFilter(new PorterDuffColorFilter(Color.Transparent, PorterDuff.Mode.Clear!));
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var view = LayoutInflater.From(parent.Context)
                ?.Inflate(Resource.Layout.player_list_item, parent, false);
            var instance = new PlayerListViewHolder(view!, _roles);
            instance.RoleUpdated += (guid, s) => PlayerRoleChanged?.Invoke(guid, s);
            return instance;
        }

        public override int ItemCount => _players?.Count ?? 0;
    }
}