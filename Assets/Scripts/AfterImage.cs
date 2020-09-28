using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterImage : MonoBehaviour
{
    PlayerController player;

    ParticleSystem thisSystem;
    public bool playParticle;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Character").GetComponent<PlayerController>();
        thisSystem = GetComponent<ParticleSystem>();

        //thisSystem.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
        transform.position = player.transform.position;
        
        if (!(player.currentlyDashing || player.backDash))
        {
            gameObject.SetActive(false);
        }

        
        if (player.facingRight)
        {
            gameObject.transform.localScale = new Vector2(0.61f, gameObject.transform.localScale.y);
        }
        else
        {
            gameObject.transform.localScale = new Vector2(-0.61f, gameObject.transform.localScale.y);
            
        }

    }
}
