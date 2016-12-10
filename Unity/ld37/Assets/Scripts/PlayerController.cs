using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Left Click
    public const int MOUSE0 = 0;

    public GameObject G_GameController;
    private GameController mGameController;
    public GameObject G_MainCamera;
    private Camera mCamera;
    public GameObject[] Workstations;

    public GameObject G_ParticleSystem;
    private ParticleSystem mSelectionParticles;

    private GameObject mSelectedWorkstation;

    // Use this for initialization
    private void Start()
    {
        Debug.Assert(G_GameController != null);
        mGameController = G_GameController.GetComponent<GameController>();
        Debug.Assert(mGameController != null);
        Debug.Assert(G_MainCamera != null, "Player needs a reference to the main camera");
        Debug.Assert(G_ParticleSystem != null, "Player needs a reference to the particle emitter");
        G_ParticleSystem.transform.position = new Vector3(100.0f, 100.0f, 100.0f);
        mCamera = G_MainCamera.GetComponent<Camera>();
        mSelectionParticles = G_ParticleSystem.GetComponent<ParticleSystem>();
        Workstations = new GameObject[7];
        // This just keeps getting better and better
        Workstations[1] = mGameController.G_Workstation_1;
        Workstations[2] = mGameController.G_Workstation_2;
        Workstations[3] = mGameController.G_Workstation_3;
        Workstations[4] = mGameController.G_Workstation_4;
        Workstations[5] = mGameController.G_Workstation_5;
        Workstations[6] = mGameController.G_Workstation_6;
    }

    /// <summary>
    /// Use 0 for the starting location
    /// </summary>
    /// <param name="number"></param>
    private void MoveToWorkstation(int number)
    {
        Vector3 targetPos = iTweenPath.GetPath("Workstations")[number];
        iTween.MoveTo(gameObject, iTween.Hash("position", targetPos, "time", 1.0f));
        iTween.LookTo(gameObject, iTween.Hash("looktarget", Workstations[number].transform, "time", 1.0f));
    }

    // Update is called once per frame
    private void Update()
    {
        // Oh god
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            MoveToWorkstation(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            MoveToWorkstation(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            MoveToWorkstation(3);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            MoveToWorkstation(4);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            MoveToWorkstation(5);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            MoveToWorkstation(6);
        }

        if (mGameController.mState != GameController.GameState.Microgame)
        {
            // Raycast from the camera
            RaycastHit hit;
            Ray ray = mCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit) && hit.collider.gameObject.CompareTag("Workstation"))
            {
                mSelectionParticles.transform.position = hit.collider.gameObject.transform.position + Vector3.up * 2;
                mSelectedWorkstation = hit.collider.gameObject;
                iTween.LookTo(gameObject, iTween.Hash("looktarget", mSelectedWorkstation.transform, "time", 0.25f));
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
                mSelectionParticles.Stop();
                WorkstationData wsData = mSelectedWorkstation.GetComponent<WorkstationData>();
                mGameController.SelectWorkstation(wsData.StationType);
            }
        }
    }
}