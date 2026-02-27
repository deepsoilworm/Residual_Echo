using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
    [SerializeField] private string itemId;
    [SerializeField] private string itemName;
    [SerializeField] [TextArea] private string description;
    [SerializeField] private Sprite icon;

    /// <summary>
    /// 아이템 고유 ID.
    /// </summary>
    public string ItemId => itemId;

    /// <summary>
    /// 아이템 표시 이름.
    /// </summary>
    public string ItemName => itemName;

    /// <summary>
    /// 아이템 설명.
    /// </summary>
    public string Description => description;

    /// <summary>
    /// 아이템 아이콘.
    /// </summary>
    public Sprite Icon => icon;
}