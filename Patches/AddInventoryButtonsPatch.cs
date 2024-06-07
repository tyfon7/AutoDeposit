using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using EFT.UI;
using EFT.UI.DragAndDrop;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AutoDeposit
{
    public class AddInventoryButtonsPatch : ModulePatch
    {
        private static readonly EItemUiContextType[] AllowedScreens = [EItemUiContextType.InventoryScreen, EItemUiContextType.ScavengerInventoryScreen];
        private static readonly EquipmentSlot[] Slots = [EquipmentSlot.TacticalVest, EquipmentSlot.Pockets, EquipmentSlot.Backpack, EquipmentSlot.SecuredContainer];

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ContainersPanel), nameof(ContainersPanel.Show));
        }

        [PatchPostfix]
        public static void Postfix(Dictionary<EquipmentSlot, SlotView> ___dictionary_0)
        {
            if (!AllowedScreens.Contains(ItemUiContext.Instance.ContextType) || InRaid())
            {
                return;
            }

            foreach (EquipmentSlot slot in Slots)
            {
                SlotView slotView = ___dictionary_0[slot];
                AddAutoDepositButton(slotView);

                void onChange(Item item)
                {
                    // Gotta wait because the slot gridview isn't set up yet
                    slotView.WaitForEndOfFrame(() => AddAutoDepositButton(slotView));
                }

                slotView.AddDisposable(slotView.Slot.ReactiveContainedItem.Subscribe(onChange));
            }
        }

        private static void AddAutoDepositButton(SlotView slotView)
        {
            SearchableSlotView searchableSlotView = slotView as SearchableSlotView;
            SearchableItemView searchableItemView = searchableSlotView?.R().SearchableItemView;
            ContainedGridsView gridsView = searchableItemView?.R().ContainedGridsView;
            if (gridsView == null)
            {
                return;
            }

            LootItemClass container = searchableItemView.R().LootItem;

            AutoDepositPanel autoDepositPanel = gridsView.transform.Find("AutoDeposit")?.GetComponent<AutoDepositPanel>();
            if (autoDepositPanel == null)
            {
                autoDepositPanel = AutoDepositPanel.Create(gridsView.transform);
            }

            autoDepositPanel.Show(container);
        }

        public static bool InRaid()
        {
            bool? inRaid = Singleton<AbstractGame>.Instance?.InRaid;
            return inRaid.HasValue && inRaid.Value;
        }
    }
}
