using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nade : MonoBehaviour
{
    private Rigidbody2D rb;
    private Explosive explosive;
    public float radius = 5f;
    private bool hasExploded = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        explosive = GetComponent<Explosive>();
    }
    private void Start()
    {
        Invoke("Explode", 3f);
    }
    public void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        Collider2D[] objects = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (Collider2D obj in objects)
        {
            Bom bom = obj.GetComponent<Bom>();
            if (bom != null)
            {
                bom.Explode();
                Destroy(bom.gameObject);
            }
            Nade nade = obj.GetComponent<Nade>();
            if (nade != null)
            {
                nade.Explode();
                Destroy(nade.gameObject);
            }
            else if (obj.gameObject.CompareTag("Enemy"))
            {
                explosive.HandleEnemy(obj);
            }
            else
            {
                explosive.ApplyForceToObject(obj);
            }
        }
        Destroy(gameObject);
    }
}