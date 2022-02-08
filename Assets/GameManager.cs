using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;


    public static GameObject lastBulletCreated;

    public static int numberOfBulletsInScene;

    public static float DEFAULT_DAMAGE;

    //public static List<Bullet> allBullets;

    private void Awake()
    {
        //Instance = this;



        Application.LoadLevel("MyLevel");

        int random = Random.Range(0, 1000);

        float temp = Time.timeSinceLevelLoad;
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
         
    }

    public void CreateBook(string message)
    {

    }


}
