using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 인벤토리에서 선택한 아이템의 정보를 우측 패널에 표시한다.
/// </summary>
public class InventoryItemInfoView : MonoBehaviour
{
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text itemDescriptionText;
    [SerializeField] private Image itemIconImage;

    private void Awake()
    {
        Clear();
    }

    /// <summary>
    /// 선택한 아이템 정보를 UI에 표시한다.
    /// </summary>
    /// <param name="itemData">표시할 아이템 데이터.</param>
    public void ShowItem(ItemData itemData)
    {
        if (itemData == null)
        {
            Clear();
            return;
        }

        Debug.Log("ShowItem을 호출합니다. 대상 :" + itemData.name);

        if (itemNameText != null)
        {
            itemNameText.text = itemData.ItemName;
        }

        if (itemDescriptionText != null)
        {
            itemDescriptionText.text = itemData.Description;
        }

        if (itemIconImage != null)
        {
            itemIconImage.sprite = itemData.Icon;
            itemIconImage.enabled = itemData.Icon != null;
        }
    }

    /// <summary>
    /// 표시 중인 아이템 정보를 비운다.
    /// </summary>
    public void Clear()
    {
        if (itemNameText != null)
        {
            itemNameText.text = string.Empty;
        }

        if (itemDescriptionText != null)
        {
            itemDescriptionText.text = string.Empty;
        }

        if (itemIconImage != null)
        {
            itemIconImage.sprite = null;
            itemIconImage.enabled = false;
        }
    }
}
