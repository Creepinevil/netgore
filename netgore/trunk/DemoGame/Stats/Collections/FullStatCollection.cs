using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NetGore;
using NetGore.IO;

namespace DemoGame
{
    /// <summary>
    /// A specialized collection that contains every StatType. This is the ideal IStatCollection to use when you want
    /// the collection to always contain every StatType.
    /// </summary>
    public class FullStatCollection<TStatType> : IStatCollection<TStatType>
        where TStatType : struct, IComparable, IConvertible, IFormattable
    {
        readonly StatCollectionType _collectionType;
        readonly IStat<TStatType>[] _stats;
        readonly EnumIOHelper<TStatType> _enumIOHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="FullStatCollection"/> class.
        /// </summary>
        /// <param name="collectionType">The type of StatTypes that this collection contains.</param>
        public FullStatCollection(StatCollectionType collectionType, EnumIOHelper<TStatType> enumIOHelper)
        {
            if (enumIOHelper == null)
                throw new ArgumentNullException("enumIOHelper");

            _enumIOHelper = enumIOHelper;
            _collectionType = collectionType;
            _stats = new IStat<TStatType>[_enumIOHelper.MaxValue + 1];

            foreach (var statType in EnumIOHelper<TStatType>.Values)
            {
                IStat<TStatType> istat = StatFactory<TStatType>.CreateStat(statType, collectionType);
                _stats[enumIOHelper.ToInt(statType)] = istat;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FullStatCollection"/> class.
        /// </summary>
        /// <param name="source">The FullStatCollection to copy the values from.</param>
        FullStatCollection(FullStatCollection<TStatType> source)
        {
            _collectionType = source._collectionType;

            _stats = new IStat<TStatType>[source._stats.Length];
            for (int i = 0; i < _stats.Length; i++)
            {
                _stats[i] = source._stats[i].DeepCopy();
            }
        }

        /// <summary>
        /// Checks if the values of this FullStatCollection are equal to the values of another FullStatCollection.
        /// </summary>
        /// <param name="other">The FullStatCollection to compare against.</param>
        /// <returns>True if the values of this FullStatCollection are equal to the values
        /// of <paramref name="other"/>.</returns>
        public bool AreValuesEqual(FullStatCollection<TStatType> other)
        {
            return _stats.All(x => x.Value == other[x.StatType]);
        }

        /// <summary>
        /// Creates a deep copy of this FullStatCollection/
        /// </summary>
        /// <returns>A deep copy of this FullStatCollection/</returns>
        public FullStatCollection<TStatType> DeepCopy()
        {
            return new FullStatCollection<TStatType>(this);
        }

        /// <summary>
        /// Sets the value of all stats in this collection.
        /// </summary>
        /// <param name="value">Value to set the stats to.</param>
        public void SetAll(int value)
        {
            foreach (var stat in _stats)
            {
                stat.Value = value;
            }
        }

        #region IStatCollection Members

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<IStat<TStatType>> GetEnumerator()
        {
            for (int i = 0; i < _stats.Length; i++)
            {
                yield return _stats[i];
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Gets or sets the value of the stat with the given <paramref name="statType"/>.
        /// </summary>
        /// <param name="statType">The StatType of the stat to get or set the value for.</param>
        /// <returns>The value of the stat with the given <paramref name="statType"/>.</returns>
        public int this[TStatType statType]
        {
            get { return _stats[_enumIOHelper.ToInt(statType)].Value; }
            set { _stats[_enumIOHelper.ToInt(statType)].Value = value; }
        }

        /// <summary>
        /// Gets the StatCollectionType that this collection is for.
        /// </summary>
        public StatCollectionType StatCollectionType
        {
            get { return _collectionType; }
        }

        /// <summary>
        /// Checks if this collection contains the stat with the given <paramref name="statType"/>.
        /// </summary>
        /// <param name="statType">The StatType to check if exists in the collection.</param>
        /// <returns>True if this collection contains the stat with the given <paramref name="statType"/>;
        /// otherwise false.</returns>
        public bool Contains(TStatType statType)
        {
            return true;
        }

        /// <summary>
        /// Gets the IStat for the stat of the given <paramref name="statType"/>.
        /// </summary>
        /// <param name="statType">The StatType of the stat to get.</param>
        /// <returns>The IStat for the stat of the given <paramref name="statType"/>.</returns>
        public IStat<TStatType> GetStat(TStatType statType)
        {
            return _stats[_enumIOHelper.ToInt(statType)];
        }

        /// <summary>
        /// Tries to get the IStat for the stat of the given <paramref name="statType"/>.
        /// </summary>
        /// <param name="statType">The StatType of the stat to get.</param>
        /// <param name="stat">The IStat for the stat of the given <paramref name="statType"/>. If this method
        /// returns false, this value will be null.</param>
        /// <returns>True if the stat with the given <paramref name="statType"/> was found and
        /// successfully returned; otherwise false.</returns>
        public bool TryGetStat(TStatType statType, out IStat<TStatType> stat)
        {
            stat = GetStat(statType);
            return true;
        }

        /// <summary>
        /// Tries to get the value of the stat of the given <paramref name="statType"/>.
        /// </summary>
        /// <param name="statType">The StatType of the stat to get.</param>
        /// <param name="value">The value of the stat of the given <paramref name="statType"/>. If this method
        /// returns false, this value will be 0.</param>
        /// <returns>True if the stat with the given <paramref name="statType"/> was found and
        /// successfully returned; otherwise false.</returns>
        public bool TryGetStatValue(TStatType statType, out int value)
        {
            value = this[statType];
            return true;
        }

        /// <summary>
        /// Copies the values from the given IEnumerable of <paramref name="values"/> using the given StatType
        /// into this IStatCollection.
        /// </summary>
        /// <param name="values">IEnumerable of StatTypes and stat values to copy into this IStatCollection.</param>
        /// <param name="checkContains">If true, each StatType in <paramref name="values"/> will first be checked
        /// if it is in this IStatCollection before trying to copy over the value. Any StatType in
        /// <paramref name="values"/> but not in this IStatCollection will be skipped. If false, no checking will
        /// be done. Any StatType in <paramref name="values"/> but not in this IStatCollection will behave
        /// the same as if the value of a StatType not in this IStatCollection was attempted to be assigned
        /// in any other way.</param>
        public void CopyValuesFrom(IEnumerable<KeyValuePair<TStatType, int>> values, bool checkContains)
        {
            foreach (var value in values)
            {
                if (checkContains && !Contains(value.Key))
                    continue;

                this[value.Key] = value.Value;
            }
        }

        /// <summary>
        /// Copies the values from the given IEnumerable of <paramref name="values"/> using the given StatType
        /// into this IStatCollection.
        /// </summary>
        /// <param name="values">IEnumerable of StatTypes and stat values to copy into this IStatCollection.</param>
        /// <param name="checkContains">If true, each StatType in <paramref name="values"/> will first be checked
        /// if it is in this IStatCollection before trying to copy over the value. Any StatType in
        /// <paramref name="values"/> but not in this IStatCollection will be skipped. If false, no checking will
        /// be done. Any StatType in <paramref name="values"/> but not in this IStatCollection will behave
        /// the same as if the value of a StatType not in this IStatCollection was attempted to be assigned
        /// in any other way.</param>
        public void CopyValuesFrom(IEnumerable<IStat<TStatType>> values, bool checkContains)
        {
            CopyValuesFrom(values.ToKeyValuePairs(), checkContains);
        }

        #endregion
    }
}