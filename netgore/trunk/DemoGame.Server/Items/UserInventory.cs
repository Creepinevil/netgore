using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using DemoGame.Extensions;
using log4net;

// FUTURE: There may be a bug in picking up items, from threading conflict, that will allow two people to pick up the same item

// TODO: Change the "UserItem" part of queries to "UserInventory", since it is a bit misleading

namespace DemoGame.Server
{
    /// <summary>
    /// An inventory for a single User on the Server
    /// </summary>
    class UserInventory : Inventory
    {
        static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly UserInventoryUpdater _inventoryUpdater;

        readonly User _user;
        bool _isLoading;

        /// <summary>
        /// Gets the Character that this inventory is for
        /// </summary>
        public override Character Character
        {
            get { return _user; }
        }

        /// <summary>
        /// Gets the DBController used by this UserInventory.
        /// </summary>
        DBController DbController
        {
            get { return User.DBController; }
        }

        /// <summary>
        /// Gets the User that this inventory is for
        /// </summary>
        public User User
        {
            get { return _user; }
        }

        /// <summary>
        /// UserInventory constructor
        /// </summary>
        /// <param name="user">User that the inventory is for</param>
        public UserInventory(User user)
        {
            _user = user;
            _inventoryUpdater = new UserInventoryUpdater(this);
        }

        public void DecreaseItemAmount(byte slot)
        {
            // TODO: It would be preferred that I hook to the Item to listen to the amount's changes instead

            // Get the item
            ItemEntity item = this[slot];
            if (item == null)
            {
                const string errmsg = "Tried to decrease amount of inventory slot `{0}`, but it contains no item.";
                Debug.Fail(string.Format(errmsg, slot));
                if (log.IsWarnEnabled)
                    log.WarnFormat(errmsg, slot);
                return;
            }

            // Check for a valid amount
            if (item.Amount == 0)
            {
                // If amount is already 0, we will show a warning but still remove the item
                const string errmsg = "Item in slot `{0}` already contains an amount of 0.";
                Debug.Fail(string.Format(errmsg, slot));
                if (log.IsWarnEnabled)
                    log.WarnFormat(errmsg, slot);

                // Remove the item
                ClearSlot(slot, true);
            }
            else
            {
                // Decrease the amount
                item.Amount--;

                // Remove the item if it ran out
                if (item.Amount <= 0)
                    ClearSlot(slot, true);
            }
        }

        /// <summary>
        /// When overridden in the derived class, performs additional processing to handle an Inventory slot
        /// changing. This is only called when the object references changes, not when any part of the object
        /// (such as the Item's amount) changes. It is guarenteed that if <paramref name="newItem"/> is null,
        /// <paramref name="oldItem"/> will not be, and vise versa. Both will never be null or non-null.
        /// </summary>
        /// <param name="slot">Slot that the change took place in.</param>
        /// <param name="newItem">The ItemEntity that was added to the <paramref name="slot"/>.</param>
        /// <param name="oldItem">The ItemEntity that used to be in the <paramref name="slot"/>,
        /// or null if the slot used to be empty.</param>
        protected override void HandleSlotChanged(int slot, ItemEntity newItem, ItemEntity oldItem)
        {
            // If we are loading the Inventory, we do not want to do database updates since that would be redundant
            // and likely cause problems

            if (newItem == null)
            {
                // Item was removed

                // Update the database
                if (!_isLoading)
                    DbController.DeleteUserItem.Execute(oldItem.Guid);

                // Stop listening for changes
                oldItem.OnChangeGraphicOrAmount -= ItemGraphicOrAmountChangeHandler;
            }
            else
            {
                // Item was added

                // Update the database
                if (!_isLoading)
                    DbController.InsertUserItem.Execute(new InsertUserItemQuery.QueryArgs(User.Guid, newItem.Guid));

                // Listen to the item for changes
                newItem.OnChangeGraphicOrAmount += ItemGraphicOrAmountChangeHandler;
            }

            // Prepare the slot for updating
            _inventoryUpdater.SlotChanged(slot);
        }

        /// <summary>
        /// Handles when an item in the UserInventory has changed the amount or graphic, and notifies the
        /// InventoryUpdater to handle the change.
        /// </summary>
        /// <param name="item">Item that has changed.</param>
        void ItemGraphicOrAmountChangeHandler(ItemEntity item)
        {
            int slot;

            // Try to get the slot
            try
            {
                slot = GetSlot(item);
            }
            catch (ArgumentException ex)
            {
                log.Warn(string.Format("Failed to get the inventory slot of item `{0}`", item), ex);
                return;
            }

            // Set the slot as changed
            _inventoryUpdater.SlotChanged(slot);
        }

        /// <summary>
        /// Loads the User's inventory.
        /// </summary>
        public void Load()
        {
            _isLoading = true;

            // TODO: Need to track the slots, too, I guess
            int slot = 0;
            foreach (ItemValues values in DbController.SelectUserInventoryItems.Execute(User.Guid))
            {
                // Make sure no item is already in the slot... just in case
                if (this[slot] != null)
                {
                    Debug.Fail("An item is already in this slot.");
                    this[slot].Dispose();
                }

                // Set the item into the slot
                this[slot] = new ItemEntity(values);
                slot++;
            }

            _isLoading = false;
        }

        /// <summary>
        /// Updates the client controlling the User that this Inventory belongs to with all the
        /// most accurate inventory information.
        /// </summary>
        public void UpdateClient()
        {
            _inventoryUpdater.Update();
        }
    }
}