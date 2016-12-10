using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Left Click
    public const int MOUSE0 = 0;

    public GameObject G_MainCamera;
    private Camera mCamera;

    public GameObject G_ParticleSystem;
    private ParticleSystem mSelectionParticles;

    private GameObject mSelectedWorkstation;

    // Use this for initialization
    private void Start()
    {
        Debug.Assert(G_MainCamera != null, "Player needs a reference to the main camera");
        Debug.Assert(G_ParticleSystem != null, "Player needs a reference to the particle emitter");
        G_ParticleSystem.transform.position = new Vector3(100.0f, 100.0f, 100.0f);
        mCamera = G_MainCamera.GetComponent<Camera>();
        mSelectionParticles = G_ParticleSystem.GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    private void Update()
    {
        // Raycast from the camera
        RaycastHit hit;
        Ray ray = mCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit) && hit.collider.gameObject.CompareTag("Workstation"))
        {
            mSelectionParticles.transform.position = hit.collider.gameObject.transform.position + Vector3.up * 2;
            mSelectedWorkstation = hit.collider.gameObject;
            if (!mSelectionParticles.isEmitting)
            {
                mSelectionParticles.Play();
            }
        }
        else
        {
            mSelectedWorkstation = null;
            mSelectionParticles.Stop();
        }

        if (Input.GetMouseButtonDown(MOUSE0) && mSelectedWorkstation != null)
        {
            Debug.Log("Selecting " + mSelectedWorkstation.name);
        }
    }
}