using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FacePlayer : MonoBehaviour
{
    public bool m_IsActive = false;
    public float m_Speed = 10.0f;

    // Update is called once per frame
    void Update()
    {
        if (m_IsActive)
        {
            GameObject player = GameObject.FindWithTag("MainCamera");
            Debug.Assert(player != null);

            Quaternion quat = Quaternion.LookRotation(player.transform.position - transform.position, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, quat, m_Speed);
        }
    }
}
