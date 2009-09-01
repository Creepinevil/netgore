using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using log4net;
using NetGore;

namespace DemoGame
{
    public static class ItemTypeExtensions
    {
        static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static IEnumerable<EquipmentSlot> GetPossibleSlots(this ItemType itemType)
        {
            // FUTURE: Ugly hack - would be better if this was all defined better (attributes at least maybe)

            switch (itemType)
            {
                case ItemType.Body:
                    return new EquipmentSlot[] { EquipmentSlot.Body };

                case ItemType.Helmet:
                    return new EquipmentSlot[] { EquipmentSlot.Head };

                case ItemType.Weapon:
                    return new EquipmentSlot[] { EquipmentSlot.RightHand };

                case ItemType.Unusable:
                    return null;

                default:
                    const string errmsg = "Specified itemType `{0}` is not handled.";

                    Debug.Fail(string.Format(errmsg, itemType));
                    if (log.IsErrorEnabled)
                        log.ErrorFormat(errmsg, itemType);

                    return null;
            }
        }

        /// <summary>
        /// Checks if a specified ItemType value is defined by the ItemType enum.
        /// </summary>
        /// <param name="itemType">ItemType value to check.</param>
        /// <returns>True if the <paramref name="itemType"/> is defined in the ItemType enum, else false.</returns>
        public static bool IsDefined(this ItemType itemType)
        {
            return EnumHelper.IsDefined(itemType);
        }
    }
}