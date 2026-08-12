using System.Reflection;
using EFT.InventoryLogic;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace AutoDeposit;

public class AddTransferButtonPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.FirstMethod(typeof(TransferItemsScreen), m => m.Name == nameof(TransferItemsScreen.Show));
    }

    [PatchPostfix]
    public static void Postfix(TransferItemsScreen __instance)
    {
        if (!Settings.EnableTransfer.Value)
        {
            return;
        }

        Transform leftGrid = __instance.transform.Find("TransferScreen/Left Person/Possessions Grid");
        AutoDepositPanel autoDepositPanel = (leftGrid.Find("AutoDeposit")?.GetComponent<AutoDepositPanel>()) ?? AutoDepositPanel.Create(leftGrid);
        if (__instance._itemsToTransferGridView.Grid.ParentItem is CompoundItem container)
        {
            autoDepositPanel.Show(container);
        }
    }
}
