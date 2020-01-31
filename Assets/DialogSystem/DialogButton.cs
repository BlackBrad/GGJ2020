using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DialogButton : MonoBehaviour, IPointerClickHandler
{
    public DialogSystem m_System;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnClick()
    {
        Debug.Log("OnCLick!");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("OnPointerClick");
        m_System.TaskOnClick();
        //m_Reply.text = "This is my reply";
        //enemyHealth -= 1;
    }

    public void Register(DialogSystem dialogSystem)
    {
        m_System = dialogSystem;
    }
}
