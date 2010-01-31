using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using NetGore.Graphics.GUI;

namespace NetGore.Features.Guilds
{
    public class GuildOnlineMembersForm : GuildMembersFormBase
    {
        readonly UserGuildInformation.UserGuildInformationEventHandler<string> _updateHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="GuildOnlineMembersForm"/> class.
        /// </summary>
        /// <param name="parent">Parent <see cref="Control"/> of this <see cref="Control"/>.</param>
        /// <param name="position">Position of the Control reletive to its parent.</param>
        /// <param name="clientSize">The size of the <see cref="Control"/>'s client area.</param>
        /// <exception cref="NullReferenceException"><paramref name="parent"/> is null.</exception>
        public GuildOnlineMembersForm(Control parent, Vector2 position, Vector2 clientSize) : base(parent, position, clientSize)
        {
            _updateHandler = (x, y) => UpdateCache();
        }

        /// <summary>
        /// Sets the default values for the <see cref="Control"/>. This should always begin with a call to the
        /// base class's method to ensure that changes to settings are hierchical.
        /// </summary>
        protected override void SetDefaultValues()
        {
            base.SetDefaultValues();

            Text = "Online Guild Members";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GuildOnlineMembersForm"/> class.
        /// </summary>
        /// <param name="guiManager">The GUI manager this <see cref="Control"/> will be managed by.</param>
        /// <param name="position">Position of the Control reletive to its parent.</param>
        /// <param name="clientSize">The size of the <see cref="Control"/>'s client area.</param>
        /// <exception cref="ArgumentNullException"><paramref name="guiManager"/> is null.</exception>
        public GuildOnlineMembersForm(IGUIManager guiManager, Vector2 position, Vector2 clientSize)
            : base(guiManager, position, clientSize)
        {
            _updateHandler = (x, y) => UpdateCache();
        }

        /// <summary>
        /// When overridden in the derived class, gets the items to display in the list.
        /// </summary>
        /// <returns>The items to display in the list.</returns>
        protected override IEnumerable<GuildMemberNameRank> GetListItems(UserGuildInformation guildInfo)
        {
            return guildInfo.OnlineMembers;
        }

        /// <summary>
        /// When overridden in the derived class, handles when the <see cref="UserGuildInformation"/> changes.
        /// </summary>
        /// <param name="newValue">The new value.</param>
        /// <param name="oldValue">The old value.</param>
        protected override void HandleChangeGuild(UserGuildInformation newValue, UserGuildInformation oldValue)
        {
            base.HandleChangeGuild(newValue, oldValue);

            if (oldValue != null)
            {
                oldValue.OnAddOnlineMember -= _updateHandler;
                oldValue.OnRemoveOnlineMember -= _updateHandler;
            }

            if (newValue != null)
            {
                newValue.OnAddOnlineMember += _updateHandler;
                newValue.OnRemoveOnlineMember += _updateHandler;
            }
        }
    }
}