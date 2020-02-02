using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Fader : MonoBehaviour
{
    public Color m_StartingColor;
    public Color m_TargetColor;
    public float m_MaxTime = 3.0f;

    private Image m_Image;

    // Start is called before the first frame update
    void Start()
    {
        m_Image = GetComponent<Image>();
    }

    public IEnumerator UpdateColor()
    {
        float m_CurrentTime = 0.0f;
        while (m_CurrentTime <= m_MaxTime)
        {
            m_CurrentTime += Time.deltaTime;
            float t = Mathf.Clamp(m_CurrentTime / m_MaxTime, 0.0f, 1.0f);
            t = EasingFunctions.OutQuat(t);
            m_Image.color = Color.Lerp(m_StartingColor, m_TargetColor, t);
            yield return new WaitForEndOfFrame();
        }

        Debug.Log("Transition to next scene");
        SceneManager.LoadScene("Scenes/FinalStudioApartment", LoadSceneMode.Single);
    }

    public void SwitchToScene()
    {
        StopAllCoroutines();
        StartCoroutine(UpdateColor());
    }
}
