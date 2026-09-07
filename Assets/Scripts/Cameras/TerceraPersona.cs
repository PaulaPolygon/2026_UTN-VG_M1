using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;


public class CamaraTerceraPersona : MonoBehaviour
{
    public Vector3 offset;
    public float LerpValue = 1f;
    
    private Transform target;


void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void LateUpdate()
    {

        
        transform.position = Vector3.Lerp(transform.position, target.position + offset, LerpValue * Time.deltaTime * 5f);
        transform.LookAt(target);
    }




}
