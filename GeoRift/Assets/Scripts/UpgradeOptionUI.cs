using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeOptionUI : MonoBehaviour
{
    [SerializeField] Image image;
    [SerializeField] Button button;

    public void Setup(UpgradeData upgradeData, Action<UpgradeData> onClick)
    {
        image.sprite = upgradeData.sprite;
        
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick(upgradeData));
    }
}
