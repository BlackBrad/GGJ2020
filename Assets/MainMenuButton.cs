using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainMenuButton : MonoBehaviour, IPointerClickHandler,
    IPointerEnterHandler, IPointerExitHandler
{
    private Text m_Text = null;
    private Color m_InitialColor;
    public Color m_HoverColor;
    public Fader m_Fader;
    public float m_BlendFactor = 2.5f;
    public bool m_IsQuitButton = false;

    private float m_BlendAmount = 0.0f;
    private bool m_IsHovered = false;

    // Start is called before the first frame update
    void Start()
    {
        m_Text = GetComponent<Text>();
        m_InitialColor = m_Text.color;
    }

    // Update is called once per frame
    void Update()
    {
        float sign = m_IsHovered ? 1.0f : -1.0f;
        m_BlendAmount = Mathf.Clamp(
                m_BlendAmount + m_BlendFactor * Time.deltaTime * sign,
                0.0f, 1.0f);

        m_Text.color = Color.Lerp(m_InitialColor, m_HoverColor,
                EasingFunctions.InOutQuat(m_BlendAmount));
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        m_IsHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        m_IsHovered = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (m_IsQuitButton)
        {
            Application.Quit();
        }
        else
        {
            m_Fader.SwitchToScene();
        }
    }
}
