using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Runtime;
using Android.Support.V7.RecyclerView.Extensions;
using Android.Support.V7.Util;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Shared.Protos;
using Shared.Utils;

namespace AndroidBlankApp1
{
    public class PlayersListAdapter : RecyclerView.Adapter
    {
        public event Action<Guid, string> PlayerRoleChanged;
        private readonly IReadOnlyList<string> _roles;
        private readonly bool _amHost;
        private IReadOnlyCollection<PlayerInLobby> _players;

        public IReadOnlyCollection<PlayerInLobby> Players
        {
            get => _players;
            set
            {
                _players = value;
                NotifyDataSetChanged();
            }
        }

        public PlayersListAdapter(IReadOnlyCollection<PlayerInLobby> players, IReadOnlyList<string> roles, bool AmHost)
        {
            _roles = roles;
            _amHost = AmHost;
            Players = players;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var viewHolder = holder as PlayerListViewHolder;
            var player = _players.ElementAt(position);
            viewHolder.PlayerName.Text = player.Name;
            viewHolder.RoleSelector.SetSelection(_roles.IndexOf(player.Role));
            viewHolder.PlayerId = player.Id;
            if (!_amHost)
                viewHolder.RoleSelector.Enabled = false;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var view = LayoutInflater.From(parent.Context)
                .Inflate(Resource.Layout.player_list_item, parent, false);
            var instance = new PlayerListViewHolder(view, _roles);
            instance.RoleUpdated += (guid, s) => PlayerRoleChanged?.Invoke(guid, s);
            return instance;
        }

        public override int ItemCount => _players.Count;
    }
}