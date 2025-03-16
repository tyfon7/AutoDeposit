using System;
using System.Linq;
using System.Reflection;
using EFT.InventoryLogic;
using EFT.UI;
using EFT.UI.DragAndDrop;
using HarmonyLib;

namespace AutoDeposit
{
    public static class R
    {
        public static void Init()
        {
            // Order is significant, as some reference each other
            UIElement.InitUITypes();
            UIInputNode.InitUITypes();
            UIContext.InitTypes();
            ItemUiContext.InitTypes();
            GridWindow.InitTypes();
            SearchableSlotView.InitTypes();
            SearchableItemView.InitTypes();
        }

        public abstract class Wrapper(object value)
        {
            public object Value { get; protected set; } = value;
        }

        public class UIElement(object value) : Wrapper(value)
        {
            private static FieldInfo UIField;

            public static void InitUITypes()
            {
                UIField = AccessTools.Field(typeof(EFT.UI.UIElement), "UI");
            }

            public UIContext UI { get { return new UIContext(UIField.GetValue(Value)); } }
        }

        public class UIInputNode(object value) : Wrapper(value)
        {
            private static FieldInfo UIField;

            public static void InitUITypes()
            {
                UIField = AccessTools.Field(typeof(EFT.UI.UIInputNode), "UI");
            }

            public UIContext UI { get { return new UIContext(UIField.GetValue(Value)); } }
        }

        public class UIContext(object value) : Wrapper(value)
        {
            public static Type Type { get; private set; }
            private static MethodInfo AddDisposableActionMethod;

            public static void InitTypes()
            {
                Type = AccessTools.Field(typeof(EFT.UI.UIElement), "UI").FieldType;
                AddDisposableActionMethod = AccessTools.Method(Type, "AddDisposable", [typeof(Action)]);
            }

            public void AddDisposable(Action destroy) => AddDisposableActionMethod.Invoke(Value, [destroy]);
        }

        public class ItemUiContext(object value) : Wrapper(value)
        {
            public static Type Type { get; private set; }
            private static FieldInfo InventoryControllerField;
            private static FieldInfo GridWindowTemplateField;
            private static PropertyInfo ItemContextProperty;

            public static void InitTypes()
            {
                Type = typeof(EFT.UI.ItemUiContext);
                InventoryControllerField = AccessTools.GetDeclaredFields(Type).Single(t => t.FieldType == typeof(InventoryController));
                GridWindowTemplateField = AccessTools.Field(Type, "_gridWindowTemplate");
                ItemContextProperty = AccessTools.GetDeclaredProperties(Type).Single(p => p.PropertyType == typeof(ItemContextAbstractClass));
            }

            public InventoryController InventoryController { get { return (InventoryController)InventoryControllerField.GetValue(Value); } }
            public EFT.UI.GridWindow GridWindowTemplate { get { return (EFT.UI.GridWindow)GridWindowTemplateField.GetValue(Value); } }
            public ItemContextAbstractClass ItemContext { get { return (ItemContextAbstractClass)ItemContextProperty.GetValue(Value); } }
        }

        public class GridWindow(object value) : UIInputNode(value)
        {
            public static Type Type { get; private set; }
            private static FieldInfo GridSortPanelField;
            private static FieldInfo LootItemField;

            public static void InitTypes()
            {
                Type = typeof(EFT.UI.GridWindow);
                GridSortPanelField = AccessTools.Field(Type, "_sortPanel");
                LootItemField = AccessTools.GetDeclaredFields(Type).Single(f => f.FieldType == typeof(CompoundItem));
            }

            public EFT.UI.DragAndDrop.GridSortPanel GridSortPanel { get { return (EFT.UI.DragAndDrop.GridSortPanel)GridSortPanelField.GetValue(Value); } }
            public CompoundItem LootItem { get { return (CompoundItem)LootItemField.GetValue(Value); } }
        }

        public class SearchableSlotView(object value) : Wrapper(value)
        {
            public static Type Type { get; private set; }
            private static FieldInfo SearchableItemViewField;

            public static void InitTypes()
            {
                Type = typeof(EFT.UI.DragAndDrop.SearchableSlotView);
                SearchableItemViewField = AccessTools.Field(Type, "_searchableItemView");
            }

            public EFT.UI.DragAndDrop.SearchableItemView SearchableItemView { get { return (EFT.UI.DragAndDrop.SearchableItemView)SearchableItemViewField.GetValue(Value); } }
        }

        public class SearchableItemView(object value) : Wrapper(value)
        {
            public static Type Type { get; private set; }
            private static FieldInfo ContainedGridsViewField;
            private static FieldInfo LootItemField;

            public static void InitTypes()
            {
                Type = typeof(EFT.UI.DragAndDrop.SearchableItemView);
                ContainedGridsViewField = AccessTools.Field(Type, "containedGridsView_0");
                LootItemField = AccessTools.GetDeclaredFields(Type).Single(f => f.FieldType == typeof(CompoundItem));
            }

            public ContainedGridsView ContainedGridsView { get { return (ContainedGridsView)ContainedGridsViewField.GetValue(Value); } }
            public CompoundItem LootItem { get { return (CompoundItem)LootItemField.GetValue(Value); } }
        }
    }

    public static class RExtentensions
    {
        public static R.ItemUiContext R(this ItemUiContext value) => new(value);
        public static R.GridWindow R(this GridWindow value) => new(value);
        public static R.SearchableSlotView R(this SearchableSlotView value) => new(value);
        public static R.SearchableItemView R(this SearchableItemView value) => new(value);
    }
}
