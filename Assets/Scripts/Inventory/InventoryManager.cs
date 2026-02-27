using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 인벤토리 시스템 상태를 관리하고 인벤토리 입력 게이트웨이 이벤트를 처리한다.
/// </summary>
public class InventoryManager : MonoBehaviour
{
    private InventoryInputGateway inputGateway;

    [SerializeField] private GameObject inventoryRoot;
    [SerializeField] private InvenGridView inventoryGridView;

    private readonly List<ItemData> items = new();
    private bool isOpen;

    private void Awake()
    {
        SetOpenState(false);
        inputGateway = GetComponent<InventoryInputGateway>();
        RefreshGrid();
    }

    private void OnEnable()
    {
        if (inputGateway == null)
        {
            return;
        }

        inputGateway.ToggleRequested += HandleToggleRequested;
        inputGateway.CloseRequested += HandleCloseRequested;
    }

    private void OnDisable()
    {
        if (inputGateway == null)
        {
            return;
        }

        inputGateway.ToggleRequested -= HandleToggleRequested;
        inputGateway.CloseRequested -= HandleCloseRequested;
    }

    /// <summary>
    /// 인벤토리 창을 토글한다.
    /// </summary>
    public void ToggleInventory()
    {
        SetOpenState(!isOpen);
    }

    /// <summary>
    /// 인벤토리 창을 닫는다.
    /// </summary>
    public void CloseInventory()
    {
        SetOpenState(false);
    }

    /// <summary>
    /// 인벤토리에 아이템을 추가한다.
    /// 왼쪽 위 슬롯(인덱스 0)부터 순서대로 채운다.
    /// </summary>
    /// <param name="itemData">추가할 아이템 데이터.</param>
    /// <returns>추가 성공 여부.</returns>
    public bool TryAddItem(ItemData itemData)
    {
        if (itemData == null || inventoryGridView == null)
        {
            return false;
        }

        if (items.Count >= inventoryGridView.SlotCount)
        {
            return false;
        }

        items.Add(itemData);
        RefreshGrid();
        return true;
    }

    /// <summary>
    /// 인벤토리 전체를 비운다.
    /// </summary>
    public void ClearAllItems()
    {
        items.Clear();
        RefreshGrid();
    }

    private void HandleToggleRequested()
    {
        ToggleInventory();
    }

    private void HandleCloseRequested()
    {
        CloseInventory();
    }

    private void SetOpenState(bool shouldOpen)
    {
        isOpen = shouldOpen;

        if (inventoryRoot != null)
        {
            inventoryRoot.SetActive(isOpen);
        }
    }

    private void RefreshGrid()
    {
        if (inventoryGridView == null)
        {
            return;
        }

        inventoryGridView.Render(items);
    }
}
