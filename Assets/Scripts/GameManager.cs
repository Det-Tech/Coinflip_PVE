using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnitySocketIO;
using UnitySocketIO.Events;
using System.Runtime.InteropServices;
using SimpleJSON;



public class GameManager : MonoBehaviour
{
    //declared variables
    private bool isStarted ;

    private float TotalAmount;
    private float BetAmount;
    private float PreviousAmount;
    private float estimatedAmount;
    

    public  TMP_Text totalAmountText;
    public  TMP_Text winAmountText;
    public  TMP_Text oddAmountText;
    public  TMP_Text levelNumberText;
    public  TMP_Text Alerttext;
    public  TMP_Text Warning;
    public  TMP_Text estimatetext;

    public TMP_InputField betAmount;

    public Button Betbutton;
    public Button Cashbutton;
    public Button frontbutton;
    public Button backbutton;

    
    public GameObject coin;
    public GameObject betPanel;
    public GameObject CashoutPanel;
    public GameObject AlertPanel;
    public GameObject ErrorPanel;
    public GameObject EstimatedPanel;

    public SocketIOController io;

    public Animator anim;


    BetPlayer _player;


    // GameReadyStatus Send
    [DllImport("__Internal")]
    private static extern void GameReady(string msg);


    void Start()
    {
        isStarted = false;
        betPanel.SetActive(true);
        EstimatedPanel.SetActive(false);
        CashoutPanel.SetActive(false);
        AlertPanel.SetActive(false);
        ErrorPanel.SetActive(false);
        _player = new BetPlayer();
        Warning.text = "";
        totalAmountText.text = "10000.00";
        BetAmount = 10f;
        betAmount.text = "10.00";
        levelNumberText.text = "0";
        oddAmountText.text = "0.00";
        estimatetext.text = "0.00";
        PreviousAmount = float.Parse(totalAmountText.text);


        io.Connect();
        io.On("connect", (e) =>
        {
            Debug.Log("Game started");
            io.On("game start", (res) =>
            {
                StartCoroutine(BetInfo(res));
            });
            io.On("card result", (res) =>
            {   
                StartCoroutine(CardResult(res));
            });
            io.On("game result", (res) =>
            {
                StartCoroutine(GameResult(res));
            });
            io.On("error message", (res) =>
            {
                StartCoroutine(ShowError(res));
            });
        });

        #if UNITY_WEBGL == true && UNITY_EDITOR == false
            GameReady("Ready");
        #endif
    }
    
    IEnumerator ShowError(SocketIOEvent socketIOEvent)
    {
        var res = ReceiveJsonObject.CreateFromJSON(socketIOEvent.data);
        switch (res.errMessage)
        {
            case "0":
                ErrorPanel.SetActive(true);
                Warning.text = "Bet Error!!!";
                yield return new WaitForSeconds(6f);
                ErrorPanel.SetActive(false);
                Warning.text = "";
                betPanel.SetActive(true);
                CashoutPanel.SetActive(false);
                break;
            case "1":
                ErrorPanel.SetActive(true);
                Warning.text = "Can't find Server!!!";
                yield return new WaitForSeconds(6f);
                Warning.text = "";
                ErrorPanel.SetActive(false);
                betPanel.SetActive(true);
                CashoutPanel.SetActive(false);
                break;
        }
    }
    void Update()
    {
    }

    public void RequestToken(string data)
    {
        JSONNode usersInfo = JSON.Parse(data);
        Debug.Log("token=--------" + usersInfo["token"]);
        Debug.Log("amount=------------" + usersInfo["amount"]);
        Debug.Log("userName=------------" + usersInfo["userName"]);
        _player.token = usersInfo["token"];
        _player.username = usersInfo["userName"];

        float i_balance = float.Parse(usersInfo["amount"]);
        totalAmountText.text = (i_balance).ToString();
        TotalAmount = float.Parse(totalAmountText.text);
    }

    public void BetAmountField_Changed()
    {
            betAmount.interactable = true;
            float amount = float.Parse(string.IsNullOrEmpty(betAmount.text) ? "0" : betAmount.text);
            float mytotalAmount = float.Parse(string.IsNullOrEmpty(totalAmountText.text) ? "0" : totalAmountText.text);
            amount -= (amount % 10f);
            betAmount.text = (amount).ToString("F2");
            if (mytotalAmount <= 100000f)
            {
                if (amount <= mytotalAmount)
                {

                    if (amount <= 10f)
                    {
                        amount = 10f;
                        betAmount.text = (amount).ToString("F2");
                    }
                }
                else
                {
                    amount = mytotalAmount;
                    betAmount.text = (amount).ToString("F2");
                }
            }
            else if(mytotalAmount <=10f)
            {
                betAmount.text = "10.0";
            }
            else
            {
                betAmount.text = "100000.00";
            }
    }

    public void CrossBtn_Clicked()
    {
        if(!isStarted)
        {   
            float amount = float.Parse(string.IsNullOrEmpty(betAmount.text) ? "0" : betAmount.text);
            float mytotalAmount = float.Parse(string.IsNullOrEmpty(totalAmountText.text) ? "0" : totalAmountText.text);
            if (amount <= mytotalAmount)
            {
                 if (amount >= 10f && amount <= 50000f)
                {
                    amount +=  amount;
                    if(amount <= mytotalAmount)
                    {
                        betAmount.text = (amount).ToString("F2");
                        estimatetext.text = (amount * 1.98f).ToString("F2");
                    }
                    else
                    {
                        betAmount.text = (mytotalAmount).ToString("F2");
                        estimatetext.text = (mytotalAmount * 1.98f).ToString("F2");
                    }

                }
                else if(amount >50000f)
                {
                    betAmount.text = "100000.00";
                    estimatetext.text = (100000f * 1.98f).ToString("F2");
                }
                 else
                {
                    betAmount.text = "10.00";
                    estimatetext.text = (10f * 1.98f).ToString("F2");
                }
            }
            else 
            {
                betAmount.text = (mytotalAmount).ToString("F2");
                estimatetext.text = (amount * 1.98f).ToString("F2");
            }
        }
    }

    public void HalfBtn_Clicked()
    {
        if(!isStarted)
        {
            float amount = float.Parse(string.IsNullOrEmpty(betAmount.text) ? "0" : betAmount.text);
            if (amount <= 10f)
            {
                betAmount.text = "10.00";
                estimatedAmount = amount * 1.98f;
            }
            else if(amount >= 20f)
            {
                betAmount.text = (amount / 2.0f).ToString("F2");
                estimatedAmount = amount * 1.98f;
            }
            else
            {
                betAmount.text = (BetAmount).ToString("F2");
                estimatedAmount = amount * 1.98f;
            }
        }
    }

    public void MaxBtn_Clicked()
    {
        if(!isStarted)
        {
            float myTotalAmount = float.Parse(string.IsNullOrEmpty(totalAmountText.text) ? "0" : totalAmountText.text);
            if (myTotalAmount >= 100000f)
            {
                betAmount.text = "100000.00";
                estimatedAmount = 100000f * 1.98f;
            }
            else
            {
                betAmount.text = (myTotalAmount).ToString("F2");
                estimatedAmount = myTotalAmount * 1.98f;
            }
        }
    }

    public void BetButton_OnClick()
    {
        Jsontype JObject = new Jsontype();
        float myTotalAmount = float.Parse(string.IsNullOrEmpty(totalAmountText.text) ? "0" : totalAmountText.text);
        float betamount = float.Parse(string.IsNullOrEmpty(betAmount.text) ? "0" : betAmount.text);
        
        if (betamount <= myTotalAmount)
        {
            JObject.totalAmount = myTotalAmount;
            JObject.betAmount = betamount;
            JObject.token = _player.token;
            JObject.userName = _player.username;
            io.Emit("bet info", JsonUtility.ToJson(JObject));
            ReturnToInitState();
        }
        else
        {
            ErrorPanel.SetActive(true);
            Warning.text = "Not Enough Funds!!!";
        }
    }

    public void CashButton_OnClick()
    {
        Jsontype JObject = new Jsontype();
        float myTotalAmount = float.Parse(string.IsNullOrEmpty(totalAmountText.text) ? "0" : totalAmountText.text);
        float earnAmount = float.Parse(string.IsNullOrEmpty(winAmountText.text) ? "0" : winAmountText.text);
        JObject.totalAmount = myTotalAmount;
        JObject.earnAmount = earnAmount;
        JObject.token = _player.token;
        JObject.userName = _player.username;
        io.Emit("cash out", JsonUtility.ToJson(JObject));
    }

    public void Frontbutton_OnClick()
    {
        float myTotalAmount = float.Parse(string.IsNullOrEmpty(totalAmountText.text) ? "0" : totalAmountText.text);
        int front = 0;
        Jsontype JObject = new Jsontype();
        JObject.sideFlag = front;
        JObject.totalAmount = myTotalAmount;
        JObject.token = _player.token;
        io.Emit("card click", JsonUtility.ToJson(JObject));
        ButtonUnactivate();

    }
    public void Backbutton_OnClick()
    {
        float myTotalAmount = float.Parse(string.IsNullOrEmpty(totalAmountText.text) ? "0" : totalAmountText.text);
        int back = 1;
        Jsontype JObject = new Jsontype();
        JObject.totalAmount = myTotalAmount;
        JObject.sideFlag =back;
        JObject.token = _player.token;
        io.Emit("card click", JsonUtility.ToJson(JObject));
        ButtonUnactivate();
    }
    IEnumerator BetInfo(SocketIOEvent socketIOEvent)
    {
        var res = ReceiveJsonObject.CreateFromJSON(socketIOEvent.data);
        isStarted = res.isStarted;
        if (isStarted)
        {
            TotalAmount = res.amount;
        estimatedAmount = float.Parse(betAmount.text) * 1.98f;
        StartCoroutine(UpdateCoinsAmount());
        yield return new WaitForSeconds(1f);
        estimatetext.text = (estimatedAmount).ToString("F2");
            betPanel.SetActive(false);
            CashoutPanel.SetActive(true);
            EstimatedPanel.SetActive(true);
            Cashbutton.interactable = false;

        }
    }


    IEnumerator CardResult(SocketIOEvent socketIOEvent)
    {
        var res = ReceiveJsonObject.CreateFromJSON(socketIOEvent.data);
        Debug.Log(res.gameResult + " " + res.odd + " " + res.earnAmount + " " + res.amount);
        ShowResult(res.sideFlag);
        yield return new WaitForSeconds(2f);
        ReturnToInitState();
        if(res.gameResult)
        {
            ButtonActivate();
            oddAmountText.text = (res.odd).ToString("F2");
            estimatedAmount = res.earnAmount * 2;
            estimatetext.text = (estimatedAmount).ToString("F2");
            winAmountText.text = (res.earnAmount).ToString("F2");
            totalAmountText.text = (res.amount).ToString("F2");
            levelNumberText.text = (res.level).ToString();
        }
        else
        {
            ReturnToInitState();
            Cashbutton.interactable = false;
            EstimatedPanel.SetActive(false);
            AlertPanel.SetActive(true);
            Alerttext.text = "Better luck next time!!!!";
            yield return new WaitForSeconds(4f);
            Alerttext.text = "";
            AlertPanel.SetActive(false);
            CashoutPanel.SetActive(false);
            betPanel.SetActive(true);
            Cashbutton.interactable = true;
            ButtonActivate();
            oddAmountText.text = (res.odd).ToString("F2");
            winAmountText.text = (res.earnAmount).ToString("F2");
            totalAmountText.text = (res.amount).ToString("F2");
            levelNumberText.text = (res.level).ToString();
            estimatetext.text = "";
            isStarted = false;
        }

    }
    IEnumerator GameResult(SocketIOEvent socketIOEvent)
    {
        var res = ReceiveJsonObject.CreateFromJSON(socketIOEvent.data);
        ReturnToInitState();
        EstimatedPanel.SetActive(false);
        Betbutton.interactable = false;
        CashoutPanel.SetActive(false);
        betPanel.SetActive(true);
        AlertPanel.SetActive(true);
        Alerttext.text = "You earned!!!";
        yield return new WaitForSeconds(4f);
        Alerttext.text = "";
        AlertPanel.SetActive(false);
        TotalAmount = res.amount;
        StartCoroutine(UpdateCoinsAmount());
        yield return new WaitForSeconds(1f);
        Betbutton.interactable = true;
        ButtonActivate();
        estimatetext.text = "";
        oddAmountText.text = (res.odd).ToString("F2");
        winAmountText.text = (res.earnAmount).ToString("F2");
        levelNumberText.text = (res.level).ToString();
        yield return new WaitForSeconds(1f);
        winAmountText.text = "0.00";
        isStarted = false;
    }

    void ShowResult(int sideFlag)
    {
        if (sideFlag == 0)
        {
            anim.SetBool("frontFlag", true);
            anim.SetBool("backFlag", false);
        }
        else if (sideFlag == 1)
        {
            anim.SetBool("frontFlag", false);
            anim.SetBool("backFlag", true);
        }
    }
    void ButtonActivate()
    {
        frontbutton.interactable = true;
        backbutton.interactable = true;
        Cashbutton.interactable = true;
    }
    void ButtonUnactivate()
    {
        frontbutton.interactable = false;
        backbutton.interactable = false;
        Cashbutton.interactable = false;
    }
    void ReturnToInitState()
    {
        anim.SetBool("frontFlag", false);
        anim.SetBool("backFlag", false);
    }
    private IEnumerator UpdateCoinsAmount()
    {
        // Animation for increasing and decreasing of coins amount
        const float seconds = 0.5f;
        float elapsedTime = 0f;
        while (elapsedTime < seconds)
        {
            totalAmountText.text = Mathf.Floor(Mathf.Lerp(PreviousAmount, TotalAmount, (elapsedTime / seconds))).ToString();
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        PreviousAmount = TotalAmount;
        totalAmountText.text = TotalAmount.ToString("F2");
    }
}
public class BetPlayer
{
    public string username;
    public string token;
}
