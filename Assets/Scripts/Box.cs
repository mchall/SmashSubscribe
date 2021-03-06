﻿using UnityEngine;
using System.Collections;

public class Box : MonoBehaviour
{
    public Material flash;
    public int health = 1;
    public float speed = 0.06f;
    public bool trainMode;
    public bool isPox;

    Rigidbody body;
    Player player;
    public bool dead;
    int hitCount;
    float flashTime;
    bool landed;
    Vector3 destination;
    AudioHelper audioHelper;

    void Start()
    {
        body = GetComponent<Rigidbody>();
        player = FindObjectOfType<Player>();

        audioHelper = Camera.main.GetComponent<AudioHelper>();

        if (trainMode)
        {
            destination = (body.transform.position - player.transform.position).normalized * -100;
        }
    }

    void Update()
    {
        if (!dead && transform.position.y < -3)
        {
            //UserData.Instance.CurrentScore += health;
            dead = true;
            StartCoroutine(Destroy());
        }
        else if (!dead && player.invisible)
        {
            GetComponentInChildren<BodyAnimator>().enabled = false;
            body.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
        }
        else if (player.superSpeed && !dead)
        {
            body.constraints = RigidbodyConstraints.FreezeAll;
            GetComponentInChildren<BodyAnimator>().enabled = false;
        }
        else if (!dead && !landed)
        {
            body.constraints = RigidbodyConstraints.None;
            GetComponentInChildren<BodyAnimator>().enabled = true;
        }
        else if (!dead && player != null && landed)
        {
            body.constraints = RigidbodyConstraints.None;
            GetComponentInChildren<BodyAnimator>().enabled = true;

            if (!trainMode)
                body.transform.LookAt(player.transform.position);
            else
                body.transform.LookAt(destination);

            if (!isPox)
                body.MovePosition(body.position + (body.transform.forward * speed));
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!dead && collision.gameObject.tag == "Height")
        {
            landed = true;
        }

        if (collision.gameObject.tag == "Arrow" || collision.gameObject.tag == "Trolley" || (!isPox && collision.gameObject.tag == "PoxArrow"))
        {
            if (Time.time - flashTime >= 0.15f)
            {
                StartCoroutine(Flash());

                hitCount++;

                if (!dead && audioHelper != null)
                    audioHelper.Die();

                if (hitCount >= health && !dead)
                {
                    body.constraints = RigidbodyConstraints.None;

                    UserData.Instance.CurrentScore += health;
                    dead = true;

                    if (isPox)
                    {
                        GetComponent<Pox>().enabled = false;
                    }

                    GetComponentInChildren<BodyAnimator>().enabled = false;

                    body.mass = 1f;

                    var force = collision.relativeVelocity * 10;
                    body.AddForce(force);

                    if (collision.gameObject.tag == "Trolley")
                    {
                        force = collision.relativeVelocity.normalized * 15;
                        body.AddForce(new Vector3(force.x, 2, force.z), ForceMode.Impulse);
                    }

                    body.mass = 0.1f;

                    StartCoroutine(Destroy());
                }

                flashTime = Time.time;
            }
        }
        else if (!dead && collision.gameObject.tag == "Player")
        {
            var player = collision.gameObject.GetComponent<Player>();
            if (player != null && !player.invincible)
            {
                Destroy(collision.gameObject);
                FindObjectOfType<Canvas>().GetComponent<Scene>().LoseMenu();
            }
        }
    }

    IEnumerator Destroy()
    {
        yield return new WaitForSeconds(5f);
        Destroy(gameObject);
    }

    IEnumerator Flash()
    {
        if (!dead)
        {
            var renderers = GetComponentsInChildren<Renderer>();

            Material[] originals = new Material[renderers.Length];

            for (int i = 0; i < renderers.Length; i++)
            {
                originals[i] = renderers[i].material;
                renderers[i].material = flash;
            }

            yield return new WaitForSeconds(0.1f);

            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].material = originals[i];
            }
        }
    }
}