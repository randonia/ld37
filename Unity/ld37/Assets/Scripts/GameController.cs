using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
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
        GameEnd,
        Lose
    }

    private GameState mLastState;
    private GameState mState = GameState.Menu;
    public GameState State { get { return mState; } set { mState = value; } }
    public GameObject G_DEBUGTEXT;
    private Text mDebugText;

    #region Workstation References

    // Forgive me father for I have sinned.

    public GameObject G_MicroGame_coffee_1;
    public GameObject G_MicroGame_tea_1;
    public GameObject G_MicroGame_water_1;
    public GameObject G_MicroGame_bakery_1;

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
    public GameObject PREFAB_WORKSTATION_WATER_1;
    public GameObject PREFAB_WORKSTATION_BAKERY_1;

    #endregion Workstation Prefabs

    #region UI Elements because the GameLogic totally should dictate what the UI does

    public GameObject G_MicroGameArena;
    public GameObject G_Camera;
    public GameObject UI_LoseScreen;
    public GameObject UI_LoseScore;
    private string mLoseReason;
    private const int LOSE_QUEUE = 1;
    private const int LOSE_WAIT = 2;
    private const int LOSE_INVENTORY = 3;
    public GameObject[] UI_ToDisableOnLoss;
    public GameObject UI_LoseReason;
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

    private int mRoundNumber = 0;
    public float mRoundStartTime = -1;
    private const float kRoundDuration = 5;
    private float mLastNPCSpawn = -1;
    private int mSpawnRate = 5;

    public float RoundTime { get { return Time.time - mRoundStartTime; } }
    public string RoundTimeString { get { return string.Format("{1:D2}:{2:D2}", mRoundNumber, (int)(RoundTime / 60), Mathf.RoundToInt(RoundTime % 60)); } }

    #endregion Round-based controls

    // Use this for initialization
    private void Start()
    {
        mRoundStartTime = Time.time;
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
        //Debug.Assert(G_MicroGame_bakery_1 != null);
        //Debug.Assert(G_MicroGame_water_1 != null);
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
        Debug.Assert(UI_LoseScreen != null);
        Debug.Assert(UI_LoseScore != null);
        Debug.Assert(UI_LoseReason != null);
        Debug.Assert(UI_UpgradePanel != null);
        Debug.Assert(UI_ToDisableOnLoss != null);
        Debug.Assert(UI_ToDisableOnLoss.Length > 0);
        Debug.Assert(PREFAB_WORKSTATION_COFFEE_1 != null);
        Debug.Assert(PREFAB_WORKSTATION_TEA_1 != null);
        Debug.Assert(PREFAB_WORKSTATION_WATER_1 != null);
        Debug.Assert(PREFAB_WORKSTATION_BAKERY_1 != null);
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

        // DEFINITELY NOT DEBUG DON'T TOUCH THIS OR THE GAME BREAKS
        BuildWorkstation("water", 1);
        BuildWorkstation("coffee", 2);
        BuildWorkstation("tea", 3);
        BuildWorkstation("bakery", 4);
        CameraToPos(CAMERA_GAMEPOS);
        State = GameState.Playing;
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
        if (mActiveNPCs.Count > 9)
        {
            State = GameState.Lose;
            mLoseReason = GetLoseReason(LOSE_QUEUE);
            return;
        }
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
        State = GameState.Playing;
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
                newWorkstation = Instantiate(PREFAB_WORKSTATION_WATER_1);
                break;
            case "coffee":
                newWorkstation = Instantiate(PREFAB_WORKSTATION_COFFEE_1);
                break;
            case "tea":
                newWorkstation = Instantiate(PREFAB_WORKSTATION_TEA_1);
                break;
            case "bakery":
                newWorkstation = Instantiate(PREFAB_WORKSTATION_BAKERY_1);
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
        switch (workstationIdx)
        {
            case 1:
                newYRot = 30f;
                break;
            case 2:
                newYRot = 60f;
                break;
            case 3:
                newYRot = 120f;
                break;
            case 4:
                newYRot = 150f;
                break;
        }
        // newYRot = (workstationIdx == 1 || workstationIdx == 2) ? 0f : (workstationIdx == 3 || workstationIdx == 4) ? 90f : 180f;
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

    public void RestartGame()
    {
        SceneManager.LoadScene("ryansandbox");
    }

    // Update is called once per frame
    private void Update()
    {
        OrganizeInventory();
        OrganizeOrderingQueue();
        OrganizeWaitQueue();
        switch (State)
        {
            case GameState.Menu:
                break;
            case GameState.Playing:
                tickPlaying();
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
            case GameState.Lose:
                if (mLastState != GameState.Lose)
                {
                    InitializeLosingScreen();
                }
                break;
        }
        // Update the UI
        UI_TimerText.GetComponent<Text>().text = RoundTimeString;

        mDebugText.text = string.Format("State: {0}", State.ToString());
        mLastState = State;
    }

    private void tickPlaying()
    {
        if (mLastNPCSpawn + mSpawnRate < Time.time)
        {
            mLastNPCSpawn = Time.time;
            AddCustomer();
        }
    }

    private void InitializeLosingScreen()
    {
        UI_LoseScreen.SetActive(true);
        UI_LoseScore.GetComponent<Text>().text = RoundTimeString;
        UI_LoseReason.GetComponent<Text>().text = mLoseReason;
        foreach (GameObject ui in UI_ToDisableOnLoss)
        {
            ui.SetActive(false);
        }
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
                    State = GameState.Playing;
                    mActiveGame = null;
                    mActiveWorkstation = null;
                    break;
                case MicroGameController.MicroState.Playing:
                    break;
                case MicroGameController.MicroState.Victory:
                    WorkstationData.WorkstationType desire = mActiveGame.GetDesire();
                    Debug.Log("Victory " + desire);
                    mActiveGame.ResetGame();
                    State = GameState.Playing;
                    mActiveGame = null;
                    mActiveWorkstation = null;
                    CreateInventory(desire);
                    break;
            }
        }
    }

    public void DEBUGCREATEINVENTORY(int i)
    {
        switch (i)
        {
            case 1:
                CreateInventory(WorkstationData.WorkstationType.bakery_1);
                break;
            case 2:
                CreateInventory(WorkstationData.WorkstationType.coffee_1);
                break;
            case 3:
                CreateInventory(WorkstationData.WorkstationType.tea_1);
                break;
            case 4:
                CreateInventory(WorkstationData.WorkstationType.water_1);
                break;
        }
    }

    public void CreateInventory(WorkstationData.WorkstationType desire)
    {
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
        if (newItem == null) { return; }
        if (mInventory.Count > 14)
        {
            State = GameState.Lose;
            mLoseReason = GetLoseReason(LOSE_INVENTORY);
            return;
        }
        newItem.transform.position = new Vector3(0, 10f, 5f);
        int desireIdx = desire.ToString().IndexOf('_');
        string desireKey = desire.ToString().Substring(0, desireIdx);
        newItem.name = desireKey;
        mInventory.Add(newItem);
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
                if (mActiveNPCs.Count > 0)
                {
                    // You lose if 11+ NPCs are waiting
                    if (mWaitingZoneNPCs.Count >= 11)
                    {
                        State = GameState.Lose;
                        mLoseReason = GetLoseReason(LOSE_WAIT);
                        return;
                    }
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
            case WorkstationData.WorkstationType.bakery_1:
                // Hyper specific code I guess because #CLAMJAM
                BakeryController controller = workstation.GetComponent<BakeryController>();
                if (controller.Interact())
                {
                    controller.TakeBread();
                    CreateInventory(WorkstationData.WorkstationType.bakery_1);
                }
                return;
        }

        Debug.Assert(microGameObject != null);
        mActiveGame = microGameObject.GetComponent<MicroGameController>();
        Debug.Assert(mActiveGame != null);
        State = GameState.Microgame;
        mActiveGame.StartGame();
        CameraToPos(CAMERA_MICROGAMEPOS);
    }

    public string GetLoseReason(int reason)
    {
        string loseString = "";

        // Holy crap but for real this is so #GAMEJAM
        if (AmazingEnding())
        {
            switch (reason)
            {
                case LOSE_QUEUE:
                    loseString = "The line at your shop became so long that you made FinFeed's frontpage - 'Local Coffee Shop A Must See, You Won't Believe How Long The Lines Are!' - The town's tourism board helped you hire new staff and your shop florished. Due to the success, you were able to sell the shop while still retaining an owner's share and retired receiving a steady income from your shop.";
                    break;
                case LOSE_WAIT:
                    loseString = "Your wait times became so long that local foodies started writing apps to track the current wait time. Your popularity skyrocketed when one of the apps made a Top 10 nationwide list for 'Most Ridiculous App' and your wait times got even longer. Local entrepeneurs flocked to buy your business, and because you knew how the sausage was made, sold to the highest bidder. You retired happy with the notariety of the town as a successful tourism power. People still download the app to this day out of nostalgia for times lost.";
                    break;
                case LOSE_INVENTORY:
                    loseString = "You made more food than people ordered but always made your customers smile. Your shop eventually earned a reputation of giving out free samples alongside the orders and you became quite popular. A local entrepeneur took notice and offered to buy you out. In reality, you couldn't sustain this much overproduction and you took the offer. You are a legend among foodies in your town and retired happily.";
                    break;
            }
        }
        else if (GreatEnding())
        {
            switch (reason)
            {
                case LOSE_QUEUE:
                    loseString = "The lines at your cafe became so long that you earned a bit of a reputation around town as 'That trendy new coffee thing' which boosted your sales for a bit but only made the problem worse. Eventually you had to close up shop but you earned some notariety for your efforts.";
                    break;
                case LOSE_WAIT:
                    loseString = "Your wait times gradually became too much for all but the most hardcore fans. You earned a well respected title of 'Worth the Wait... Sometimes' in local food magazines, but ultimately you had to close up shop. Food bloggers across the region cited your amazing food but lack of time management skills.";
                    break;
                case LOSE_INVENTORY:
                    loseString = "You made too much food that people didn't order but were happy to take. You established a good relationship with the local homeless shelter, but eventually you had to cut your losses and close up shop.";
                    break;
            }
        }
        else if (GoodEnding())
        {
            switch (reason)
            {
                case LOSE_QUEUE:
                    loseString = "Your lines started getting long enough that you earned a bit of a reputation around town. Eventually people decided they didn't want to spend an hour for a cup of coffee and you had to close up shop.";
                    break;
                case LOSE_WAIT:
                    loseString = "Your wait times began to overpower your one-person production and eventually your customers stopped coming. You had to close your doors after having a decent run at starting your own business.";
                    break;
                case LOSE_INVENTORY:
                    loseString = "You made too much food that people didn't order. At first you could donate most of it but over time the costs started to eat into your profits. You had a good run but eventually had to close up shop.";
                    break;
            }
        }
        else
        {
            switch (reason)
            {
                case LOSE_QUEUE:
                    loseString = "Your line got too long and people started to write bad Yelp reviews. As time went on, you received fewer and fewer daily customers, and eventually you had to close up shop.";
                    break;
                case LOSE_WAIT:
                    loseString = "You had too many orders to fill. People started to doubt your management skills and eventually stopped coming to your store.";
                    break;
                case LOSE_INVENTORY:
                    loseString = "Your food storage began to overflow because you made too much food no one wanted. You felt bad wasting it so you started to store it in the closet, but the health inspector had something to say about that. Upon hearing the news of your low health rating, business started to dwindle until you eventually had to close up shop.";
                    break;
            }
        }
        return loseString;
    }

    private bool GoodEnding()
    {
        // 1 minutes
        return RoundTime > 1 * 60f;
    }

    private bool GreatEnding()
    {
        // 2 minutes
        return RoundTime > 2 * 60f;
    }

    private bool AmazingEnding()
    {
        // 3 minutes
        return RoundTime > 3 * 60f;
    }
}