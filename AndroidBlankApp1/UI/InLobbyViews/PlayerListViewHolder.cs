using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidBlankApp1.ViewModels;
using Unity;

namespace AndroidBlankApp1
{
    public class PlayerListViewHolder : RecyclerView.ViewHolder
    {
        public event Action<Guid, string> RoleUpdated;
        public Guid PlayerId { get; set; }
        public TextView PlayerName { get; private set; }
        public Spinner RoleSelector { get; private set; }

        public PlayerListViewHolder(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public PlayerListViewHolder(View itemView, IReadOnlyList<String> roles) : base(itemView)
        {
            Log.Info(nameof(PlayerListViewHolder), "Creating viewholder");
            PlayerName = itemView.FindViewById<TextView>(Resource.Id.player_name);
            RoleSelector = itemView.FindViewById<Spinner>(Resource.Id.role_selector);
            RoleSelector!.Adapter = new ArrayAdapter(itemView.Context!,
                Resource.Layout.support_simple_spinner_dropdown_item, roles.ToArray());
            RoleSelector.ItemSelected += (sender, args) => RoleUpdated?.Invoke(PlayerId, roles[args.Position]);
        }
    }
}