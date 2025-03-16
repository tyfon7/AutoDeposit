using System.Reflection;
using EFT.InventoryLogic;
using EFT.UI;
using EFT.UI.DragAndDrop;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace AutoDeposit
{
    public class AddTransferButtonPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.FirstMethod(typeof(TransferItemsScreen), m => m.Name == nameof(TransferItemsScreen.Show));
        }

        [PatchPostfix]
        public static void Postfix(TransferItemsScreen __instance, GridView ____itemsToTransferGridView)
        {
            if (!Settings.EnableTransfer.Value)
            {
                return;
            }

            Transform leftGrid = __instance.transform.Find("TransferScreen/Left Person/Possessions Grid");
            AutoDepositPanel autoDepositPanel = (leftGrid.Find("AutoDeposit")?.GetComponent<AutoDepositPanel>()) ?? AutoDepositPanel.Create(leftGrid);
            if (____itemsToTransferGridView.Grid.ParentItem is CompoundItem container)
            {
                autoDepositPanel.Show(container);
            }
        }
    }
}
