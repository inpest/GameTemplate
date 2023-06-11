using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Numbers : MonoBehaviour
{
    public TextMeshProUGUI numTmp;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        numTmp.text = Time.time.ToString();
    }
}
