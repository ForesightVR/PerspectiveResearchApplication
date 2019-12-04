using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class Keyboard : MonoBehaviour
{
    public List<Button> inputKeys;
    public GameObject defaultKeyboard;
    
    private TMP_InputField _InputField;

    private void Awake()
    {
        #if UNITY_WEBGL
        foreach (Button button in inputKeys)
            button.onClick.AddListener(() => Type(button));
        #endif
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && !IsPointerOverUIObject())
            CloseKeyboard();

    }
    public TMP_InputField InputField
    {
        get { return _InputField; }
        set {
#if UNITY_WEBGL
            defaultKeyboard.SetActive(true);
            gameObject.SetActive(true);
#endif
            _InputField = value;
        } 
    }

    public void Type(Button button)
    {
        TextMeshProUGUI text = button.GetComponentInChildren<TextMeshProUGUI>();
        InputField.text += text.text;
    }

    public void Backspace()
    {
        if (InputField.text.Length > 0)
        {
            InputField.text = InputField.text.Remove(InputField.text.Length - 1);
        }
    }

    public void CloseKeyboard()
    {
        InputField = null;
        gameObject.SetActive(false);
    }

    private bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();

        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        foreach (RaycastResult result in results)
        {
            if (result.gameObject.tag == "Keyboard")
                return true;
        }

        return false;
    }
}
