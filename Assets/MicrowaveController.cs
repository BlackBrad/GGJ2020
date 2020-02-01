using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MicrowaveController : MonoBehaviour
{
    public Light m_Light;
    public Transform m_Door;
    public float m_DoorRate;

    public float m_OpenDoorY = 120.0f;

    public float m_CurrentT = 0.0f;
    public float m_MaxT = 0.9f;

    private Quaternion m_OpenDoorRotation;
    private Quaternion m_ClosedDoorRotation;

    // Start is called before the first frame update
    void Start()
    {
        m_Light.enabled = false;

        m_ClosedDoorRotation = m_Door.localRotation;
        m_OpenDoorRotation = Quaternion.AngleAxis(m_OpenDoorY, Vector3.up);
    }

    public IEnumerator OpenDoorAnimation()
    {
        m_CurrentT = 0.0f;
        float t = 0.0f;
        while (t < 1.0f)
        {
            t = Mathf.Clamp(m_CurrentT / m_MaxT, 0.0f, 1.0f);
            if (t >= 0.9f)
            {
                m_Light.enabled = true;
            }

            m_CurrentT += Time.deltaTime;
            t = EasingFunctions.OutElastic(t);
            m_Door.localRotation = Quaternion.Slerp(m_ClosedDoorRotation, m_OpenDoorRotation, t);
            Debug.Log(m_Door.rotation);
            yield return new WaitForEndOfFrame();
        }
    }

    public IEnumerator CloseDoorAnimation()
    {
        m_CurrentT = 0.0f;
        float t = 0.0f;
        m_Light.enabled = false;

        while (t < 1.0f)
        {
            t = Mathf.Clamp(m_CurrentT / m_MaxT, 0.0f, 1.0f);
            m_CurrentT += Time.deltaTime;
            t = EasingFunctions.InOutQuat(t);
            m_Door.localRotation = Quaternion.Slerp(m_OpenDoorRotation, m_ClosedDoorRotation, t);
            yield return new WaitForEndOfFrame();
        }
    }

    public void OpenDoor()
    {
        StopAllCoroutines();
        StartCoroutine(OpenDoorAnimation());
    }

    public void CloseDoor()
    {
        StopAllCoroutines();
        StartCoroutine(CloseDoorAnimation());
    }
}
