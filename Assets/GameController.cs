using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public static GameController instance;
    public static bool isLeaving;
    public static bool isAlarm;
    public static float alarmDuration = 10f;
    private static float alarmTimer = 0;

    private AudioSource audioSource;
    private AudioSource alarmTriggerSource;

    public AudioClip alarmBgm;
    public AudioClip normalBgm;
    public AudioClip alarmTriggerSound;

    // Start is called before the first frame update
    void Start()
    {
        isLeaving = false;
        isAlarm = false;

        audioSource = GetComponent<AudioSource>();
        instance = this;

        alarmTriggerSource = gameObject.AddComponent<AudioSource>();
        alarmTriggerSource.loop = true;
        alarmTriggerSource.clip = alarmTriggerSound;
    }

    // Update is called once per frame
    void Update()
    {
        if(isAlarm)
        {
            alarmTimer -= Time.deltaTime;

            if(alarmTimer < 0)
            {
                isAlarm = false;
                alarmTimer = 0;
                instance.audioSource.clip = instance.normalBgm;
                instance.audioSource.Play();
                instance.alarmTriggerSource.Stop();
            }
        }
    }

    public void StartLeaving()
    {
        isLeaving = true;
        StartCoroutine(LoadSceneCoroutine());
    }

    IEnumerator LoadSceneCoroutine()
    {
        yield return new WaitForSeconds(6);
        SceneManager.LoadScene(0);
    }

    public static void SetAlarm()
    {
        isAlarm = true;
        alarmTimer = alarmDuration;
        instance.audioSource.clip = instance.alarmBgm;
        instance.audioSource.Play();
        instance.alarmTriggerSource.Play();
    }

    void OnGUI()
    {
        GUI.Label(new Rect(0, 0, Screen.width, Screen.height), string.Format("Alarm:{0}, Timer:{1}", isAlarm, alarmTimer));
    }

}
