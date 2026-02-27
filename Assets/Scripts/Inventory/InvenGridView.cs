using System.Collections.Generic;
using UnityEngine;

/// 인벤토리 그리드 전체 UI 렌더링을 담당한다.
public class InventoryGridView : MonoBehaviour
{
    [SerializeField] private List<InventorySlotView> slotViews = new();

    /// 현재 슬롯 개수를 반환한다.
    public int SlotCount => slotViews.Count;

    /// 아이템 목록을 그리드에 렌더링한다.
    /// <param name="items">렌더링할 아이템 목록.</param>
    public void Render(IReadOnlyList<ItemData> items)
    {
        for (int i = 0; i < slotViews.Count; i++)
        {
            InventorySlotView slotView = slotViews[i];

            if (slotView == null)
            {
                continue;
            }

            if (items != null && i < items.Count && items[i] != null)
            {
                slotView.Bind(items[i]);
            }
            else
            {
                slotView.Clear();
            }
        }
    }
}
