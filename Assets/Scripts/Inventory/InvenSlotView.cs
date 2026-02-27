using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 인벤토리 단일 슬롯 UI 표시를 담당한다.
/// </summary>
public class InventorySlotView : MonoBehaviour
{
    [SerializeField] private Image iconImage;

    /// 슬롯에 아이템을 표시한다.
    /// <param name="itemData">표시할 아이템 데이터.</param>
    public void Bind(ItemData itemData)
    {
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
    }

    /// <summary>
    /// 슬롯 표시를 비운다.
    /// </summary>
    public void Clear()
    {
        if (iconImage == null)
        {
            return;
        }

        iconImage.sprite = null;
        iconImage.enabled = false;
    }
}
