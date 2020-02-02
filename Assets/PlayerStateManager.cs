using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.SceneManagement;

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
    private MicrowaveController m_MicrowaveController = null;

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
                        if (DialogSystem.m_Instance.GetTaskState(appliance.m_Key) == TaskState.Incomplete)
                        {
                            SetState(PlayerState.Speaking);
                            DialogSystem.m_Instance.SetState(appliance.m_StartingState);
                            DialogSystem.m_Instance.m_ApplianceStats = appliance.m_Stats;
                            m_Facer = appliance.GetComponent<FacePlayer>();
                            if (m_Facer != null)
                            {
                                m_Facer.RotateToPlayer();
                            }
                        }
                    }
                }
                else
                {
                    Door door = hit.collider.gameObject.GetComponent<Door>();
                    if (door != null)
                    {
                        if (DialogSystem.m_Instance.AreAllTasksComplete() ||
                                DialogSystem.m_Instance.HasTask(ApplianceKey.Door))
                        {
                            DialogSystem.m_Instance.SetTaskState(
                                    ApplianceKey.Door, TaskState.Completed);

                            SetState(PlayerState.Speaking);
                            // Enable fade out and change to main menu
                            SceneManager.LoadScene("Scenes/MainMenu", LoadSceneMode.Single);
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
            Debug.Log("SetState moving");

            if (m_Facer != null)
            {
                m_Facer.RotateAwayFromPlayer();
            }
            if (m_MicrowaveController != null)
            {
                m_MicrowaveController.CloseDoor();
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
            Debug.Log("SetState speaking");
        }

        m_State = state;
    }
}
