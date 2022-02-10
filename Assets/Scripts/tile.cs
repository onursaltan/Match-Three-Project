using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tile : MonoBehaviour
{
    public GameObject mainprojectile;
    public ParticleSystem mainparticlesystem;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            mainprojectile.SetActive(true);
        }

        if (mainparticlesystem.IsAlive() == false)
            mainprojectile.SetActive(false);
    }
}
