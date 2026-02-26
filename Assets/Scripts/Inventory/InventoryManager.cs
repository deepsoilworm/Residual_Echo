using UnityEngine;

/// <summary>
/// 인벤토리 시스템 상태를 관리하고 인벤토리 입력 게이트웨이 이벤트를 처리한다.
/// </summary>
public class InventoryManager : MonoBehaviour
{
    private InventoryInputGateway inputGateway;
    [SerializeField] private GameObject inventoryRoot;

    private bool isOpen;

    private void Awake()
    {
        SetOpenState(false);

        inputGateway = GetComponent<InventoryInputGateway>();

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
}
