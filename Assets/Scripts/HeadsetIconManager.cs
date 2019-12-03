using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeadsetIconManager : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI idText;

    public void Initialize(int id)
    {
        idText.text = id.ToString();
        Ready(false);
    }

    public void Ready(bool isReady)
    {
        if(isReady)
            icon.color = Color.green;
        else
            icon.color = Color.red;
    }
}
