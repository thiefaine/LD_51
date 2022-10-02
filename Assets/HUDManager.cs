using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    public List<Image> images = new List<Image>();
    
    // Start is called before the first frame update
    void Start()
    {
        foreach (var img in images)
        {
            img.enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetImage(Sprite sprite, int index)
    {
        images[index].enabled = true;
        images[index].sprite = sprite;
    }
}
