using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class StoveManager : MonoBehaviour
{
    [Header("Dummy Timer")]
    public int timeCount;

    [Space(10)]
    public float timeSpeed = 1;
    [SerializeField] private Image itemPreview;
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private Button takeItButton;
    [SerializeField] private Slider sliderObjective;

    [Space(10)]
    public UnityEvent cancelListener;
    public UnityEvent backListener;
    public UnityEvent takeItListener;

    int timeCreated;
    float incrementAmount = 1.0f;
    float incrementDuration = 0.2f;

    bool isIncrementing = false;
    bool startCountdown = false;
    // Start is called before the first frame update
    void Start()
    {
        //dummy load Time Count
        timeCount = PlayerPrefs.GetInt("dummyTimeCount", 0);

        //if have time Created - Automatic load
        //dummy load Time Created
        if (PlayerPrefs.HasKey("TimeCreated") && PlayerPrefs.HasKey("CountDown"))
        {
            startStove(PlayerPrefs.GetInt("CountDown"));
            takeItButton.interactable = false;
        }
        else
        {
            PlayerPrefs.DeleteKey("TimeCreated");
            PlayerPrefs.DeleteKey("CountDown");

            countdownText.text = "Finish!";
            takeItButton.interactable = true;
            sliderObjective.value = sliderObjective.maxValue;
        }


        StartCoroutine(dummyTimerCount());

    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            startStove();
        }
    }

    //Ini serharusnya di hapus, karena ini hanya dummy timer count untuk di scene ini. Npc ville seharusnya sudah ada
    IEnumerator dummyTimerCount()
    {

        yield return new WaitForSeconds(timeSpeed);

        timeCount++;
        //dummy save Time Count;
        PlayerPrefs.SetInt("dummyTimeCount", timeCount);

        StartCoroutine(dummyTimerCount());
    }
    //----------------------------------------------------------------------------------------------- Sampe sini

    IEnumerator countDownInSecond(int countDown)
    {
        setCountdownText(countDown - (timeCount - timeCreated));
        PlayerPrefs.SetInt("TimeCreated", timeCreated);

        //Check If Finish CountDown
        if(countDown - (timeCount - timeCreated) <= 0)
        {
            startCountdown = false;
            PlayerPrefs.DeleteKey("TimeCreated");
            PlayerPrefs.DeleteKey("CountDown");

            countdownText.text = "Finish!";
            takeItButton.interactable = true;
        }

        yield return new WaitForSeconds(timeSpeed);
        StartCoroutine(IncrementSliderSmoothly(timeCount - timeCreated));


        if (startCountdown)
            StartCoroutine(countDownInSecond(countDown));

    }

    IEnumerator IncrementSliderSmoothly(int startValue)
    {
        isIncrementing = true;

        float targetValue = Mathf.Clamp(startValue + incrementAmount, 0, sliderObjective.maxValue);
        float currentTime = 0;

        while (currentTime < incrementDuration)
        {
            currentTime += Time.deltaTime;
            float newValue = Mathf.Lerp(startValue, targetValue, currentTime / incrementDuration);
            sliderObjective.value = newValue;

            yield return null;
        }

        sliderObjective.value = targetValue;
        isIncrementing = false;
    }


    public void startStove(int countDown = 30, Sprite sprite = null, int itemId = 0)
    {
        if(!startCountdown)
        {
            takeItButton.interactable = false;

            startCountdown = true;
            timeCreated = PlayerPrefs.GetInt("TimeCreated", timeCount);

            if (!PlayerPrefs.HasKey("ItemId"))
            {
                PlayerPrefs.SetInt("ItemId", itemId);
            }

            PlayerPrefs.SetInt("CountDown", countDown);


            if (sprite) setItemPreview(sprite);

            sliderObjective.maxValue = PlayerPrefs.GetInt("CountDown");


            StartCoroutine(IncrementSliderSmoothly(timeCount - timeCreated - 1)); //Fix bug

            StartCoroutine(countDownInSecond(countDown));
        }
 
    }
    public void setCountdownText(int c)
    {
        countdownText.text = c.ToString() + "<color=#779241>s</color>";
    }
    public void setItemPreview(Sprite sprite)
    {
        itemPreview.sprite = sprite;
    }


    public void back()
    {
        backListener.Invoke();

    }
    public void takeIt()
    {
        takeItListener.Invoke();

        //Set Item To Player Here
        //

        cancel();

    }
    public void cancel()
    {
        cancelListener.Invoke();
        countdownText.text = "";
        startCountdown = false;
        StopCoroutine(countDownInSecond(PlayerPrefs.GetInt("CountDown")));
        PlayerPrefs.DeleteKey("TimeCreated");
        PlayerPrefs.DeleteKey("CountDown");
        PlayerPrefs.DeleteKey("ItemId");

    }
}
