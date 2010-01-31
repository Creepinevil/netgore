﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using log4net;
using NetGore.IO;

namespace NetGore.Features.Guilds
{
    /// <summary>
    /// Contains the guild information for the client, and methods the server can use to send the data to the client.
    /// </summary>
    public class UserGuildInformation
    {
        /// <summary>
        /// Delegate for handling events from the <see cref="UserGuildInformation"/>.
        /// </summary>
        /// <param name="sender">The <see cref="UserGuildInformation"/> the event came from.</param>
        /// <param name="member">The guild member the event is related to.</param>
        public delegate void UserGuildInformationEventHandler<T>(UserGuildInformation sender, T member);

        /// <summary>
        /// Delegate for handling events from the <see cref="UserGuildInformation"/>.
        /// </summary>
        /// <param name="sender">The <see cref="UserGuildInformation"/> the event came from.</param>
        public delegate void UserGuildInformationEventHandler(UserGuildInformation sender);

        static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        readonly List<GuildMemberNameRank> _members = new List<GuildMemberNameRank>();
        readonly List<string> _onlineMembers = new List<string>();

        bool _inGuild = false;

        /// <summary>
        /// Notifies listeners when a guild member has been added.
        /// </summary>
        public event UserGuildInformationEventHandler<GuildMemberNameRank> OnAddMember;

        /// <summary>
        /// Notifies listeners when an offline guild member has come online.
        /// </summary>
        public event UserGuildInformationEventHandler<string> OnAddOnlineMember;

        /// <summary>
        /// Notifies listeners when the guild has changed. This can be either the user leaving a guild, joining a new
        /// guild, or having the initial guild being set.
        /// </summary>
        public event UserGuildInformationEventHandler OnChangeGuild;

        /// <summary>
        /// Notifies listeners when a guild member has been removed.
        /// </summary>
        public event UserGuildInformationEventHandler<string> OnRemoveMember;

        /// <summary>
        /// Notifies listeners when an online guild member has gone offline.
        /// </summary>
        public event UserGuildInformationEventHandler<string> OnRemoveOnlineMember;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserGuildInformation"/> class.
        /// </summary>
        public UserGuildInformation()
        {
            Name = string.Empty;
            Tag = string.Empty;
            InGuild = false;
        }

        /// <summary>
        /// Gets if the client is in a guild at all.
        /// </summary>
        public bool InGuild
        {
            get { return _inGuild; }
            set
            {
                if (_inGuild == value)
                    return;

                _inGuild = value;

                if (!_inGuild)
                {
                    Name = string.Empty;
                    Tag = string.Empty;
                    _members.Clear();
                    _onlineMembers.Clear();
                }
            }
        }

        /// <summary>
        /// Gets all of the guild members. If the user is not in a guild, this will be empty.
        /// </summary>
        public IEnumerable<GuildMemberNameRank> Members
        {
            get
            {
                if (!InGuild)
                    return Enumerable.Empty<GuildMemberNameRank>();

                return _members;
            }
        }

        /// <summary>
        /// Gets the name of the guild, or an empty string if not in a guild.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the online members in the guild. If the user is not in a guild, this will be empty.
        /// </summary>
        public IEnumerable<GuildMemberNameRank> OnlineMembers
        {
            get
            {
                if (InGuild)
                {
                    for (int i = 0; i < _members.Count; i++)
                    {
                        if (_onlineMembers.Contains(_members[i].Name))
                            yield return _members[i];
                    }
                }
            }
        }

        /// <summary>
        /// Gets the guild's tag, or an empty string if not in a guild.
        /// </summary>
        public string Tag { get; private set; }

        /// <summary>
        /// Reads the data from the server related to the user guild information. This should only be used by the client.
        /// </summary>
        /// <param name="bitStream">The <see cref="BitStream"/> containing the data.</param>
        public void Read(BitStream bitStream)
        {
            var id = bitStream.ReadEnum<GuildInfoMessages>();
            switch (id)
            {
                case GuildInfoMessages.SetGuild:
                    ReadSetGuild(bitStream);
                    return;

                case GuildInfoMessages.AddMember:
                    ReadAddMember(bitStream);
                    return;

                case GuildInfoMessages.RemoveMember:
                    ReadRemoveMember(bitStream);
                    return;

                case GuildInfoMessages.AddOnlineMember:
                    ReadAddOnlineMember(bitStream);
                    return;

                case GuildInfoMessages.RemoveOnlineMember:
                    ReadRemoveOnlineMember(bitStream);
                    return;

                default:
                    const string errmsg = "Unknown GuildInfoMessages value `{0}`. Could not parse!";
                    string err = string.Format(errmsg, id);
                    log.Fatal(err);
                    Debug.Fail(err);
                    return;
            }
        }

        void ReadAddMember(IValueReader r)
        {
            var member = r.ReadGuildMemberNameRank(null);
            _members.Add(member);
            _members.Sort();

            if (OnAddMember != null)
                OnAddMember(this, member);
        }

        void ReadAddOnlineMember(BitStream r)
        {
            string name = r.ReadString();
            SetOnlineValue(name, true);

            if (OnAddOnlineMember != null)
                OnAddOnlineMember(this, name);
        }

        void ReadRemoveMember(BitStream r)
        {
            var name = r.ReadString();
            int removeCount = _members.RemoveAll(x => StringComparer.OrdinalIgnoreCase.Equals(x.Name, name));

            Debug.Assert(removeCount != 0, "Nobody with the name " + name + " existed in the collection.");
            Debug.Assert(removeCount < 2, "How the hell did we remove more than one item?");

            if (OnRemoveMember != null)
                OnRemoveMember(this, name);
        }

        void ReadRemoveOnlineMember(BitStream pw)
        {
            string name = pw.ReadString();
            SetOnlineValue(name, false);

            if (OnRemoveOnlineMember != null)
                OnRemoveOnlineMember(this, name);
        }

        void ReadSetGuild(BitStream r)
        {
            _members.Clear();
            _onlineMembers.Clear();

            InGuild = r.ReadBool();

            if (InGuild)
            {
                Name = r.ReadString();
                Tag = r.ReadString();

                ushort numMembers = r.ReadUShort();
                for (int i = 0; i < numMembers; i++)
                {
                    var v = r.ReadGuildMemberNameRank(null);
                    _members.Add(v);
                }

                ushort onlineMembers = r.ReadUShort();
                for (int i = 0; i < onlineMembers; i++)
                {
                    string name = r.ReadString();
                    SetOnlineValue(name, true);
                }

                _members.Sort();
            }

            if (OnChangeGuild != null)
                OnChangeGuild(this);
        }

        /// <summary>
        /// Sets the online value for the member at the given index.
        /// </summary>
        /// <param name="name">The member's name.</param>
        /// <param name="online">True to set them as online; false to set them as offline.</param>
        void SetOnlineValue(string name, bool online)
        {
            if (!online)
            {
                // Remove online status
                _onlineMembers.Remove(name);
            }
            else
            {
                // Add online status
                if (!_onlineMembers.Contains(name, StringComparer.OrdinalIgnoreCase))
                    _onlineMembers.Add(name);
            }
        }

        public static void WriteAddMember(BitStream pw, GuildMemberNameRank member)
        {
            pw.WriteEnum(GuildInfoMessages.AddMember);
            pw.Write(null, member);
        }

        public static void WriteAddOnlineMember(BitStream pw, string memberName)
        {
            pw.WriteEnum(GuildInfoMessages.AddOnlineMember);
            pw.Write(memberName);
        }

        public static void WriteGuildInfo(BitStream pw, IGuild guild)
        {
            pw.WriteEnum(GuildInfoMessages.SetGuild);

            if (guild == null)
            {
                pw.Write(false);
                return;
            }

            var members = guild.GetMembers().ToArray();
            var onlineMembers = guild.OnlineMembers.ToArray();

            pw.Write(true);
            pw.Write(guild.Name);
            pw.Write(guild.Tag);

            pw.Write((ushort)members.Length);
            for (int i = 0; i < members.Length; i++)
            {
                pw.Write(null, members[i]);
            }

            pw.Write((ushort)onlineMembers.Length);
            for (int i = 0; i < onlineMembers.Length; i++)
            {
                pw.Write(onlineMembers[i].Name);
            }
        }

        public static void WriteRemoveMember(BitStream pw, string memberName)
        {
            pw.WriteEnum(GuildInfoMessages.RemoveMember);
            pw.Write(memberName);
        }

        public static void WriteRemoveOnlineMember(BitStream pw, string memberName)
        {
            pw.WriteEnum(GuildInfoMessages.RemoveOnlineMember);
            pw.Write(memberName);
        }

        /// <summary>
        /// Enum of the different packet messages for this class.
        /// </summary>
        enum GuildInfoMessages
        {
            /// <summary>
            /// Adds a single member.
            /// </summary>
            AddMember,

            /// <summary>
            /// Removes a single member.
            /// </summary>
            RemoveMember,

            /// <summary>
            /// Adds a single online member
            /// </summary>
            AddOnlineMember,

            /// <summary>
            /// Removes a single online member.
            /// </summary>
            RemoveOnlineMember,

            /// <summary>
            /// Sets all the initial guild information.
            /// </summary>
            SetGuild
        }
    }
}