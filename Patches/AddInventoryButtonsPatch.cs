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
using UnityEngine;
using UnityEngine.UI;

namespace AutoDeposit
{
    public class AddInventoryButtonsPatch : ModulePatch
    {
        private static readonly EItemUiContextType[] AllowedScreens = [EItemUiContextType.InventoryScreen, EItemUiContextType.ScavengerInventoryScreen];

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

            AddAutoDepositButton(___dictionary_0[EquipmentSlot.TacticalVest]);
            AddAutoDepositButton(___dictionary_0[EquipmentSlot.Pockets]);
            AddAutoDepositButton(___dictionary_0[EquipmentSlot.Backpack]);
            AddAutoDepositButton(___dictionary_0[EquipmentSlot.SecuredContainer]);
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
                GameObject template = ItemUiContext.Instance.R().GridWindowTemplate.R().GridSortPanel.gameObject;
                GameObject clone = UnityEngine.Object.Instantiate(template, gridsView.transform, false);
                clone.name = "AutoDeposit";

                var gridSortPanel = clone.GetComponent<GridSortPanel>();
                UnityEngine.Object.Destroy(gridSortPanel);

                var text = clone.transform.Find("Text").gameObject;
                UnityEngine.Object.Destroy(text);

                Transform iconTransform = clone.transform.Find("SortIcon");
                iconTransform.name = "ArrowIcon";

                Transform iconTransform2 = UnityEngine.Object.Instantiate(iconTransform, clone.transform, false);
                iconTransform2.name = "BagIcon";

                Image arrowIcon = iconTransform.GetComponent<Image>();
                arrowIcon.sprite = EFTHardSettings.Instance.StaticIcons.GetAttributeIcon(EItemAttributeId.EffectiveDist);
                arrowIcon.transform.Rotate(0f, 0f, -45f);
                arrowIcon.overrideSprite = null;
                arrowIcon.SetNativeSize();

                Image bagIcon = iconTransform2.GetComponent<Image>();
                RagFairClass.IconsLoader.GetIcon("/files/handbook/icon_gear_cases.png", sprite =>
                {
                    bagIcon.sprite = sprite;
                    bagIcon.overrideSprite = null;
                    bagIcon.SetNativeSize();
                });

                var button = clone.GetComponent<Button>();
                button.onClick.RemoveAllListeners();

                autoDepositPanel = clone.AddComponent<AutoDepositPanel>();
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
