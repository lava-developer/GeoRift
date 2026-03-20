using System;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeUIPanel : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private UpgradeOptionUI[] optionSlots; // 3 sloty w UI

    private Action<UpgradeData> onChosen;

    public void Show(List<UpgradeData> options, Action<UpgradeData> callback)
    {
        onChosen = callback;
        panel.SetActive(true);

        for (int i = 0; i < optionSlots.Length; i++)
        {
            if (i < options.Count)
            {
                optionSlots[i].gameObject.SetActive(true);
                optionSlots[i].Setup(options[i], OnOptionClicked);
            }
            else
            {
                optionSlots[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnOptionClicked(UpgradeData chosen)
    {
        panel.SetActive(false);
        onChosen?.Invoke(chosen);
    }
}
