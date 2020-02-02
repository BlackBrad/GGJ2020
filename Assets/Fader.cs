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
    public AudioClip m_IntroClip;

    private Image m_Image;
    private AudioSource m_AudioSource;

    // Start is called before the first frame update
    void Start()
    {
        m_Image = GetComponent<Image>();
        m_AudioSource = GetComponent<AudioSource>();
    }

    public IEnumerator UpdateColor()
    {
        float m_CurrentTime = 0.0f;
        float fadeTime = 3.0f;
        float introTime = 16.0f;
        float totalTime = fadeTime + introTime;
        bool isPlayingIntro = false;
        while (m_CurrentTime <= totalTime)
        {
            m_CurrentTime += Time.deltaTime;
            if (m_CurrentTime <= fadeTime)
            {
                float t = Mathf.Clamp(m_CurrentTime / fadeTime, 0.0f, 1.0f);
                t = EasingFunctions.OutQuat(t);
                m_Image.color = Color.Lerp(m_StartingColor, m_TargetColor, t);
            }
            else if (!isPlayingIntro)
            {
                m_AudioSource.clip = m_IntroClip;
                m_AudioSource.Play();
                isPlayingIntro = true;
            }
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
