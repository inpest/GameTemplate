using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateByCode : MonoBehaviour
{
    public Button initBtn;
    public Button showBtn;
    public Text num_text;
    public int num = 0;
    AndroidJavaObject ao;
    void Start()
    {

        ao = new AndroidJavaObject("com.pocket.zxpa.ShowAds");

        initBtn.onClick.AddListener(() => {
            ao.Call("Init", "taptap", "12049"); // 平台 ， appid
        });
        showBtn.onClick.AddListener(() => { // 广告位id
            ao.Call("Show", "55995");
        });

        Debug.Log("游戏初始化成功");
    }

    public void OnReward()
    {
        num++;
        num_text.text = num.ToString();
    }
}
