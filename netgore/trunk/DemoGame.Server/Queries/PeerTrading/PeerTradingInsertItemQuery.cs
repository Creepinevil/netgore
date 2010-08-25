using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using DemoGame.Server.DbObjs;
using NetGore;
using NetGore.Db;

namespace DemoGame.Server.Queries
{
    [DbControllerQuery]
    public class PeerTradingInsertItemQuery : DbQueryNonReader<PeerTradingInsertItemQuery.QueryArgs>
    {
        static readonly string _queryStr = FormatQueryString("INSERT IGNORE INTO `{0}` {1}", ActiveTradeItemTable.TableName,
                                                             FormatParametersIntoValuesString(ActiveTradeItemTable.DbColumns));

        /// <summary>
        /// Initializes a new instance of the <see cref="PeerTradingInsertItemQuery"/> class.
        /// </summary>
        /// <param name="connectionPool"><see cref="DbConnectionPool"/> to use for creating connections to
        /// execute the query on.</param>
        public PeerTradingInsertItemQuery(DbConnectionPool connectionPool) : base(connectionPool, _queryStr)
        {
            QueryAsserts.ContainsColumns(ActiveTradeItemTable.DbColumns, "item_id", "character_id");
        }

        /// <summary>
        /// Executes the query on the database using the specified item.
        /// </summary>
        /// <param name="characterID">The character ID.</param>
        /// <param name="itemID">The item ID.</param>
        /// <returns>Number of rows affected by the query.</returns>
        /// <exception cref="DuplicateKeyException">Trying to insert a value who's primary key already exists.</exception>
        public int Execute(ItemID itemID, CharacterID characterID)
        {
            return Execute(new QueryArgs(itemID, characterID));
        }

        /// <summary>
        /// When overridden in the derived class, creates the parameters this class uses for creating database queries.
        /// </summary>
        /// <returns>
        /// The <see cref="DbParameter"/>s needed for this class to perform database queries.
        /// If null, no parameters will be used.
        /// </returns>
        protected override IEnumerable<DbParameter> InitializeParameters()
        {
            return CreateParameters(ActiveTradeItemTable.DbColumns);
        }

        /// <summary>
        /// When overridden in the derived class, sets the database parameters values <paramref name="p"/>
        /// based on the values specified in the given <paramref name="item"/> parameter.
        /// </summary>
        /// <param name="p">Collection of database parameters to set the values for.</param>
        /// <param name="item">The value or object/struct containing the values used to execute the query.</param>
        protected override void SetParameters(DbParameterValues p, QueryArgs item)
        {
            p["item_id"] = (int)item.ItemID;
            p["character_id"] = (int)item.CharacterID;
        }

        /// <summary>
        /// The arguments for the <see cref="PeerTradingInsertItemQuery"/>.
        /// </summary>
        public struct QueryArgs
        {
            /// <summary>
            /// The <see cref="CharacterID"/> for the row to insert.
            /// </summary>
            public CharacterID CharacterID;

            /// <summary>
            /// The <see cref="ItemID"/> of the row to insert.
            /// </summary>
            public ItemID ItemID;

            /// <summary>
            /// Initializes a new instance of the <see cref="QueryArgs"/> struct.
            /// </summary>
            /// <param name="itemID">The item ID.</param>
            /// <param name="characterID">The character ID.</param>
            public QueryArgs(ItemID itemID, CharacterID characterID)
            {
                CharacterID = characterID;
                ItemID = itemID;
            }
        }
    }
}