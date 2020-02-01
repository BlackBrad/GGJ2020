using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TvController : MonoBehaviour
{
    public GameObject m_FaceGroup;
    public GameObject m_FlatFace;
    private bool m_FaceOn = false;

    // Start is called before the first frame update
    void Start()
    {
        SwitchOffScreen();
    }

    public IEnumerator DisplayFace()
    {
        float currentT = 0.0f;
        float maxT = 1.5f;

        while (currentT < maxT)
        {
            float t = Mathf.Clamp(currentT / maxT, 0.0f, 1.0f);
            currentT += Time.deltaTime;
            if (t < 0.9f)
            {
                t = Mathf.Abs(Mathf.Sin(t * 16.423f));
                if (t < 0.4f)
                {
                    m_FaceGroup.SetActive(true);
                    m_FlatFace.SetActive(false);
                }
                else
                {
                    m_FlatFace.SetActive(true);
                    m_FaceGroup.SetActive(false);
                }
            }
            else
            {
                m_FaceGroup.SetActive(true);
                m_FlatFace.SetActive(false);
            }
            yield return new WaitForEndOfFrame();
        }
    }

    public void SwitchOnScreen()
    {
        StopAllCoroutines();
        StartCoroutine(DisplayFace());
    }

    public void SwitchOffScreen()
    {
        m_FlatFace.SetActive(true);
        m_FaceGroup.SetActive(false);
    }
}
