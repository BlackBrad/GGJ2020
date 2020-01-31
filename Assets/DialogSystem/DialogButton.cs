using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DialogButton : MonoBehaviour, IPointerClickHandler
{
    public DialogSystem m_System;
    public int m_Index;
    public DialogOptionKey m_Key;

    private Text m_TextComponent;
    private string m_Text;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Button start");
         m_TextComponent = GetComponent<Text>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("OnPointerClick");
        m_System.HandleDialogOption(m_Key);
    }

    public void Register(DialogSystem dialogSystem)
    {
        m_System = dialogSystem;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void SetOption(string text, DialogOptionKey key)
    {
        Debug.Log("SetOption: " + text);
        m_Text = text;
        m_Key = key;
        gameObject.SetActive(true);

        UpdateText();
    }

    public void UpdateText()
    {
        Debug.Log("UpdateText: " + m_Text);
        GetComponent<Text>().text = m_Text;
    }
}
