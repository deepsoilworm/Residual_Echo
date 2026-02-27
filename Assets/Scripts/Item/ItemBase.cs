using UnityEngine;
using ResidualEcho.Common.Interfaces;

/// <summary>
/// 월드에 배치된 아이템 오브젝트의 상호작용 및 인벤토리 추가를 처리한다.
/// </summary>
public class ItemBase : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemData itemData;
    [SerializeField] private string defaultInteractionPrompt = "아이템 줍기";

    private InventoryManager inventoryManager;

    private bool isCollected;

    /// <summary>
    /// 상호작용 UI에 표시할 문구를 반환한다.
    /// </summary>
    public string InteractionPrompt
    {
        get
        {
            if (itemData == null || string.IsNullOrWhiteSpace(itemData.ItemName))
            {
                return defaultInteractionPrompt;
            }

            return $"{itemData.ItemName} 줍기";
        }
    }

    private void Awake()
    {
        if (inventoryManager == null)
        {
            inventoryManager = FindFirstObjectByType<InventoryManager>();
        }
    }

    /// <summary>
    /// 상호작용 성공 시 인벤토리에 아이템을 추가하고 월드 오브젝트를 비활성화한다.
    /// </summary>
    public void Interact()
    {
        if (isCollected || itemData == null || inventoryManager == null)
        {
            return;
        }

        if (!inventoryManager.TryAddItem(itemData))
        {
            return;
        }

        isCollected = true;
        gameObject.SetActive(false);
    }
}
