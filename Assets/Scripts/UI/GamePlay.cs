using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GamePlay : MonoBehaviour
{
    public Text help;
    public Transform helpScreen;
    public Boolean state = false;
    public Button playerOne, playerTwo, instruction;
    public int one, two, numplayer;
    public Image pOne, pTwo;
    public Animator aOne, aTwo;
    public float timer = 20f;
    public Text time, p1score, p2score, final, wrong1, wrong2;
    public int numQ = 0;
    public Image circuit;
    public Transform gameOver, goBack;
    public Boolean pause;
    public InputField p1ans, p2ans;
    public double answer;
    public LoadRandomCircuit load;

    public double p1answer, p2answer;
    public double p1s = 0.0, p2s = 0.0;
    public Boolean correct1 = false, correct2 = false;

    void Awake()
    {
        one = SelectPlayer.ONE;
        two = SelectPlayer.TWO;
        numplayer = SelectPlayer.PNUM;

        if (numplayer == 1)
        {
            playerTwo.gameObject.SetActive(false);
            p2ans.gameObject.SetActive(false);
            switch (one)
            {
                case 1:
                    playerOne.image.sprite = Resources.Load<Sprite>("chara/CAP");
                    //playerOne.gameObject.GetComponents<Animator> = (Resources.Load<Animator>("cahra.cao"));
                    break;
                case 2:
                    playerOne.image.sprite = Resources.Load<Sprite>("chara/LED");
                    break;
                case 3:
                    playerOne.image.sprite = Resources.Load<Sprite>("chara/RES");
                    break;
                case 4:
                    playerOne.image.sprite = Resources.Load<Sprite>("chara/TRANS");
                    break;
            }
        }
        else
        {
            playerTwo.gameObject.SetActive(true);
            p2ans.gameObject.SetActive(true);
            switch (one)
            {
                case 1:
                    playerOne.image.sprite = Resources.Load<Sprite>("chara/CAP");
                    //playerOne.gameObject.GetComponent<Animator>() = Resources.Load<Animator>("cahra.cao");
                    break;
                case 2:
                    playerOne.image.sprite = Resources.Load<Sprite>("chara/LED");
                    break;
                case 3:
                    playerOne.image.sprite = Resources.Load<Sprite>("chara/RES");
                    break;
                case 4:
                    playerOne.image.sprite = Resources.Load<Sprite>("chara/TRANS");
                    break;
            }

            switch (two)
            {
                case 1:
                    playerTwo.image.sprite = Resources.Load<Sprite>("chara/CAP");
                    //playerOne.gameObject.GetComponent<Animator>() = Resources.Load<Animator>("cahra.cao");
                    break;
                case 2:
                    playerTwo.image.sprite = Resources.Load<Sprite>("chara/LED");
                    break;
                case 3:
                    playerTwo.image.sprite = Resources.Load<Sprite>("chara/RES");
                    break;
                case 4:
                    playerTwo.image.sprite = Resources.Load<Sprite>("chara/TRANS");
                    break;
            }

        }
    }

    void Start()
    {
        load = FindObjectOfType<LoadRandomCircuit>();
        gameOver.gameObject.SetActive(false);
        help.text = " < Objective >: Calculate and Enter the correct equvialent resistance of the circuit. \n < Score System >: Player to answer correct will < +score >. \n IF, both players answer correct within the give time, first person to answer will gain extra points. \n \n<EXIT> : | esc | ";
        StartCoroutine(Instruction(instruction.gameObject));
    }

    private void Update()
    {
        answer = LoadRandomCircuit.ANS;
        helpScreen.gameObject.SetActive(state);
        check1answer();
        check2answer();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            goBack.gameObject.SetActive(true);
            CancelInvoke();
        }

        if (timer == 0)
        {
            numQ++;

            if (numQ == 10)
            {
                timer = 0;
                gameOver.gameObject.SetActive(true);
                CancelInvoke();

                if (numplayer == 1)
                {
                    final.text = "SCORE: " + p1s + "";
                }

                else
                {
                    if (p1s > p2s)
                    {
                        final.text = "PLAYER 1 WINS";
                    }
                    else if (p2s > p1s)
                    {
                        final.text = "PLAYER 2 WINS";
                    }
                    else
                    {
                        final.text = "DRAW";
                    }
                }
            }

            else
            {
                load.NextCircuit();
                timer = 120;
            }
        }

        if (numQ == 10)
        {
            timer = 0;
            gameOver.gameObject.SetActive(true);
            CancelInvoke();

            if (numplayer == 1)
            {
                final.text = "SCORE: " + p1s;
            }

            else
            {
                if (p1s > p2s)
                {
                    final.text = "PLAYER 1 WINS";
                }
                else if (p2s > p1s)
                {
                    final.text = "PLAYER 2 WINS";
                }
                else
                {
                    final.text = "DRAW";
                }
            }
        }

        time.text = timer + "";

    }

    void decreaseTimeRemaining()
    {
        timer--;
    }

    public void HELP()
    {
        state = !state;
    }

    public void buttonYes()
    {
        SceneManager.LoadScene("GameMenu");
    }

    public void buttonNo()
    {
        goBack.gameObject.SetActive(false);
        InvokeRepeating("decreaseTimeRemaining", 1, 1);
    }

    void check1answer()
    {
        if (p1ans.isFocused && p1ans.text != "" && Input.GetKey(KeyCode.Return))
        {
            wrong1.gameObject.SetActive(false);
            double.TryParse(p1ans.text, out p1answer);
            if (p1answer == answer)
            {
                correct1 = true;
                p1s = p1s + timer;
                p1score.text = p1s + "";


                if (numplayer == 1)
                {
                    numQ++;
                    load.NextCircuit();
                    timer = 120;
                }

                else
                {
                    if (correct2 == true)
                    {
                        numQ++;
                        p2ans.text = "";
                        p2ans.interactable = true;
                        correct1 = false;
                        correct2 = false;
                        load.NextCircuit();
                        timer = 120;
                    }
                    else
                    {
                        p1ans.interactable = false;
                    }
                }
            }
            else
            {
                StartCoroutine(ShowFeedback(wrong1.gameObject));
            }

            p1ans.text = "";
        }
    }

    void check2answer()
    {
        if (p2ans.isFocused && p2ans.text != "" && Input.GetKey(KeyCode.Return))
        {
            wrong2.gameObject.SetActive(false);
            double.TryParse(p2ans.text, out p2answer);
            if (p2answer == answer)
            {
                correct2 = true;
                p2s = p2s + timer;
                p2score.text = p2s + "";

                if (correct1 == true)
                {
                    numQ++;
                    p1ans.text = "";
                    p1ans.interactable = true;
                    correct1 = false;
                    correct2 = false;
                    load.NextCircuit();
                    timer = 120;
                }

                else
                {
                    p2ans.interactable = false;
                }
            }
            else
            {
                StartCoroutine(ShowFeedback(wrong2.gameObject));
            }

            p2ans.text = "";
        }

    }

    IEnumerator ShowFeedback(GameObject feedback)
    {
        feedback.SetActive(true);
        yield return new WaitForSeconds(2.0f);
        feedback.SetActive(false);
    }

    IEnumerator Instruction (GameObject inst)
    {
        inst.SetActive(true);
        yield return new WaitForSeconds(3.0f);
        inst.SetActive(false);
        InvokeRepeating("decreaseTimeRemaining", 1, 1);
    }
}
