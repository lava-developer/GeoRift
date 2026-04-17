using System;
using TMPro;
using UnityEngine;

public class SliderValue : MonoBehaviour
{
    [SerializeField] TMP_Text text;
    
    public void SetValue(float value)
    {
        text.text = Mathf.RoundToInt(value * 100).ToString();
    }
}
