using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using EFT.UI;
using EFT.UI.DragAndDrop;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace AutoDeposit;

public class AddInventoryButtonsPatch : ModulePatch
{
    private static readonly EItemUiContextType[] AllowedScreens = [EItemUiContextType.InventoryScreen, EItemUiContextType.ScavengerInventoryScreen];
    private static readonly Dictionary<EquipmentSlot, Func<bool>> Slots = new()
    {
        { EquipmentSlot.TacticalVest, () => Settings.EnableRig.Value },
        { EquipmentSlot.Pockets, () => Settings.EnablePockets.Value },
        { EquipmentSlot.Backpack, () => Settings.EnableBackpack.Value },
        { EquipmentSlot.SecuredContainer, () => Settings.EnableSecureContainer.Value }
    };

    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(ContainersPanel), nameof(ContainersPanel.Show));
    }

    [PatchPostfix]
    public static void Postfix(Dictionary<EquipmentSlot, SlotView> ____slotViews)
    {
        if (!AllowedScreens.Contains(ItemUiContext.Instance.ContextType) || InRaid())
        {
            return;
        }

        foreach (EquipmentSlot slot in Slots.Keys)
        {
            if (!Slots[slot]() || !____slotViews.ContainsKey(slot))
            {
                continue;
            }

            SlotView slotView = ____slotViews[slot];
            AddAutoDepositButton(slotView);

            void OnChange(Item item)
            {
                // Gotta wait because the slot gridview isn't set up yet
                slotView.WaitForEndOfFrame(() => AddAutoDepositButton(slotView));
            }

            slotView.AddDisposable(slotView.Slot.ReactiveContainedItem.Subscribe(OnChange));
        }
    }

    private static void AddAutoDepositButton(SlotView slotView)
    {
        SearchableSlotView searchableSlotView = slotView as SearchableSlotView;
        SearchableItemView searchableItemView = searchableSlotView?._searchableItemView;
        ContainedGridsView gridsView = searchableItemView?.R().ContainedGridsView;
        if (gridsView == null)
        {
            return;
        }

        CompoundItem container = searchableItemView.R().LootItem;

        AutoDepositPanel autoDepositPanel = (gridsView.transform.Find("AutoDeposit")?.GetComponent<AutoDepositPanel>()) ?? AutoDepositPanel.Create(gridsView.transform);
        autoDepositPanel.Show(container);
    }

    public static bool InRaid()
    {
        var instance = Singleton<AbstractGame>.Instance;
        return instance != null && instance.InRaid;
    }
}
