using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DialogButton : MonoBehaviour, IPointerClickHandler
{
    public DialogSystem m_System;
    public int m_Index;
    public DialogOption m_Option;

    private Text m_TextComponent;
    private string m_Text;

    // Start is called before the first frame update
    void Start()
    {
         m_TextComponent = GetComponent<Text>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        m_System.HandleDialogOption(m_Option);
    }

    public void Register(DialogSystem dialogSystem)
    {
        m_System = dialogSystem;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void SetOption(string text, DialogOption option)
    {
        m_Text = text;
        m_Option = option;
        gameObject.SetActive(true);

        UpdateText();
    }

    public void UpdateText()
    {
        GetComponent<Text>().text = m_Text;
    }
}
