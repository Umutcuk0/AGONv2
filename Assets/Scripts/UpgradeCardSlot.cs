using UnityEngine;
using UnityEngine.UI;

public class UpgradeCardSlot : MonoBehaviour
{
    [SerializeField] private Button selectButton;
    private UpgradeData runtimeData;

    public void SetupSlot(UpgradeData data)
    {
        runtimeData = data;

        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(OnCardClicked);
        }
    }

    private void OnCardClicked()
    {
        if (UpgradePanelUI.Instance != null)
        {
            UpgradePanelUI.Instance.ApplyUpgradeAndClose(runtimeData);
        }
    }
}