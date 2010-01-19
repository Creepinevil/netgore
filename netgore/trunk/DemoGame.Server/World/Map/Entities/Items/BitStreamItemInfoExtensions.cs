using System.Linq;
using DemoGame.DbObjs;
using DemoGame.Server.DbObjs;
using NetGore.IO;
using NetGore.Stats;

namespace DemoGame.Server
{
    public static class BitStreamItemInfoExtensions
    {
        public static void Write(this IValueWriter w, string name, IItemTemplateTable it)
        {
            // TODO: $$$$ Messy piece of shit

            // NOTE: Excessive ItemStat collection construction - would be better to cache the stats for an item template?
            var baseStats = new ItemStats(it.Stats, StatCollectionType.Base);
            var reqStats = new ItemStats(it.ReqStats, StatCollectionType.Requirement);
            ItemInfo.Write((BitStream)w, it.Name, it.Description, it.Value, it.Graphic, it.HP, it.MP, baseStats, reqStats);
        }

        public static void Write(this BitStream w, IItemTable it)
        {
            var baseStats = new ItemStats(it.Stats, StatCollectionType.Base);
            var reqStats = new ItemStats(it.ReqStats, StatCollectionType.Requirement);
            ItemInfo.Write(w, it.Name, it.Description, it.Value, it.Graphic, it.HP, it.MP, baseStats, reqStats);
        }
    }
}