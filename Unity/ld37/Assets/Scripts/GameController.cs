using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public enum GameState
    {
        Menu,
        Playing,
        Microgame,
        Paused,
        RoundEnd,
        GameEnd
    }

    private GameState mLastState;
    public GameState mState = GameState.Menu;
    public GameObject G_DEBUGTEXT;
    private Text mDebugText;

    #region Workstation References

    // Forgive me father for I have sinned.
    public GameObject G_MicroGame_coffee_1;

    public GameObject G_MicroGame_tea_1;

    public GameObject[] G_EmptyWorkstations;

    public HashSet<WorkstationData.WorkstationType> GetAvailableDesires()
    {
        HashSet<WorkstationData.WorkstationType> allDesires = new HashSet<WorkstationData.WorkstationType>();

        for (int i = 1; i < G_Workstations.Length; ++i)
        {
            WorkstationData wsData = G_Workstations[i].GetComponent<WorkstationData>();
            if (wsData == null) { continue; }
            allDesires.Add(wsData.StationType);
        }
        return allDesires;
    }

    public GameObject[] G_Workstations;
    public MicroGameController mActiveGame;
    public GameObject mActiveWorkstation;

    #endregion Workstation References

    #region NPC Prefabs

    public GameObject[] PREFAB_NPCS;

    #endregion NPC Prefabs

    #region NPC References

    private Queue<GameObject> mActiveNPCs;
    private List<GameObject> mWaitingZoneNPCs;
    private List<GameObject> mInventory;
    private List<GameObject> mInventoryUIStack;
    public GameObject G_CashRegister;
    public GameObject G_WaitingZoneLookTarget;

    #endregion NPC References

    #region Item Prefabs

    public GameObject PREFAB_WATER_1;
    public GameObject PREFAB_COFFEE_1;
    public GameObject PREFAB_TEA_1;
    public GameObject PREFAB_CROISSANT_1;

    #endregion Item Prefabs

    #region Workstation Prefabs

    public GameObject PREFAB_WORKSTATION_COFFEE_1;
    public GameObject PREFAB_WORKSTATION_TEA_1;

    #endregion Workstation Prefabs

    #region UI Elements because the GameLogic totally should dictate what the UI does

    public GameObject G_MicroGameArena;
    public GameObject G_Camera;
    public GameObject UI_TimerText;
    public GameObject UI_WorkstationList;
    public GameObject UI_WorkstationTemplate;
    public GameObject UI_BlankWorkstationTemplate;
    public GameObject UI_UpgradePanel;
    public GameObject UI_OrderStack;
    public GameObject UI_CoffeeIcon;
    public GameObject UI_TeaIcon;
    public GameObject UI_BakeryIcon;
    public GameObject UI_WaterIcon;
    public const int CAMERA_GAMEPOS = 0;
    public const int CAMERA_MICROGAMEPOS = 1;

    #endregion UI Elements because the GameLogic totally should dictate what the UI does

    #region Round-based controls

    public float RoundTimeRemaining { get { return (mState == GameState.Playing) ? (mRoundStartTime + kRoundDuration) - Time.time : 0; } }
    private int mRoundNumber = 0;
    private float mRoundStartTime = -1;
    private const float kRoundDuration = 5;

    public string RoundTime { get { return string.Format("Round {0}\n{1}:{2:D2}", mRoundNumber, (int)(RoundTimeRemaining / 60), Mathf.RoundToInt(RoundTimeRemaining % 60)); } }

    #endregion Round-based controls

    // Use this for initialization
    private void Start()
    {
        mActiveNPCs = new Queue<GameObject>();
        mWaitingZoneNPCs = new List<GameObject>();
        mInventory = new List<GameObject>();
        mInventoryUIStack = new List<GameObject>();
        Debug.Assert(G_Camera != null);
        Debug.Assert(PREFAB_COFFEE_1 != null);
        Debug.Assert(PREFAB_WATER_1 != null);
        Debug.Assert(PREFAB_TEA_1 != null);
        Debug.Assert(PREFAB_CROISSANT_1 != null);
        Debug.Assert(G_MicroGameArena != null);
        Debug.Assert(G_MicroGame_coffee_1 != null);
        Debug.Assert(G_MicroGame_tea_1 != null);
        Debug.Assert(G_CashRegister != null);
        Debug.Assert(UI_OrderStack != null);
        Debug.Assert(UI_CoffeeIcon != null);
        Debug.Assert(UI_TeaIcon != null);
        Debug.Assert(UI_BakeryIcon != null);
        Debug.Assert(UI_WaterIcon != null);
        Debug.Assert(UI_TimerText != null);
        Debug.Assert(UI_WorkstationList != null);
        Debug.Assert(UI_WorkstationTemplate != null);
        Debug.Assert(UI_BlankWorkstationTemplate != null);
        Debug.Assert(UI_UpgradePanel != null);
        Debug.Assert(PREFAB_WORKSTATION_COFFEE_1 != null);
        Debug.Assert(PREFAB_WORKSTATION_TEA_1 != null);
        for (int i = 0; i < G_Workstations.Length; ++i)
        {
            Debug.Assert(G_Workstations[i] != null, "Workstation[" + i + "] is NULL!");
        }
        for (int j = 0; j < G_EmptyWorkstations.Length; ++j)
        {
            Debug.Assert(G_EmptyWorkstations[j] != null, "Workstation [" + j + "] is NULL!");
        }
        Debug.Assert(G_Workstations.Length > 0);
        Debug.Assert(G_EmptyWorkstations.Length > 0);
        mDebugText = G_DEBUGTEXT.GetComponent<Text>();
        // Move this into user-clicked-start territory
        //Debug.Log("STARTING ROUND IN DEBUG");
        //StartRound();
        // Builds and creates the workstation list!
        //InitializeWorkstationList();
        // Debug for building workstation 3 as coffee
        BuildWorkstation("tea", 3);
        BuildWorkstation("coffee", 2);
        //CameraToPos(CAMERA_GAMEPOS);
    }

    public void ToggleWorkstationList()
    {
        UI_UpgradePanel.SetActive(!UI_UpgradePanel.activeSelf);
        if (UI_UpgradePanel.activeSelf)
        {
            InitializeWorkstationList();
        }
        GUI.FocusControl(null);
    }

    public void AddCustomer()
    {
        // 9 is the max
        if (mActiveNPCs.Count > 9) { return; }
        int randomNPC = (int)(UnityEngine.Random.Range(0f, 1f) * PREFAB_NPCS.Length);
        GameObject newCustomer = Instantiate(PREFAB_NPCS[randomNPC]);
        Vector3[] queue = iTweenPath.GetPath("Customer Line");
        newCustomer.transform.position = queue[queue.Length - 1];
        CustomerController customer = newCustomer.GetComponent<CustomerController>();
        customer.G_GameController = gameObject;
        customer.G_CashRegister = G_CashRegister;
        customer.MoveToQueuePosition(mActiveNPCs.Count);
        mActiveNPCs.Enqueue(newCustomer);
    }

    public void StartRound()
    {
        mState = GameState.Playing;
        mRoundNumber++;
        mRoundStartTime = Time.time;
    }

    public void BuildWorkstationCallback(string parameters)
    {
        // This is so janky but #CLAMJAM
        string[] callbackParams = parameters.Split(',');
        string type = callbackParams[0];
        string buttonName = EventSystem.current.currentSelectedGameObject.name;
        string[] buttonInfo = buttonName.Split('_');
        int workstationIdx = int.Parse(buttonInfo[1]);
        BuildWorkstation(type, workstationIdx);
        ToggleWorkstationList();
    }

    public void BuildWorkstation(string type, int workstationIdx)
    {
        GameObject newWorkstation = null;
        float newYRot = 0;
        switch (type)
        {
            case "water":
                break;
            case "coffee":
                newWorkstation = Instantiate(PREFAB_WORKSTATION_COFFEE_1);
                break;
            case "tea":
                newWorkstation = Instantiate(PREFAB_WORKSTATION_TEA_1);
                break;
            case "bakery":
                break;
            case "upgrade":
                break;
            case "delete":
                Destroy(G_Workstations[workstationIdx]);
                G_EmptyWorkstations[workstationIdx].SetActive(true);
                G_Workstations[workstationIdx] = G_EmptyWorkstations[workstationIdx];
                break;
        }

        if (newWorkstation == null) { return; }
        G_EmptyWorkstations[workstationIdx].SetActive(false);
        G_Workstations[workstationIdx].SetActive(false);
        newWorkstation.transform.position = G_EmptyWorkstations[workstationIdx].transform.position;
        newYRot = (workstationIdx == 1 || workstationIdx == 2) ? 0f : (workstationIdx == 3 || workstationIdx == 4) ? 90f : 180f;
        Quaternion currRot = newWorkstation.transform.rotation;
        currRot.Set(currRot.x, newYRot, currRot.z, currRot.w);
        newWorkstation.transform.Rotate(transform.up, newYRot);
        G_Workstations[workstationIdx] = newWorkstation;
    }

    /// <summary>
    /// Given the state of g_workstations, initialize the list
    /// </summary>
    public void InitializeWorkstationList()
    {
        int infinicheck = 0;
        // Wipe the current workstationlist (if any)
        while (UI_WorkstationList.transform.childCount > 0 && infinicheck++ < 50)
        {
            Debug.Assert(infinicheck < 48);
            Transform nextOnTheChoppingBlock = UI_WorkstationList.transform.GetChild(0);
            nextOnTheChoppingBlock.SetParent(null);
            Destroy(nextOnTheChoppingBlock.gameObject);
        }

        // Index at 1 because 0 is the register
        for (int wsIdx = 1; wsIdx < G_Workstations.Length; ++wsIdx)
        {
            GameObject workstation = G_Workstations[wsIdx];
            WorkstationData wsData = workstation.GetComponent<WorkstationData>();
            GameObject newPanel;
            if (wsData != null)
            {
                newPanel = Instantiate(UI_WorkstationTemplate);
                // Set the upgrade and delete buttton names
                newPanel.transform.FindChild("upgrade_button").name = string.Format("wsupgrade_{0}", wsIdx.ToString());
                newPanel.transform.FindChild("delete_button").name = string.Format("wsdelete_{0}", wsIdx.ToString());
                newPanel.transform.FindChild("ws_number").GetComponent<Text>().text = wsIdx.ToString();
            }
            else
            {
                newPanel = Instantiate(UI_BlankWorkstationTemplate);
                Transform t_text = newPanel.transform.FindChild("number");
                t_text.gameObject.GetComponent<Text>().text = wsIdx.ToString();

                // Rename all the group buttons because sure why not
                Transform tButtonGroup = newPanel.transform.FindChild("group");
                for (int tIdx = 0; tIdx < tButtonGroup.childCount; ++tIdx)
                {
                    tButtonGroup.GetChild(tIdx).name = string.Format("wsbutton_{0}", wsIdx);
                }
            }
            newPanel.name = string.Format("workstation_{0}", wsIdx.ToString());
            newPanel.SetActive(true);
            newPanel.transform.SetParent(UI_WorkstationList.transform);
        }
    }

    private void OnRoundEnded()
    {
        Debug.Log("Round ended");
    }

    // Update is called once per frame
    private void Update()
    {
        mLastState = mState;
        if (RoundTimeRemaining < 0)
        {
            // End the round - do the cleanup
            mState = GameState.RoundEnd;
            if (mActiveGame != null)
            {
                mActiveGame.ResetGame();
                mActiveGame = null;
            }
        }

        OrganizeInventory();
        OrganizeOrderingQueue();
        OrganizeWaitQueue();
        switch (mState)
        {
            case GameState.Menu:
                break;
            case GameState.Playing:
                break;
            case GameState.Microgame:
                tickMicrogame();
                break;
            case GameState.RoundEnd:
                // Oh yea bb this is how transitions work
                if (mLastState == GameState.Playing || mLastState == GameState.Microgame)
                {
                    OnRoundEnded();
                }
                break;
            case GameState.GameEnd:
                break;
            case GameState.Paused:
                break;
        }
        // Update the UI
        UI_TimerText.GetComponent<Text>().text = RoundTime;

        mDebugText.text = string.Format("State: {0}", mState.ToString());
    }

    public void DequeueInventory()
    {
        if (mInventory.Count > 0)
        {
            GameObject oldItem = mInventory[0];
            mInventory.RemoveAt(0);
            Destroy(oldItem);
        }
    }

    private void OrganizeInventory()
    {
        Vector3[] path = iTweenPath.GetPath("Customer Counter Queue");
        int idx = 0;
        foreach (GameObject item in mInventory)
        {
            //item.transform.position = path[idx++];
            iTween.MoveTo(item, iTween.Hash("position", path[idx++], "time", 3.25f));
        }
    }

    private void OrganizeWaitQueue()
    {
        int idx = 0;
        Stack<GameObject> customersToRemove = new Stack<GameObject>();
        foreach (GameObject customerObj in mWaitingZoneNPCs)
        {
            CustomerController customer = customerObj.GetComponent<CustomerController>();
            customer.MoveToWaitingZone(idx++);
            WorkstationData.WorkstationType desire = customer.Desire;
            int desireIdx = desire.ToString().IndexOf('_');
            string desireKey = desire.ToString().Substring(0, desireIdx);
            GameObject selectedItem = null;
            GameObject selectedUIItem = null;
            // Find the inventory item we want
            foreach (GameObject inventoryItem in mInventory)
            {
                if (inventoryItem.name == desireKey)
                {
                    selectedItem = inventoryItem;
                    break;
                }
            }
            // Find the inventory desire icon
            foreach (GameObject desireIcon in mInventoryUIStack)
                if (selectedItem != null)
                {
                    if (desireIcon.name == desireKey)
                    {
                        selectedUIItem = desireIcon;
                        Debug.Log("Destroying " + selectedItem.name);
                        mInventory.Remove(selectedItem);
                        customersToRemove.Push(customerObj);
                        Destroy(selectedItem);
                        break;
                    }
                }
            // blow it up
            if (selectedUIItem != null)
            {
                mInventoryUIStack.Remove(selectedUIItem);
                Destroy(selectedUIItem);
            }
        }
        while (customersToRemove.Count > 0)
        {
            GameObject removedCustomer = customersToRemove.Pop();
            mWaitingZoneNPCs.Remove(removedCustomer);
            removedCustomer.GetComponent<CustomerController>().LeaveStore();
            // TODO: Make them leave instead of deleting
            Destroy(removedCustomer);
        }
    }

    private void OrganizeOrderingQueue()
    {
        int idx = 0;
        foreach (GameObject customer in mActiveNPCs)
        {
            customer.GetComponent<CustomerController>().MoveToQueuePosition(idx++);
        }
    }

    public void CameraToPos(int pos)
    {
        Vector3[] path = iTweenPath.GetPath("CameraPath");
        iTween.MoveTo(G_Camera, iTween.Hash("position", path[pos], "time", 0.25f));
    }

    private void tickMicrogame()
    {
        if (mActiveGame != null)
        {
            switch (mActiveGame.State)
            {
                case MicroGameController.MicroState.Idle:
                    break;
                case MicroGameController.MicroState.Lose:
                    Debug.Log("Lose! " + mActiveGame.GetDesire());
                    mActiveGame.ResetGame();
                    mState = GameState.Playing;
                    mActiveGame = null;
                    mActiveWorkstation = null;
                    break;
                case MicroGameController.MicroState.Playing:
                    break;
                case MicroGameController.MicroState.Victory:
                    WorkstationData.WorkstationType desire = mActiveGame.GetDesire();
                    Debug.Log("Victory " + desire);
                    mActiveGame.ResetGame();
                    mState = GameState.Playing;
                    mActiveGame = null;
                    mActiveWorkstation = null;
                    GameObject newItem = null;
                    switch (desire)
                    {
                        case WorkstationData.WorkstationType.coffee_1:
                            newItem = Instantiate(PREFAB_COFFEE_1);
                            break;
                        case WorkstationData.WorkstationType.tea_1:
                            newItem = Instantiate(PREFAB_TEA_1);
                            break;
                        case WorkstationData.WorkstationType.water_1:
                            newItem = Instantiate(PREFAB_WATER_1);
                            break;
                        case WorkstationData.WorkstationType.bakery_1:
                            newItem = Instantiate(PREFAB_CROISSANT_1);
                            break;
                    }
                    if (newItem == null) { break; }
                    newItem.transform.position.Set(0, 0, 5f);
                    int desireIdx = desire.ToString().IndexOf('_');
                    string desireKey = desire.ToString().Substring(0, desireIdx);
                    newItem.name = desireKey;
                    mInventory.Add(newItem);
                    break;
            }
        }
    }

    /// <summary>
    /// Activates the workstation and starts the microgame associated with it
    /// </summary>
    /// <param name="type"></param>
    public void ActivateWorkstation(GameObject workstation)
    {
        mActiveWorkstation = workstation;
        WorkstationData wsData = workstation.GetComponent<WorkstationData>();
        if (wsData == null) { return; }
        GameObject microGameObject = null;
        switch (wsData.StationType)
        {
            case WorkstationData.WorkstationType.register:
                Debug.Log("Check out customer");
                if (mActiveNPCs.Count > 0)
                {
                    // Adds a desire to the list
                    GameObject nextInLine = mActiveNPCs.Dequeue();
                    CustomerController cust = nextInLine.GetComponent<CustomerController>();
                    int strIndex = cust.Desire.ToString().IndexOf('_');
                    if (strIndex < 0)
                    {
                        Debug.LogError(string.Format("String index for {0} is negative. Aborting customer", cust.Desire.ToString()));
                        return;
                    }
                    string desireKey = cust.Desire.ToString().Substring(0, strIndex);
                    GameObject newIcon = null;
                    switch (desireKey)
                    {
                        case "coffee":
                            newIcon = Instantiate(UI_CoffeeIcon);
                            break;
                        case "tea":
                            newIcon = Instantiate(UI_TeaIcon);
                            break;
                        case "bakery":
                            newIcon = Instantiate(UI_BakeryIcon);
                            break;
                        case "water":
                            newIcon = Instantiate(UI_WaterIcon);
                            break;
                    }
                    if (newIcon == null) { return; }
                    newIcon.name = desireKey;
                    newIcon.transform.SetParent(UI_OrderStack.transform);
                    newIcon.transform.SetAsFirstSibling();
                    newIcon.SetActive(true);
                    mInventoryUIStack.Add(newIcon);

                    cust.MoveToWaitingZone(mWaitingZoneNPCs.Count);
                    mWaitingZoneNPCs.Add(nextInLine);
                }

                return;
            case WorkstationData.WorkstationType.coffee_1:
                microGameObject = G_MicroGame_coffee_1;
                break;
            case WorkstationData.WorkstationType.tea_1:
                microGameObject = G_MicroGame_tea_1;
                break;
        }

        Debug.Assert(microGameObject != null);
        mActiveGame = microGameObject.GetComponent<MicroGameController>();
        Debug.Assert(mActiveGame != null);
        mState = GameState.Microgame;
        mActiveGame.StartGame();
        CameraToPos(CAMERA_MICROGAMEPOS);
    }
}