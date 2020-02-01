using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EasingFunctions
{
    public static float InQuat(float t)
    {
        return t * t;
    }

    public static float OutQuat(float t)
    {
        return t * (2.0f - t);
    }

    public static float InOutQuat(float t)
    {
        return t<0.5f ? 2.0f*t*t : -1.0f+(4.0f-2.0f*t)*t;
    }

    public static float OutElastic(float t)
    {
        return 0.04f * t / (t-1.0f) * Mathf.Sin(25.0f * t);
    }
};

public class FacePlayer : MonoBehaviour
{
    private Quaternion m_InitialRotation;

    void Start()
    {
        m_InitialRotation = transform.rotation;
    }

    public void RotateToPlayer()
    {
        GameObject player = GameObject.FindWithTag("MainCamera");
        Debug.Assert(player != null);
        Quaternion targetRotation = Quaternion.LookRotation(player.transform.position - transform.position, Vector3.up);

        StopAllCoroutines();
        StartCoroutine(RotateToTarget(targetRotation, true));
    }

    public void RotateAwayFromPlayer()
    {
        MicrowaveController microwaveController = GetComponent<MicrowaveController>();
        if (microwaveController != null)
        {
            microwaveController.CloseDoor();
        }
        TvController tvController = GetComponent<TvController>();
        if (tvController != null)
        {
            tvController.SwitchOffScreen();
        }
        StopAllCoroutines();
        StartCoroutine(RotateToTarget(m_InitialRotation));
    }

    public IEnumerator RotateToTarget(Quaternion targetRotation, bool openDoor = false)
    {
        float currentT = 0.0f;
        float maxT = 1.9f;
        Quaternion initialRotation = transform.rotation;

        float t = 0.0f;
        while (t < 1.0f)
        {
            t = Mathf.Clamp(currentT / maxT, 0.0f, 1.0f);
            if (t >= 1.0f)
            {
                if (openDoor)
                {
                    MicrowaveController microwaveController = GetComponent<MicrowaveController>();
                    if (microwaveController != null)
                    {
                        microwaveController.OpenDoor();
                    }

                    TvController tvController = GetComponent<TvController>();
                    if (tvController != null)
                    {
                        tvController.SwitchOnScreen();
                    }
                }
            }

            currentT += Time.deltaTime;
            t = EasingFunctions.InOutQuat(t);
            Quaternion currentRotation = Quaternion.Slerp(initialRotation, targetRotation, t);
            transform.rotation = Quaternion.AngleAxis(currentRotation.eulerAngles.y, Vector3.up);
            yield return new WaitForEndOfFrame();
        }
    }
}
