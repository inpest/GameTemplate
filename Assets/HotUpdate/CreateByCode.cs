using UnityEngine;
using UnityEngine.UI;
using YooAsset;

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
        ao.Call("Init", "taptap", "12489"); // 平台 ， appid
        

        Debug.Log("游戏初始化成功");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            ao.Call("Show", "57000");
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            var package = YooAssets.GetPackage("DefaultPackage");
            package.LoadSceneAsync("HotUpdataScene");
        }
    }

    public void OnReward()
    {
        num++;
        num_text.text = num.ToString();
    }
}
