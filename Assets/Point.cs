using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Point : MonoBehaviour
{
    public GameObject[] points;
    public bool clicked;
    public Text texto;
    public Text last_Score;
    public GameObject camera;
    List<GameObject> aux = null;
    int coin;
    public int timer;
    int count;
    int touches = 0;
    public AudioClip audio;
    public GameObject timeraBar;
    public float time;
    public bool play;
    public GameObject playbutton;
    // Use this for initialization
    void Start()
    {
        texto.GetComponent<Text>().text = "Coins:" + coin;
        timeraBar.GetComponent<Scrollbar>().size = 1;
        coin = 0;
        count = 1;

    }

    // Update is called once per frame
    void Update()
    {
        timer++;

        controller();

       

    }
    public void startGame(){
        play =!play;

        playbutton.SetActive(false);
    }
    public List<GameObject> instancerList(int c)
    {
        List<GameObject> l = new List<GameObject>();
        for (int i = 0; i < c; i++)
        {
            if(i%2==0){
                camera.GetComponent<Transform>().position = new Vector3(camera.transform.position.x, camera.transform.position.y + 0.1f, camera.transform.position.z);
            }
            l.Add(instancer(i));
        }
        return l;
    }
    private GameObject instancer(int c)
    {
        int i = 0;


        if(c>count-2 & c>1 ){



            i  = Random.Range(0,points.Length);
          
        }else{
            i = 0;

        }
       
        GameObject inc = Instantiate(points[i]);
        return inc;
    }
    public bool end()
    {
        return timeraBar.GetComponent<Scrollbar>().size < 0.001f;

    }
    public void explotion(GameObject c)
    {
        sond();
        GameObject exp = Instantiate(c.GetComponent<PointGameObject>().expotion);
        exp.transform.position = c.transform.position;
    }

    public void timeWin()
    {
        timeraBar.GetComponent<Scrollbar>().size = timeraBar.GetComponent<Scrollbar>().size + time * 3;

    }
    public void timelose()
    {
        timeraBar.GetComponent<Scrollbar>().size = timeraBar.GetComponent<Scrollbar>().size - time;


    }
    public void DestroytAll()
    {
        int c = 0;
        GameObject[] pointsScene = GameObject.FindGameObjectsWithTag("point");
        if (pointsScene != null)
        {
            foreach (GameObject d in pointsScene)
            {
                timeWin();
                explotion(d);
                Destroy(d);
                touches++;
                coin++;
                c++;
            }
            for (int i = 0; i < c;i++){
                camara();
            }
        }

    }
    public void sond(){
        AudioSource a= camera.GetComponent<AudioSource>();
        a.clip= audio;
        a.Play();
      //  audio

    }
    public void cubeController(){

        List<GameObject> l= new List<GameObject>();
        foreach(GameObject g in GameObject.FindGameObjectsWithTag("point")){
            l.Add(g);
        }
        l.Add(GameObject.FindGameObjectWithTag("SuperPoint"));

        foreach(GameObject d in l){
            if(d.GetComponent<Transform>().position.y<8.22f){

                d.GetComponent<Transform>().position = new Vector3(Random.Range(-3.05f, 2.87f), -9.86f, Random.Range(-15.7f, -5.4f));
            }

        }


    }

    public void camara(){
        if(camera.transform.position.y>(-8.22f)){ 
            camera.GetComponent<Transform>().position = new Vector3(camera.transform.position.x, camera.transform.position.y - 0.2f, camera.transform.position.z);

        }
    }
    public void win(){
        if (touches == count)
        {
            timeraBar.GetComponent<Scrollbar>().size = 1;
            clicked = true;
            touches = 0;
          
        }
         
        texto.GetComponent<Text>().text = "Coins:" + coin;
    }
   
    public void controller(){
        if(play==true){
            if (end() == true)
            {
                texto.GetComponent<Text>().text = "FIN";
                last_Score.GetComponent<Text>().text = "Ultimo puntaje:" + coin;
                playbutton.SetActive(true);
                play = false;
            }
            else
            {
                timelose();
                if (clicked == true)
                {
                    if (timer > 50)
                    {

                        count++;
                        timer = 0;
                    }
                    List<GameObject> l = instancerList(count);


                    foreach (GameObject g in l)
                    {

                        g.transform.position = new Vector3(Random.Range(-3.05f, 2.87f), -9.86f, Random.Range(-15.7f, -5.4f));
                    }
                    aux = l;
                    clicked = !clicked;
                }
                if ((Input.touchCount > 0) && (Input.GetTouch(0).phase == TouchPhase.Began))
                {
                    Ray raycast = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
                    RaycastHit raycastHit;
                    if (Physics.Raycast(raycast, out raycastHit))
                    {



                        //OR with Tag
                        if (raycastHit.collider.CompareTag("SuperPoint"))
                        {

                            Collider c = raycastHit.collider;
                            explotion(c.gameObject);
                            Destroy(c.gameObject);
                            DestroytAll();
                            touches = count;

                        }
                        if (raycastHit.collider.CompareTag("point"))
                        {
                            timeWin();
                            Collider c = raycastHit.collider;
                            explotion(c.gameObject);
                            Destroy(c.gameObject);

                            camara();

                            touches++;
                            coin++;
                        }
                        win();

                    }
                }
            }
        }else{
            texto.GetComponent<Text>().text = "Coins:" + coin;
            timeraBar.GetComponent<Scrollbar>().size = 1;
            coin = 0;
            count = 1;
            timer = 0;
        }
      

    }
}