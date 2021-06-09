using System;
using System.Collections.Generic;
using System.Linq;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace AndroidClient.UI.InLobbyViews
{
    public class PlayerListViewHolder : RecyclerView.ViewHolder
    {
        public event Action<Guid, string>? RoleUpdated;
        public Guid PlayerId { get; set; }
        public TextView PlayerName { get; private set; }
        public Spinner RoleSelector { get; private set; }

        // public PlayerListViewHolder(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        // {
        // }

        public PlayerListViewHolder(View itemView, IReadOnlyList<string> roles) : base(itemView)
        {
            Log.Info(nameof(PlayerListViewHolder), "Creating viewHolder");
            PlayerName = itemView.FindViewById<TextView>(Resource.Id.player_name)!;
            RoleSelector = itemView.FindViewById<Spinner>(Resource.Id.role_selector)!;
            RoleSelector!.Adapter = new ArrayAdapter(itemView.Context!,
                Resource.Layout.support_simple_spinner_dropdown_item, roles.ToArray());
            RoleSelector.ItemSelected += (sender, args) => RoleUpdated?.Invoke(PlayerId, roles[args.Position]);
        }
    }
}