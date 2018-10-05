using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * @2BitsLab
 * www.2bitslab.com
 * 
*/
public class ListMenu : MonoBehaviour
{

    //Pather of the buttons.
    public GameObject scrollBox;
    //buttons for the menu.
    public GameObject[] buttons;
    public int space;


    public void Start()
    {
        List();
    }
    public void List()
    {
        int y = 0;
       
            foreach (GameObject n in buttons)
            {
                Debug.Log("Starting...");
                var btn = Instantiate(n);
                btn.SetActive(true);
                btn.transform.SetParent(scrollBox.transform, false);
                RectTransform rt = btn.GetComponent<RectTransform>();
                rt.anchoredPosition = rt.transform.position;
                y = (y + space);
                rt.localPosition = new Vector2(0,-y);
                
                
               
            }
    }
}
