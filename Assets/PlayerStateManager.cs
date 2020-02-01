using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using UnityStandardAssets.CrossPlatformInput;

public enum PlayerState
{
    Moving,
    Speaking,
};

public class PlayerStateManager : MonoBehaviour
{
    public float m_InteractionRange = 1.5f;
    public PlayerState m_State;
    private GameObject m_UiCanvas;
    private FirstPersonController m_FirstPersonController;
    private FacePlayer m_Facer = null;

    public static PlayerStateManager m_Instance = null;

    // Start is called before the first frame update
    void Start()
    {
        m_UiCanvas = GameObject.FindWithTag("ui_canvas");
        Debug.Assert(m_UiCanvas != null);
        m_FirstPersonController = this.transform.parent.GetComponent<FirstPersonController>();
        SetState(PlayerState.Moving);
        m_Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (CrossPlatformInputManager.GetButtonDown("Interact"))
        {
            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, m_InteractionRange, Physics.DefaultRaycastLayers))
            {
                Appliance appliance = hit.collider.gameObject.GetComponent<Appliance>();
                if (appliance != null)
                {
                    Debug.Log("Hit appliance");
                    if (m_State == PlayerState.Moving)
                    {
                        SetState(PlayerState.Speaking);
                        DialogSystem.m_Instance.SetState(appliance.m_StartingState);
                        m_Facer = appliance.GetComponent<FacePlayer>();
                        if (m_Facer != null)
                        {
                            m_Facer.m_IsActive = true;
                        }
                    }
                }
            }

        }
    }

    public void SetState(PlayerState state)
    {
        if (state == PlayerState.Moving)
        {
            // Disable UI
            m_UiCanvas.SetActive(false);
            // Enable mouse look
            m_FirstPersonController.m_EnableMouseLook = true;
            m_FirstPersonController.m_DisableMovement = false;
            m_FirstPersonController.m_MouseLook.SetCursorLock(true);
            m_FirstPersonController.m_MouseLook.UpdateCursorLock();

            if (m_Facer != null)
            {
                m_Facer.m_IsActive = false;
            }
        }
        else if (state == PlayerState.Speaking)
        {
            // Enable UI
            m_UiCanvas.SetActive(true);
            // Disable mouse look
            m_FirstPersonController.m_EnableMouseLook = false;
            m_FirstPersonController.m_DisableMovement = true;
            m_FirstPersonController.m_MouseLook.SetCursorLock(false);
            m_FirstPersonController.m_MouseLook.UpdateCursorLock();
        }

        m_State = state;
    }
}
