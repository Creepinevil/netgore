using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using log4net;
using NetGore;
using NetGore.Db;
using NetGore.Features.Shops;
using NetGore.IO;
using NetGore.Network;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local

namespace DemoGame.Server
{
    class ServerPacketHandler : IMessageProcessor, IGetTime
    {
        static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly Queue<IIPSocket> _disconnectedSockets = new Queue<IIPSocket>();
        readonly MessageProcessorManager _ppManager;
        readonly SayHandler _sayHandler;
        readonly Server _server;
        readonly ServerSockets _serverSockets;

        public ServerPacketHandler(ServerSockets serverSockets, Server server)
        {
            if (serverSockets == null)
                throw new ArgumentNullException("serverSockets");
            if (server == null)
                throw new ArgumentNullException("server");

            _server = server;
            _serverSockets = serverSockets;
            _serverSockets.OnDisconnect += ServerSockets_OnDisconnect;
            _sayHandler = new SayHandler(server);

            _ppManager = new MessageProcessorManager(this, EnumHelper<ClientPacketID>.BitsRequired);
        }

        public IDbController DbController
        {
            get { return Server.DbController; }
        }

        /// <summary>
        /// Gets the server that the data is coming from.
        /// </summary>
        public Server Server
        {
            get { return _server; }
        }

        /// <summary>
        /// Gets the World to use.
        /// </summary>
        public World World
        {
            get { return Server.World; }
        }

        /// <summary>
        /// Gets the <see cref="IIPSocket"/>s that have been disconnected since the last call to this method.
        /// </summary>
        /// <returns>The <see cref="IIPSocket"/>s that have been disconnected since the last call to this method.</returns>
        public IEnumerable<IIPSocket> GetDisconnectedSockets()
        {
            if (_disconnectedSockets.Count == 0)
                return Enumerable.Empty<IIPSocket>();

            lock (_disconnectedSockets)
            {
                if (_disconnectedSockets.Count == 0)
                    return Enumerable.Empty<IIPSocket>();

                var ret = _disconnectedSockets.ToArray();
                _disconnectedSockets.Clear();
                return ret;
            }
        }

        [MessageHandler((byte)ClientPacketID.Attack)]
        void RecvAttack(IIPSocket conn, BitStream r)
        {
            User user;
            if ((user = TryGetUser(conn)) != null)
                user.Attack();
        }

        [MessageHandler((byte)ClientPacketID.BuyFromShop)]
        void RecvBuyFromShop(IIPSocket conn, BitStream r)
        {
            ShopItemIndex slot = r.ReadShopItemIndex();
            byte amount = r.ReadByte();

            User user;
            if ((user = TryGetUser(conn)) == null)
                return;

            user.ShoppingState.TryPurchase(slot, amount);
        }

        [MessageHandler((byte)ClientPacketID.Emoticon)]
        void RecvEmoticon(IIPSocket conn, BitStream r)
        {
            var emoticon = r.ReadEnum<Emoticon>();

            if (!EnumHelper<Emoticon>.IsDefined(emoticon))
            {
                const string errmsg = "Attempted to use undefined emoticon `{0}`.";
                if (log.IsWarnEnabled)
                    log.WarnFormat(errmsg, emoticon);
                return;
            }

            User user;
            if ((user = TryGetUser(conn)) == null)
                return;

            user.Emote(emoticon);
        }

        [MessageHandler((byte)ClientPacketID.DropInventoryItem)]
        void RecvDropInventoryItem(IIPSocket conn, BitStream r)
        {
            InventorySlot slot = r.ReadInventorySlot();

            User user;
            if ((user = TryGetUser(conn)) == null)
                return;

            user.Inventory.Drop(slot);
        }

        [MessageHandler((byte)ClientPacketID.EndNPCChatDialog)]
        void RecvEndNPCChatDialog(IIPSocket conn, BitStream r)
        {
            User user;
            if ((user = TryGetUser(conn)) == null)
                return;

            user.ChatState.EndChat();
        }

        [MessageHandler((byte)ClientPacketID.GetEquipmentItemInfo)]
        void RecvGetEquipmentItemInfo(IIPSocket conn, BitStream r)
        {
            EquipmentSlot slot = r.ReadEnum<EquipmentSlot>();

            User user;
            if ((user = TryGetUser(conn)) != null)
                user.SendEquipmentItemStats(slot);
        }

        [MessageHandler((byte)ClientPacketID.GetInventoryItemInfo)]
        void RecvGetInventoryItemInfo(IIPSocket conn, BitStream r)
        {
            InventorySlot slot = r.ReadInventorySlot();

            User user;
            if ((user = TryGetUser(conn)) != null)
                user.SendInventoryItemStats(slot);
        }

#if !TOPDOWN
        [MessageHandler((byte)ClientPacketID.Jump)]
        void RecvJump(IIPSocket conn, BitStream r)
        {
            User user;
            if (((user = TryGetUser(conn)) != null) && user.CanJump)
                user.Jump();
        }
#endif

        [MessageHandler((byte)ClientPacketID.Login)]
        void RecvLogin(IIPSocket conn, BitStream r)
        {
            ThreadAsserts.IsMainThread();

            string name = r.ReadString();
            string password = r.ReadString();

            Server.LoginAccount(conn, name, password);
        }

        [MessageHandler((byte)ClientPacketID.CreateNewAccount)]
        void RecvCreateNewAccount(IIPSocket conn, BitStream r)
        {
            ThreadAsserts.IsMainThread();

            string name = r.ReadString();
            string password = r.ReadString();
            string email = r.ReadString();

            // Ensure the connection isn't logged in
            var user = TryGetUser(conn, false);
            if (user != null)
            {
                const string errmsg = "User `{0}` tried to create a new account while already logged in.";
                if (log.IsWarnEnabled)
                    log.WarnFormat(errmsg, user);
                return;
            }

            Server.CreateAccount(conn, name, password, email);
        }

#if TOPDOWN
        [MessageHandler((byte)ClientPacketID.MoveDown)]
        void RecvMoveDown(IIPSocket conn, BitStream r)
        {
            User user;
            if ((user = TryGetUser(conn)) != null && !user.IsMovingDown)
                user.MoveDown();
        }
#endif

        [MessageHandler((byte)ClientPacketID.MoveLeft)]
        void RecvMoveLeft(IIPSocket conn, BitStream r)
        {
            User user;
            if (((user = TryGetUser(conn)) != null) && !user.IsMovingLeft)
                user.MoveLeft();
        }

        [MessageHandler((byte)ClientPacketID.MoveRight)]
        void RecvMoveRight(IIPSocket conn, BitStream r)
        {
            User user;
            if (((user = TryGetUser(conn)) != null) && !user.IsMovingRight)
                user.MoveRight();
        }

        [MessageHandler((byte)ClientPacketID.MoveStop)]
        void RecvMoveStop(IIPSocket conn, BitStream r)
        {
            User user;
            if (((user = TryGetUser(conn)) != null) && user.IsMoving)
                user.StopMoving();
        }

#if TOPDOWN
        [MessageHandler((byte)ClientPacketID.MoveStopHorizontal)]
        void RecvMoveStopHorizontal(IIPSocket conn, BitStream r)
        {
            User user;
            if ((user = TryGetUser(conn)) != null && (user.IsMovingLeft || user.IsMovingRight))
                user.StopMovingHorizontal();
        }
#endif

#if TOPDOWN
        [MessageHandler((byte)ClientPacketID.MoveStopVertical)]
        void RecvMoveStopVertical(IIPSocket conn, BitStream r)
        {
            User user;
            if ((user = TryGetUser(conn)) != null && (user.IsMovingUp || user.IsMovingDown))
                user.StopMovingVertical();
        }
#endif

#if TOPDOWN
        [MessageHandler((byte)ClientPacketID.MoveUp)]
        void RecvMoveUp(IIPSocket conn, BitStream r)
        {
            User user;
            if ((user = TryGetUser(conn)) != null && !user.IsMovingUp)
                user.MoveUp();
        }
#endif

        [MessageHandler((byte)ClientPacketID.PickupItem)]
        void RecvPickupItem(IIPSocket conn, BitStream r)
        {
            MapEntityIndex mapEntityIndex = r.ReadMapEntityIndex();

            User user;
            Map map;
            if (!TryGetMap(conn, out user, out map))
                return;

            // Get the item
            ItemEntityBase item;
            if (!map.TryGetDynamicEntity(mapEntityIndex, out item))
                return;

            // Ensure the distance is valid
            if (!GameData.IsValidPickupDistance(user, item))
            {
                const string errmsg = "User `{0}` failed to pick up item `{1}` - distance was too great.";
                if (log.IsInfoEnabled)
                    log.InfoFormat(errmsg, user, item);
                return;
            }

            // Pick it up
            item.Pickup(user);
        }

        [MessageHandler((byte)ClientPacketID.Ping)]
        void RecvPing(IIPSocket conn, BitStream r)
        {
            // Get the User
            User user;
            if ((user = TryGetUser(conn)) == null)
                return;

            using (PacketWriter pw = ServerPacket.Ping())
            {
                user.Send(pw);
            }
        }

        [MessageHandler((byte)ClientPacketID.RaiseStat)]
        void RecvRaiseStat(IIPSocket conn, BitStream r)
        {
            StatType statType;

            // Get the StatType
            try
            {
                statType = r.ReadEnum<StatType>();
            }
            catch (InvalidCastException)
            {
                const string errorMsg = "Received invaild StatType on connection `{0}`.";
                Debug.Fail(string.Format(errorMsg, conn));
                if (log.IsWarnEnabled)
                    log.WarnFormat(errorMsg, conn);
                return;
            }

            // Get the User
            User user;
            if ((user = TryGetUser(conn)) == null)
                return;

            // Raise the user's stat
            user.RaiseStat(statType);
        }

        [MessageHandler((byte)ClientPacketID.Say)]
        void RecvSay(IIPSocket conn, BitStream r)
        {
            string text = r.ReadString(GameData.MaxClientSayLength);

            User user;
            if ((user = TryGetUser(conn)) == null)
                return;

            _sayHandler.Process(user, text);
        }

        [MessageHandler((byte)ClientPacketID.SelectAccountCharacter)]
        void RecvSelectAccountCharacter(IIPSocket conn, BitStream r)
        {
            ThreadAsserts.IsMainThread();

            byte index = r.ReadByte();

            // Ensure the client is in a valid state to select an account character
            UserAccount userAccount = World.GetUserAccount(conn);
            if (userAccount == null)
                return;

            if (userAccount.User != null)
            {
                const string errmsg = "Account `{0}` tried to change characters while a character was already selected.";
                if (log.IsInfoEnabled)
                    log.InfoFormat(errmsg, userAccount);
                return;
            }

            // Get the CharacterID
            CharacterID characterID;
            if (!userAccount.TryGetCharacterID(index, out characterID))
            {
                const string errmsg = "Invalid account character index `{0}` given.";
                if (log.IsInfoEnabled)
                    log.InfoFormat(errmsg, characterID);
                return;
            }

            // Load the user
            userAccount.SetUser(World, characterID);

            // Send the MOTD
            var user = userAccount.User;
            if (user != null && !string.IsNullOrEmpty(Server.MOTD))
            {
                using (var pw = ServerPacket.Chat(Server.MOTD))
                {
                    user.Send(pw);
                }
            }
        }

        [MessageHandler((byte)ClientPacketID.SelectNPCChatDialogResponse)]
        void RecvSelectNPCChatDialogResponse(IIPSocket conn, BitStream r)
        {
            byte responseIndex = r.ReadByte();

            User user;
            if ((user = TryGetUser(conn)) == null)
                return;

            user.ChatState.EnterResponse(responseIndex);
        }

        [MessageHandler((byte)ClientPacketID.SellInventoryToShop)]
        void RecvSellInventoryToShop(IIPSocket conn, BitStream r)
        {
            InventorySlot slot = r.ReadInventorySlot();
            byte amount = r.ReadByte();

            User user;
            if ((user = TryGetUser(conn)) == null)
                return;

            user.ShoppingState.TrySellInventory(slot, amount);
        }

        [MessageHandler((byte)ClientPacketID.SetUDPPort)]
        void RecvSetUDPPort(IIPSocket conn, BitStream r)
        {
            ushort remotePort = r.ReadUShort();
            conn.SetRemoteUnreliablePort(remotePort);
        }

        [MessageHandler((byte)ClientPacketID.StartNPCChatDialog)]
        void RecvStartNPCChatDialog(IIPSocket conn, BitStream r)
        {
            MapEntityIndex npcIndex = r.ReadMapEntityIndex();

            User user;
            Map map;
            if (!TryGetMap(conn, out user, out map))
                return;

            NPC npc = map.GetDynamicEntity<NPC>(npcIndex);
            if (npc == null)
                return;

            user.ChatState.StartChat(npc);
        }

        [MessageHandler((byte)ClientPacketID.StartShopping)]
        void RecvStartShopping(IIPSocket conn, BitStream r)
        {
            MapEntityIndex entityIndex = r.ReadMapEntityIndex();

            User user;
            Map map;
            if (!TryGetMap(conn, out user, out map))
                return;

            Character shopkeeper = map.GetDynamicEntity<Character>(entityIndex);
            if (shopkeeper == null)
                return;

            user.ShoppingState.TryStartShopping(shopkeeper);
        }

        [MessageHandler((byte)ClientPacketID.UnequipItem)]
        void RecvUnequipItem(IIPSocket conn, BitStream r)
        {
            EquipmentSlot slot = r.ReadEnum<EquipmentSlot>();

            User user;
            if ((user = TryGetUser(conn)) != null)
                user.Equipped.RemoveAt(slot);
        }

        [MessageHandler((byte)ClientPacketID.UseInventoryItem)]
        void RecvUseInventoryItem(IIPSocket conn, BitStream r)
        {
            InventorySlot slot = r.ReadInventorySlot();

            User user;
            if ((user = TryGetUser(conn)) == null)
                return;

            user.UseInventoryItem(slot);
        }

        [MessageHandler((byte)ClientPacketID.UseSkill)]
        void RecvUseSkill(IIPSocket conn, BitStream r)
        {
            SkillType skillType;

            try
            {
                skillType = r.ReadEnum<SkillType>();
            }
            catch (InvalidCastException)
            {
                const string errmsg = "Failed to read SkillType from stream.";
                if (log.IsWarnEnabled)
                    log.Warn(errmsg);
                return;
            }

            User user;
            if ((user = TryGetUser(conn)) == null)
                return;

            user.UseSkill(skillType);
        }

        [MessageHandler((byte)ClientPacketID.UseWorld)]
        void RecvUseWorld(IIPSocket conn, BitStream r)
        {
            MapEntityIndex useEntityIndex = r.ReadMapEntityIndex();

            // Get the map and user
            User user;
            Map map;
            if (!TryGetMap(conn, out user, out map))
                return;

            // Grab the DynamicEntity to use
            DynamicEntity useEntity = map.GetDynamicEntity(useEntityIndex);
            if (useEntity == null)
            {
                const string errmsg = "UseEntity received but usedEntityIndex `{0}` is not a valid DynamicEntity.";
                Debug.Fail(string.Format(errmsg, useEntityIndex));
                if (log.IsErrorEnabled)
                    log.ErrorFormat(errmsg, useEntityIndex);
                return;
            }

            // Ensure the used DynamicEntity is even usable
            IUsableEntity asUsable = useEntity as IUsableEntity;
            if (asUsable == null)
            {
                const string errmsg =
                    "UseEntity received but useByIndex `{0}` refers to DynamicEntity `{1}` which does " +
                    "not implement IUsableEntity.";
                Debug.Fail(string.Format(errmsg, useEntityIndex, useEntity));
                if (log.IsErrorEnabled)
                    log.WarnFormat(errmsg, useEntityIndex, useEntity);
                return;
            }

            // Use it
            if (asUsable.Use(user))
            {
                // Notify everyone in the map it was used
                if (asUsable.NotifyClientsOfUsage)
                {
                    using (PacketWriter pw = ServerPacket.UseEntity(useEntity.MapEntityIndex, user.MapEntityIndex))
                    {
                        map.Send(pw);
                    }
                }
            }
        }

        /// <summary>
        /// A connection has been lost with a client.
        /// </summary>
        /// <param name="conn">Connection the user was using.</param>
        void ServerSockets_OnDisconnect(IIPSocket conn)
        {
            lock (_disconnectedSockets)
            {
                _disconnectedSockets.Enqueue(conn);
            }
        }

        static bool TryGetMap(Character user, out Map map)
        {
            // Check for a valid user
            if (user == null)
            {
                const string errmsg = "user is null.";
                if (log.IsErrorEnabled)
                    log.Error(errmsg);
                Debug.Fail(errmsg);
                map = null;
                return false;
            }

            // Get the map
            map = user.Map;
            if (map == null)
            {
                // Invalid map
                const string errorMsg = "Received UseWorld from user `{0}`, but their map is null.";
                Debug.Fail(string.Format(errorMsg, user));
                if (log.IsWarnEnabled)
                    log.WarnFormat(errorMsg, user);
                return false;
            }

            // Valid map
            return true;
        }

        /// <summary>
        /// Tries to get the <see cref="Map"/> and <see cref="User"/> from an <see cref="IIPSocket"/>.
        /// </summary>
        /// <param name="conn">The <see cref="IIPSocket"/> to get the <see cref="Map"/> and <see cref="User"/> from.</param>
        /// <param name="user">When this method returns true, contains the <see cref="User"/>.</param>
        /// <param name="map">When this method returns true, contains the <see cref="Map"/>.</param>
        /// <returns>True if the <paramref name="user"/> and <paramref name="map"/> were successfully found; otherwise
        /// false.</returns>
        bool TryGetMap(IIPSocket conn, out User user, out Map map)
        {
            if ((user = TryGetUser(conn)) == null)
            {
                map = null;
                return false;
            }

            return TryGetMap(user, out map);
        }

        /// <summary>
        /// Tries to get the <see cref="User"/> from an <see cref="IIPSocket"/>.
        /// </summary>
        /// <param name="conn">The <see cref="IIPSocket"/> to get the <see cref="User"/> from.</param>
        /// <param name="errorOnFailure">If true, an error will be printed if the <see cref="User"/> for the
        /// <paramref name="conn"/> could not be found. This should only be false when it is expected that
        /// there will be no <see cref="User"/>.</param>
        /// <returns>The <see cref="User"/> from the <paramref name="conn"/>, or null if no <see cref="User"/>
        /// could be found.</returns>
        User TryGetUser(IIPSocket conn, bool errorOnFailure)
        {
            // Check for a valid connection
            if (conn == null)
            {
                const string errmsg = "conn is null.";
                Debug.Fail(errmsg);
                log.Warn(errmsg);
                return null;
            }

            // Get the user
            var user = World.GetUser(conn);

            // Check for a valid user
            if (user == null)
            {
                if (errorOnFailure)
                {
                    const string errmsg = "user is null.";
                    Debug.Fail(errmsg);
                    log.Error(errmsg);
                }
            }

            return user;
        }

        /// <summary>
        /// Tries to get the <see cref="User"/> from an <see cref="IIPSocket"/>.
        /// </summary>
        /// <param name="conn">The <see cref="IIPSocket"/> to get the <see cref="User"/> from.</param>
        /// <returns>The <see cref="User"/> from the <paramref name="conn"/>, or null if no <see cref="User"/>
        /// could be found.</returns>
        User TryGetUser(IIPSocket conn)
        {
            return TryGetUser(conn, true);
        }

        #region IGetTime Members

        /// <summary>
        /// Gets the current time.
        /// </summary>
        /// <returns>Current time.</returns>
        public int GetTime()
        {
            return Server.GetTime();
        }

        #endregion

        #region IMessageProcessor Members

        /// <summary>
        /// Handles a list of received data and forwards it to the corresponding MessageProcessors.
        /// </summary>
        /// <param name="recvData">List of SocketReceiveData to process.</param>
        public void Process(IEnumerable<SocketReceiveData> recvData)
        {
            ThreadAsserts.IsMainThread();
            _ppManager.Process(recvData);
        }

        #endregion
    }
}