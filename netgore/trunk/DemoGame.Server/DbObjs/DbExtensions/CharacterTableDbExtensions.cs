using System;
using System.Linq;
using NetGore;
using NetGore.IO;
using System.Collections.Generic;
using System.Collections;
using NetGore.Db;
using DemoGame.DbObjs;
namespace DemoGame.Server.DbObjs
{
/// <summary>
/// Contains extension methods for class CharacterTable that assist in performing
/// reads and writes to and from a database.
/// </summary>
public static  class CharacterTableDbExtensions
{
/// <summary>
/// Copies the column values into the given DbParameterValues using the database column name
/// with a prefixed @ as the key. The keys must already exist in the DbParameterValues;
///  this method will not create them if they are missing.
/// </summary>
/// <param name="source">The object to copy the values from.</param>
/// <param name="paramValues">The DbParameterValues to copy the values into.</param>
public static void CopyValues(this ICharacterTable source, NetGore.Db.DbParameterValues paramValues)
{
paramValues["@account_id"] = (System.Nullable<System.Int32>)source.AccountID;
paramValues["@ai_id"] = (System.Nullable<System.UInt16>)source.AIID;
paramValues["@body_id"] = (System.UInt16)source.BodyID;
paramValues["@cash"] = (System.Int32)source.Cash;
paramValues["@character_template_id"] = (System.Nullable<System.UInt16>)source.CharacterTemplateID;
paramValues["@chat_dialog"] = (System.Nullable<System.UInt16>)source.ChatDialog;
paramValues["@exp"] = (System.Int32)source.Exp;
paramValues["@hp"] = (System.Int16)source.HP;
paramValues["@id"] = (System.Int32)source.ID;
paramValues["@level"] = (System.Byte)source.Level;
paramValues["@map_id"] = (System.UInt16)source.MapID;
paramValues["@move_speed"] = (System.UInt16)source.MoveSpeed;
paramValues["@mp"] = (System.Int16)source.MP;
paramValues["@name"] = (System.String)source.Name;
paramValues["@respawn_map"] = (System.Nullable<System.UInt16>)source.RespawnMap;
paramValues["@respawn_x"] = (System.Single)source.RespawnX;
paramValues["@respawn_y"] = (System.Single)source.RespawnY;
paramValues["@shop_id"] = (System.Nullable<System.UInt16>)source.ShopID;
paramValues["@statpoints"] = (System.Int32)source.StatPoints;
paramValues["@stat_agi"] = (System.Int16)source.GetStat((DemoGame.StatType)DemoGame.StatType.Agi);
paramValues["@stat_defence"] = (System.Int16)source.GetStat((DemoGame.StatType)DemoGame.StatType.Defence);
paramValues["@stat_int"] = (System.Int16)source.GetStat((DemoGame.StatType)DemoGame.StatType.Int);
paramValues["@stat_maxhit"] = (System.Int16)source.GetStat((DemoGame.StatType)DemoGame.StatType.MaxHit);
paramValues["@stat_maxhp"] = (System.Int16)source.GetStat((DemoGame.StatType)DemoGame.StatType.MaxHP);
paramValues["@stat_maxmp"] = (System.Int16)source.GetStat((DemoGame.StatType)DemoGame.StatType.MaxMP);
paramValues["@stat_minhit"] = (System.Int16)source.GetStat((DemoGame.StatType)DemoGame.StatType.MinHit);
paramValues["@stat_str"] = (System.Int16)source.GetStat((DemoGame.StatType)DemoGame.StatType.Str);
paramValues["@x"] = (System.Single)source.X;
paramValues["@y"] = (System.Single)source.Y;
}

/// <summary>
/// Reads the values from an IDataReader and assigns the read values to this
/// object's properties. The database column's name is used to as the key, so the value
/// will not be found if any aliases are used or not all columns were selected.
/// </summary>
/// <param name="source">The object to add the extension method to.</param>
/// <param name="dataReader">The IDataReader to read the values from. Must already be ready to be read from.</param>
public static void ReadValues(this CharacterTable source, System.Data.IDataReader dataReader)
{
System.Int32 i;

i = dataReader.GetOrdinal("account_id");

source.AccountID = (System.Nullable<DemoGame.Server.AccountID>)(System.Nullable<DemoGame.Server.AccountID>)(dataReader.IsDBNull(i) ? (System.Nullable<System.Int32>)null : dataReader.GetInt32(i));

i = dataReader.GetOrdinal("ai_id");

source.AIID = (System.Nullable<NetGore.AI.AIID>)(System.Nullable<NetGore.AI.AIID>)(dataReader.IsDBNull(i) ? (System.Nullable<System.UInt16>)null : dataReader.GetUInt16(i));

i = dataReader.GetOrdinal("body_id");

source.BodyID = (DemoGame.BodyIndex)(DemoGame.BodyIndex)dataReader.GetUInt16(i);

i = dataReader.GetOrdinal("cash");

source.Cash = (System.Int32)(System.Int32)dataReader.GetInt32(i);

i = dataReader.GetOrdinal("character_template_id");

source.CharacterTemplateID = (System.Nullable<DemoGame.Server.CharacterTemplateID>)(System.Nullable<DemoGame.Server.CharacterTemplateID>)(dataReader.IsDBNull(i) ? (System.Nullable<System.UInt16>)null : dataReader.GetUInt16(i));

i = dataReader.GetOrdinal("chat_dialog");

source.ChatDialog = (System.Nullable<System.UInt16>)(System.Nullable<System.UInt16>)(dataReader.IsDBNull(i) ? (System.Nullable<System.UInt16>)null : dataReader.GetUInt16(i));

i = dataReader.GetOrdinal("exp");

source.Exp = (System.Int32)(System.Int32)dataReader.GetInt32(i);

i = dataReader.GetOrdinal("hp");

source.HP = (DemoGame.SPValueType)(DemoGame.SPValueType)dataReader.GetInt16(i);

i = dataReader.GetOrdinal("id");

source.ID = (DemoGame.Server.CharacterID)(DemoGame.Server.CharacterID)dataReader.GetInt32(i);

i = dataReader.GetOrdinal("level");

source.Level = (System.Byte)(System.Byte)dataReader.GetByte(i);

i = dataReader.GetOrdinal("map_id");

source.MapID = (NetGore.MapIndex)(NetGore.MapIndex)dataReader.GetUInt16(i);

i = dataReader.GetOrdinal("move_speed");

source.MoveSpeed = (System.UInt16)(System.UInt16)dataReader.GetUInt16(i);

i = dataReader.GetOrdinal("mp");

source.MP = (DemoGame.SPValueType)(DemoGame.SPValueType)dataReader.GetInt16(i);

i = dataReader.GetOrdinal("name");

source.Name = (System.String)(System.String)dataReader.GetString(i);

i = dataReader.GetOrdinal("respawn_map");

source.RespawnMap = (System.Nullable<NetGore.MapIndex>)(System.Nullable<NetGore.MapIndex>)(dataReader.IsDBNull(i) ? (System.Nullable<System.UInt16>)null : dataReader.GetUInt16(i));

i = dataReader.GetOrdinal("respawn_x");

source.RespawnX = (System.Single)(System.Single)dataReader.GetFloat(i);

i = dataReader.GetOrdinal("respawn_y");

source.RespawnY = (System.Single)(System.Single)dataReader.GetFloat(i);

i = dataReader.GetOrdinal("shop_id");

source.ShopID = (System.Nullable<NetGore.Features.Shops.ShopID>)(System.Nullable<NetGore.Features.Shops.ShopID>)(dataReader.IsDBNull(i) ? (System.Nullable<System.UInt16>)null : dataReader.GetUInt16(i));

i = dataReader.GetOrdinal("statpoints");

source.StatPoints = (System.Int32)(System.Int32)dataReader.GetInt32(i);

i = dataReader.GetOrdinal("stat_agi");

source.SetStat((DemoGame.StatType)DemoGame.StatType.Agi, (System.Int32)(System.Int16)dataReader.GetInt16(i));

i = dataReader.GetOrdinal("stat_defence");

source.SetStat((DemoGame.StatType)DemoGame.StatType.Defence, (System.Int32)(System.Int16)dataReader.GetInt16(i));

i = dataReader.GetOrdinal("stat_int");

source.SetStat((DemoGame.StatType)DemoGame.StatType.Int, (System.Int32)(System.Int16)dataReader.GetInt16(i));

i = dataReader.GetOrdinal("stat_maxhit");

source.SetStat((DemoGame.StatType)DemoGame.StatType.MaxHit, (System.Int32)(System.Int16)dataReader.GetInt16(i));

i = dataReader.GetOrdinal("stat_maxhp");

source.SetStat((DemoGame.StatType)DemoGame.StatType.MaxHP, (System.Int32)(System.Int16)dataReader.GetInt16(i));

i = dataReader.GetOrdinal("stat_maxmp");

source.SetStat((DemoGame.StatType)DemoGame.StatType.MaxMP, (System.Int32)(System.Int16)dataReader.GetInt16(i));

i = dataReader.GetOrdinal("stat_minhit");

source.SetStat((DemoGame.StatType)DemoGame.StatType.MinHit, (System.Int32)(System.Int16)dataReader.GetInt16(i));

i = dataReader.GetOrdinal("stat_str");

source.SetStat((DemoGame.StatType)DemoGame.StatType.Str, (System.Int32)(System.Int16)dataReader.GetInt16(i));

i = dataReader.GetOrdinal("x");

source.X = (System.Single)(System.Single)dataReader.GetFloat(i);

i = dataReader.GetOrdinal("y");

source.Y = (System.Single)(System.Single)dataReader.GetFloat(i);
}

/// <summary>
/// Reads the values from an IDataReader and assigns the read values to this
/// object's properties. Unlike ReadValues(), this method not only doesn't require
/// all values to be in the IDataReader, but also does not require the values in
/// the IDataReader to be a defined field for the table this class represents.
/// Because of this, you need to be careful when using this method because values
/// can easily be skipped without any indication.
/// </summary>
/// <param name="source">The object to add the extension method to.</param>
/// <param name="dataReader">The IDataReader to read the values from. Must already be ready to be read from.</param>
public static void TryReadValues(this CharacterTable source, System.Data.IDataReader dataReader)
{
for (int i = 0; i < dataReader.FieldCount; i++)
{
switch (dataReader.GetName(i))
{
case "account_id":
source.AccountID = (System.Nullable<DemoGame.Server.AccountID>)(System.Nullable<DemoGame.Server.AccountID>)(dataReader.IsDBNull(i) ? (System.Nullable<System.Int32>)null : dataReader.GetInt32(i));
break;


case "ai_id":
source.AIID = (System.Nullable<NetGore.AI.AIID>)(System.Nullable<NetGore.AI.AIID>)(dataReader.IsDBNull(i) ? (System.Nullable<System.UInt16>)null : dataReader.GetUInt16(i));
break;


case "body_id":
source.BodyID = (DemoGame.BodyIndex)(DemoGame.BodyIndex)dataReader.GetUInt16(i);
break;


case "cash":
source.Cash = (System.Int32)(System.Int32)dataReader.GetInt32(i);
break;


case "character_template_id":
source.CharacterTemplateID = (System.Nullable<DemoGame.Server.CharacterTemplateID>)(System.Nullable<DemoGame.Server.CharacterTemplateID>)(dataReader.IsDBNull(i) ? (System.Nullable<System.UInt16>)null : dataReader.GetUInt16(i));
break;


case "chat_dialog":
source.ChatDialog = (System.Nullable<System.UInt16>)(System.Nullable<System.UInt16>)(dataReader.IsDBNull(i) ? (System.Nullable<System.UInt16>)null : dataReader.GetUInt16(i));
break;


case "exp":
source.Exp = (System.Int32)(System.Int32)dataReader.GetInt32(i);
break;


case "hp":
source.HP = (DemoGame.SPValueType)(DemoGame.SPValueType)dataReader.GetInt16(i);
break;


case "id":
source.ID = (DemoGame.Server.CharacterID)(DemoGame.Server.CharacterID)dataReader.GetInt32(i);
break;


case "level":
source.Level = (System.Byte)(System.Byte)dataReader.GetByte(i);
break;


case "map_id":
source.MapID = (NetGore.MapIndex)(NetGore.MapIndex)dataReader.GetUInt16(i);
break;


case "move_speed":
source.MoveSpeed = (System.UInt16)(System.UInt16)dataReader.GetUInt16(i);
break;


case "mp":
source.MP = (DemoGame.SPValueType)(DemoGame.SPValueType)dataReader.GetInt16(i);
break;


case "name":
source.Name = (System.String)(System.String)dataReader.GetString(i);
break;


case "respawn_map":
source.RespawnMap = (System.Nullable<NetGore.MapIndex>)(System.Nullable<NetGore.MapIndex>)(dataReader.IsDBNull(i) ? (System.Nullable<System.UInt16>)null : dataReader.GetUInt16(i));
break;


case "respawn_x":
source.RespawnX = (System.Single)(System.Single)dataReader.GetFloat(i);
break;


case "respawn_y":
source.RespawnY = (System.Single)(System.Single)dataReader.GetFloat(i);
break;


case "shop_id":
source.ShopID = (System.Nullable<NetGore.Features.Shops.ShopID>)(System.Nullable<NetGore.Features.Shops.ShopID>)(dataReader.IsDBNull(i) ? (System.Nullable<System.UInt16>)null : dataReader.GetUInt16(i));
break;


case "statpoints":
source.StatPoints = (System.Int32)(System.Int32)dataReader.GetInt32(i);
break;


case "stat_agi":
source.SetStat((DemoGame.StatType)DemoGame.StatType.Agi, (System.Int32)(System.Int16)dataReader.GetInt16(i));
break;


case "stat_defence":
source.SetStat((DemoGame.StatType)DemoGame.StatType.Defence, (System.Int32)(System.Int16)dataReader.GetInt16(i));
break;


case "stat_int":
source.SetStat((DemoGame.StatType)DemoGame.StatType.Int, (System.Int32)(System.Int16)dataReader.GetInt16(i));
break;


case "stat_maxhit":
source.SetStat((DemoGame.StatType)DemoGame.StatType.MaxHit, (System.Int32)(System.Int16)dataReader.GetInt16(i));
break;


case "stat_maxhp":
source.SetStat((DemoGame.StatType)DemoGame.StatType.MaxHP, (System.Int32)(System.Int16)dataReader.GetInt16(i));
break;


case "stat_maxmp":
source.SetStat((DemoGame.StatType)DemoGame.StatType.MaxMP, (System.Int32)(System.Int16)dataReader.GetInt16(i));
break;


case "stat_minhit":
source.SetStat((DemoGame.StatType)DemoGame.StatType.MinHit, (System.Int32)(System.Int16)dataReader.GetInt16(i));
break;


case "stat_str":
source.SetStat((DemoGame.StatType)DemoGame.StatType.Str, (System.Int32)(System.Int16)dataReader.GetInt16(i));
break;


case "x":
source.X = (System.Single)(System.Single)dataReader.GetFloat(i);
break;


case "y":
source.Y = (System.Single)(System.Single)dataReader.GetFloat(i);
break;


}

}
}

/// <summary>
/// Copies the column values into the given DbParameterValues using the database column name
/// with a prefixed @ as the key. The key must already exist in the DbParameterValues
/// for the value to be copied over. If any of the keys in the DbParameterValues do not
/// match one of the column names, or if there is no field for a key, then it will be
/// ignored. Because of this, it is important to be careful when using this method
/// since columns or keys can be skipped without any indication.
/// </summary>
/// <param name="source">The object to copy the values from.</param>
/// <param name="paramValues">The DbParameterValues to copy the values into.</param>
public static void TryCopyValues(this ICharacterTable source, NetGore.Db.DbParameterValues paramValues)
{
for (int i = 0; i < paramValues.Count; i++)
{
switch (paramValues.GetParameterName(i))
{
case "@account_id":
paramValues[i] = (System.Nullable<System.Int32>)source.AccountID;
break;


case "@ai_id":
paramValues[i] = (System.Nullable<System.UInt16>)source.AIID;
break;


case "@body_id":
paramValues[i] = (System.UInt16)source.BodyID;
break;


case "@cash":
paramValues[i] = (System.Int32)source.Cash;
break;


case "@character_template_id":
paramValues[i] = (System.Nullable<System.UInt16>)source.CharacterTemplateID;
break;


case "@chat_dialog":
paramValues[i] = (System.Nullable<System.UInt16>)source.ChatDialog;
break;


case "@exp":
paramValues[i] = (System.Int32)source.Exp;
break;


case "@hp":
paramValues[i] = (System.Int16)source.HP;
break;


case "@id":
paramValues[i] = (System.Int32)source.ID;
break;


case "@level":
paramValues[i] = (System.Byte)source.Level;
break;


case "@map_id":
paramValues[i] = (System.UInt16)source.MapID;
break;


case "@move_speed":
paramValues[i] = (System.UInt16)source.MoveSpeed;
break;


case "@mp":
paramValues[i] = (System.Int16)source.MP;
break;


case "@name":
paramValues[i] = (System.String)source.Name;
break;


case "@respawn_map":
paramValues[i] = (System.Nullable<System.UInt16>)source.RespawnMap;
break;


case "@respawn_x":
paramValues[i] = (System.Single)source.RespawnX;
break;


case "@respawn_y":
paramValues[i] = (System.Single)source.RespawnY;
break;


case "@shop_id":
paramValues[i] = (System.Nullable<System.UInt16>)source.ShopID;
break;


case "@statpoints":
paramValues[i] = (System.Int32)source.StatPoints;
break;


case "@stat_agi":
paramValues[i] = (System.Int16)source.GetStat((DemoGame.StatType)DemoGame.StatType.Agi);
break;


case "@stat_defence":
paramValues[i] = (System.Int16)source.GetStat((DemoGame.StatType)DemoGame.StatType.Defence);
break;


case "@stat_int":
paramValues[i] = (System.Int16)source.GetStat((DemoGame.StatType)DemoGame.StatType.Int);
break;


case "@stat_maxhit":
paramValues[i] = (System.Int16)source.GetStat((DemoGame.StatType)DemoGame.StatType.MaxHit);
break;


case "@stat_maxhp":
paramValues[i] = (System.Int16)source.GetStat((DemoGame.StatType)DemoGame.StatType.MaxHP);
break;


case "@stat_maxmp":
paramValues[i] = (System.Int16)source.GetStat((DemoGame.StatType)DemoGame.StatType.MaxMP);
break;


case "@stat_minhit":
paramValues[i] = (System.Int16)source.GetStat((DemoGame.StatType)DemoGame.StatType.MinHit);
break;


case "@stat_str":
paramValues[i] = (System.Int16)source.GetStat((DemoGame.StatType)DemoGame.StatType.Str);
break;


case "@x":
paramValues[i] = (System.Single)source.X;
break;


case "@y":
paramValues[i] = (System.Single)source.Y;
break;


}

}
}

}

}
