using System.Collections.Generic;
using System.Linq;
using Comfort.Common;
using EFT;
using EFT.Communications;
using EFT.InventoryLogic;
using EFT.UI;
using EFT.UI.DragAndDrop;
using EFT.UI.Ragfair;
using UnityEngine;
using UnityEngine.UI;

namespace AutoDeposit;

public class AutoDepositPanel : MonoBehaviour
{
    private Button _button;
    private CompoundItem _container;
    private InventoryController _inventoryController;

    public void Awake()
    {
        var layoutElement = GetComponent<LayoutElement>();
        layoutElement.ignoreLayout = true;
        layoutElement.minWidth = -1f;

        var horizontalLayoutGroup = GetComponent<HorizontalLayoutGroup>();
        horizontalLayoutGroup.childAlignment = TextAnchor.MiddleRight;
        horizontalLayoutGroup.spacing = -7;
        horizontalLayoutGroup.padding = new RectOffset();
        horizontalLayoutGroup.childControlWidth = false;

        var rectTransform = GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector3.one;
        rectTransform.anchorMax = Vector3.one;
        rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x - 10f, rectTransform.offsetMin.y);
        rectTransform.anchoredPosition = new Vector2(0, rectTransform.rect.height + 4f);

        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnClick);
    }

    public void Show(CompoundItem container)
    {
        _container = container;
        _inventoryController = ItemUiContext.Instance.R().InventoryController;
        gameObject.SetActive(true);
    }

    public void OnDestroy()
    {
        _container = null;
        _inventoryController = null;
    }

    private void OnClick()
    {
        var stash = _inventoryController.Inventory.Stash;
        if (stash == null)
        {
            return;
        }

        List<Item> stashItems = stash.GetNotMergedItems().ToList();

        List<Item> items = _container.GetNotMergedItems().Reverse().ToList(); // Reverse so items get moved before their container
        foreach (Item item in items)
        {
            // Skip root
            if (item == _container)
            {
                continue;
            }

            // Don't move containers that aren't empty
            if (item is CompoundItem lootItem && lootItem.GetNotMergedItems().Any(i => i != lootItem))
            {
                continue;
            }

            List<CompoundItem> targets = [];

            foreach (var match in stashItems.Where(i => i.TemplateId == item.TemplateId))
            {
                var targetContainer = match.Parent.Container.ParentItem as CompoundItem;
                if (targetContainer != stash)
                {
                    targets.Add(targetContainer);
                }
            }

            if (!targets.Any())
            {
                continue;
            }

            var result = ItemManipulator.QuickFindAppropriatePlace(item, _inventoryController, targets, ItemManipulator.EMoveItemOrder.MoveToAnotherSide | ItemManipulator.EMoveItemOrder.IgnoreItemParent, true);
            if (result.Failed || !_inventoryController.CanExecute(result.Value))
            {
                string text = result.Error is InventoryError inventoryError ? inventoryError.GetLocalizedDescription() : result.Error.ToString();
                NotificationManager.DisplayWarningNotification(text.Localized(), ENotificationDurationType.Default);

                continue;
            }

            if (result.Value is IDestroyResult destroyResult && destroyResult.ItemsDestroyRequired)
            {
                NotificationManager.DisplayWarningNotification(new DiscardLimitsReachedError(item, destroyResult.ItemsToDestroy).GetLocalizedDescription(), ENotificationDurationType.Default);
                continue;
            }

            _inventoryController.RunNetworkTransaction(result.Value, null);
        }

        Singleton<GUISounds>.Instance.PlayItemSound(stash.ItemSound, EInventorySoundType.pickup, false);
    }

    public static AutoDepositPanel Create(Transform parent)
    {
        GameObject template = ItemUiContext.Instance._gridWindowTemplate._sortPanel.gameObject;
        GameObject clone = UnityEngine.Object.Instantiate(template, parent, false);
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
        RagFair.IconsLoader.GetIcon("/files/handbook/icon_gear_cases.png", sprite =>
        {
            bagIcon.sprite = sprite;
            bagIcon.overrideSprite = null;
            bagIcon.SetNativeSize();
        });

        var button = clone.GetComponent<Button>();
        button.onClick.RemoveAllListeners();

        return clone.AddComponent<AutoDepositPanel>();
    }
}
