using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DialogButton : MonoBehaviour, IPointerClickHandler,
    IPointerEnterHandler, IPointerExitHandler
{
    public DialogSystem m_System;
    public int m_Index;
    public DialogOption m_Option;

    private Text m_TextComponent;
    private string m_Text;

    public AudioClip m_HoverClip;
    public AudioClip m_ClickClip;

    private AudioSource m_AudioSource;

    // Start is called before the first frame update
    void Start()
    {
         m_TextComponent = GetComponent<Text>();
         m_AudioSource = GetComponent<AudioSource>();
         Color color = m_TextComponent.color;
         color.a = 0.7f;
         m_TextComponent.color = color;
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            m_AudioSource.clip = clip;
            m_AudioSource.Play();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        PlaySound(m_ClickClip);
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

    public void OnPointerEnter(PointerEventData eventData)
    {
        PlaySound(m_HoverClip);
        Color color = m_TextComponent.color;
        color.a = 1.0f;
        m_TextComponent.color = color;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
         Color color = m_TextComponent.color;
         color.a = 0.7f;
         m_TextComponent.color = color;
    }
}
