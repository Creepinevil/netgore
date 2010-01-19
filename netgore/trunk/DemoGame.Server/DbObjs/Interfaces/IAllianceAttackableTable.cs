using System;
using System.Linq;
namespace DemoGame.Server.DbObjs
{
/// <summary>
/// Interface for a class that can be used to serialize values to the database table `alliance_attackable`.
/// </summary>
public interface IAllianceAttackableTable
{
/// <summary>
/// Creates a deep copy of this table. All the values will be the same
/// but they will be contained in a different object instance.
/// </summary>
/// <returns>
/// A deep copy of this table.
/// </returns>
IAllianceAttackableTable DeepCopy();

/// <summary>
/// Gets the value of the database column `alliance_id`.
/// </summary>
DemoGame.Server.AllianceID AllianceID
{
get;
}
/// <summary>
/// Gets the value of the database column `attackable_id`.
/// </summary>
DemoGame.Server.AllianceID AttackableID
{
get;
}
/// <summary>
/// Gets the value of the database column `placeholder`.
/// </summary>
System.Nullable<System.Byte> Placeholder
{
get;
}
}

}
