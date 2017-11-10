using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Snake : MonoBehaviour {
    private Snake next;
    static public Action<string, string, string, Vector3> hit;
    public void SetNext(Snake IN)
    {
        next = IN;
    }
    public Snake GetNext()
    {
        return next;
    }

    public void RemoveTail()
    {
        Destroy(this.gameObject);
    }
    void OnTriggerEnter(Collider other)
    {
        try
        {
            if (hit != null)
            {
                if (tag != other.tag)
                {
                    if (tag != "Tail2" && other.tag != "Tail2")
                    {
                        if (tag != "Player" && other.tag != "Player")
                        {
                            print(tag + " collide to" + other.tag);
                            hit(other.tag, name, other.name, other.transform.position);
                        }
                    }
                }
            }
            if (other.tag == "Food")
            {
                Destroy(other.gameObject);
            }
        }
        catch (System.Exception) { }
    }

}
