using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Michsky.UI.ModernUIPack;
using UnityEngine.UI;

public class QuestUser : MonoBehaviour
{
    public TextMeshProUGUI questIDField;
    public TMP_InputField userIDField;
    public SwitchManager groupIDField;

    private void OnEnable()
    {
        userIDField.onDeselect.AddListener( delegate { ApplicationManager.Instance.SaveFields(); });
    }
}
