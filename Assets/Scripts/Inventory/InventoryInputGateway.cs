using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 인벤토리 전용 InputActionAsset을 읽어 인벤토리 입력 이벤트를 발행한다.
/// </summary>
public sealed class InventoryInputGateway : MonoBehaviour
{
    [SerializeField] private InputActionAsset inventoryInputActions;
    [SerializeField] private string actionMapName = "Inventory";
    [SerializeField] private string toggleActionName = "Toggle";
    [SerializeField] private string closeActionName = "Close";

    private InputAction toggleAction;
    private InputAction closeAction;

    /// <summary>
    /// 인벤토리 토글 요청 시 호출된다.
    /// </summary>
    public event Action ToggleRequested;

    /// <summary>
    /// 인벤토리 닫기 요청 시 호출된다.
    /// </summary>
    public event Action CloseRequested;

    private void OnEnable()
    {
        if (inventoryInputActions == null)
        {
            return;
        }

        var actionMap = inventoryInputActions.FindActionMap(actionMapName, false);
        if (actionMap == null)
        {
            return;
        }

        toggleAction = actionMap.FindAction(toggleActionName, false);
        closeAction = actionMap.FindAction(closeActionName, false);

        if (toggleAction != null)
        {
            toggleAction.performed += OnTogglePerformed;
        }

        if (closeAction != null)
        {
            closeAction.performed += OnClosePerformed;
        }

        actionMap.Enable();
    }

    private void OnDisable()
    {
        if (toggleAction != null)
        {
            toggleAction.performed -= OnTogglePerformed;
        }

        if (closeAction != null)
        {
            closeAction.performed -= OnClosePerformed;
        }

        if (inventoryInputActions != null)
        {
            var actionMap = inventoryInputActions.FindActionMap(actionMapName, false);
            if (actionMap != null)
            {
                actionMap.Disable();
            }
        }

        toggleAction = null;
        closeAction = null;
    }

    private void OnTogglePerformed(InputAction.CallbackContext context)
    {
        ToggleRequested?.Invoke();
    }

    private void OnClosePerformed(InputAction.CallbackContext context)
    {
        CloseRequested?.Invoke();
    }
}
