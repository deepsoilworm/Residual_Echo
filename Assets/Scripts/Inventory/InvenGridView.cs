using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 인벤토리 그리드 전체 UI 렌더링을 담당한다.
/// </summary>
public class InvenGridView : MonoBehaviour
{
    [SerializeField] private List<InvenSlotView> slotViews = new();

    /// <summary>
    /// 슬롯에서 아이템이 선택될 때 호출된다.
    /// </summary>
    public event Action<ItemData> SlotItemSelected;

    private void OnEnable()
    {
        for (int i = 0; i < slotViews.Count; i++)
        {
            var slotView = slotViews[i];
            if (slotView == null)
            {
                continue;
            }

            slotView.SlotClicked += HandleSlotClicked;
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < slotViews.Count; i++)
        {
            var slotView = slotViews[i];
            if (slotView == null)
            {
                continue;
            }

            slotView.SlotClicked -= HandleSlotClicked;
        }
    }

    /// <summary>
    /// 현재 슬롯 개수를 반환한다.
    /// </summary>
    public int SlotCount => slotViews.Count;

    /// <summary>
    /// 아이템 목록을 그리드에 렌더링한다.
    /// </summary>
    /// <param name="items">렌더링할 아이템 목록.</param>
    public void Render(IReadOnlyList<ItemData> items)
    {
        for (int i = 0; i < slotViews.Count; i++)
        {
            InvenSlotView slotView = slotViews[i];

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

    private void HandleSlotClicked(ItemData itemData)
    {
        SlotItemSelected?.Invoke(itemData);
    }
}
