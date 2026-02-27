using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 인벤토리 단일 슬롯 UI 표시를 담당한다.
/// </summary>
public class InvenSlotView : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image iconImage;

    private ItemData boundItemData;

    /// <summary>
    /// 슬롯 클릭 시 선택된 아이템 데이터를 전달한다.
    /// </summary>
    public event Action<ItemData> SlotClicked;

    /// 슬롯에 아이템을 표시한다.
    /// <param name="itemData">표시할 아이템 데이터.</param>
    public void Bind(ItemData itemData)
    {
        boundItemData = itemData;

        if (iconImage == null)
        {
            return;
        }

        if (itemData == null || itemData.Icon == null)
        {
            Clear();
            return;
        }

        iconImage.sprite = itemData.Icon;
        iconImage.enabled = true;
        iconImage.raycastTarget = false;
    }

    /// <summary>
    /// 슬롯 표시를 비운다.
    /// </summary>
    public void Clear()
    {
        boundItemData = null;

        if (iconImage == null)
        {
            return;
        }

        iconImage.sprite = null;
        iconImage.enabled = false;
        iconImage.raycastTarget = false;
    }

    /// <summary>
    /// 슬롯 클릭 입력을 처리한다.
    /// </summary>
    /// <param name="eventData">포인터 이벤트 데이터.</param>
    public void OnPointerClick(PointerEventData eventData)
    {
        //string itemName = boundItemData != null ? boundItemData.ItemName : "(Empty)";
        //string buttonName = eventData.button.ToString();
        //Debug.Log($"[InvenSlotView] OnPointerClick - Slot: {name}, Item: {itemName}, Button: {buttonName}", this);
        SlotClicked?.Invoke(boundItemData);
    }
}
