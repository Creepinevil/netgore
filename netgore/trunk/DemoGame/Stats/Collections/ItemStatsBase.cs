using System.Collections.Generic;
using System.Linq;

// FUTURE: Could optimize the OnStatChange by only hooking to the Stat.OnChange when OnStatChange has listeners

namespace DemoGame
{
    public class ItemStatsBase : DynamicStatCollection<StatType>
    {
        /// <summary>
        /// Notifies the listener that any of the stats have raised their OnChange event
        /// </summary>
        public event IStatEventHandler<StatType> OnStatChange;

        public ItemStatsBase(IEnumerable<KeyValuePair<StatType, int>> src, StatCollectionType statCollectionType)
            : this(statCollectionType)
        {
            foreach (var statInfo in src)
            {
                var stat = StatFactory<StatType>.CreateStat(statInfo.Key, statCollectionType, statInfo.Value);
                Add(stat);
            }
        }

        public ItemStatsBase(StatCollectionType statCollectionType) : base(statCollectionType)
        {
        }

        public override IStat<StatType> GetStat(StatType statType)
        {
            return GetStatOrCreate(statType);
        }

        protected override void HandleStatAdded(IStat<StatType> stat)
        {
            // Attach a listener to every stat to listen for changes
            stat.OnChange += HandleStatChanged;
        }

        /// <summary>
        /// Handler for listening to all of the stats and forwarding to the OnStatChange
        /// </summary>
        /// <param name="stat">Stat that changed</param>
        void HandleStatChanged(IStat<StatType> stat)
        {
            if (OnStatChange != null)
                OnStatChange(stat);
        }

        /// <summary>
        /// Checks if the stats in this ItemStatsBase contain the same values as another ItemStatsBase
        /// </summary>
        /// <param name="other">ItemStatsBase to compare against</param>
        /// <returns>True if each stat in this ItemStatsBase has the same value as the stats in
        /// <paramref name="other"/>, else false</returns>
        public bool HasEqualValues(ItemStatsBase other)
        {
            return this.All(x => x.Value == other[x.StatType]);
        }
    }
}