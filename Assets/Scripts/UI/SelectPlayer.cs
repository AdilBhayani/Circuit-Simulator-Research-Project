using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelectPlayer : MonoBehaviour
{
    public bool state1 = true, state2 = true;
    public static int playernum = 1, num = 0;
    public Transform player2, t_cap, t_led, t_res, t_trans;
    public Button one_cap, one_led, one_res, one_trans, two_cap, two_led, two_res, two_trans, playbutton;
    public Button player1chosen, player2chosen;
    public Button turnoff1, turnoff2, selectPlayer;

    public static int one = 0, two = 0;

    void Start()
    {
        playernum = 1;
        num = 0;
        StartCoroutine(ShowFeedback(selectPlayer.gameObject));
    }
    public void player1buttons(bool one_cap, bool one_led, bool one_res, bool one_trans)
    {
        if (state1 == false)
        {
            this.one_cap.interactable = one_cap;
            this.one_led.interactable = one_led;
            this.one_res.interactable = one_res;
            this.one_trans.interactable = one_trans;
        }
        else
        {
            this.one_cap.interactable = true;
            this.one_led.interactable = true;
            this.one_res.interactable = true;
            this.one_trans.interactable = true;
        }
    }

    public void player2buttons(bool two_cap, bool two_led, bool two_res, bool two_trans)
    {
        if (state2 == false)
        {
            this.two_cap.interactable = two_cap;
            this.two_led.interactable = two_led;
            this.two_res.interactable = two_res;
            this.two_trans.interactable = two_trans;
        }
        else
        {
            this.two_cap.interactable = true;
            this.two_led.interactable = true;
            this.two_res.interactable = true;
            this.two_trans.interactable = true;
        }
    }

    public void readiness()
    {
        if (turnoff1 != null)
        {
            turnoff1.interactable = state1;
        }

        if (turnoff2 != null)
        {
            turnoff2.interactable = state2;
        }

        if ((state1 == true) && (state2 == false) && (turnoff1 != null))
        {
            turnoff1.interactable = false;
        }

        if ((state1 == false) && (state2 == true) && (turnoff2 != null))
        {
            turnoff2.interactable = false;
        }

        if ((state1 == false) && (state2 == false))
        {
            player1chosen.interactable = true;
            player2chosen.interactable = true;
        }
    }


    public void checkReady(bool state1, bool state2)
    {
        if ((state1 == false) && (playernum == 1))
        {
            playbutton.gameObject.SetActive(true);
        }

        else if ((state2 == false) && (state1 == false) && (playernum == 2))
        {
            playbutton.gameObject.SetActive(true);
        }

        else
        {
            playbutton.gameObject.SetActive(false);
        }
    }

    public void back()
    {
        SceneManager.LoadScene("GameMenu");
    }

    public void play(string name)
    {
        SceneManager.LoadScene("PlayScreen");
    }

    public void PlusPlayer2()
    {
        playernum = 2;
        player2.gameObject.SetActive(false);
        checkReady(state1, state2);
        t_cap.gameObject.SetActive(true);
        t_led.gameObject.SetActive(true);
        t_res.gameObject.SetActive(true);
        t_trans.gameObject.SetActive(true);
    }


    //PLAYER ONE
    public void OneCap()
    {
        one = 1;
        state1 = !state1;
        player1chosen = one_cap;
        turnoff1 = two_cap;
        player1buttons(!state1, state1, state1, state1);
        readiness();
        checkReady(state1, state2);

    }

    public void OneLed()
    {
        one = 2;
        state1 = !state1;
        player1chosen = one_led;
        turnoff1 = two_led;
        player1buttons(state1, !state1, state1, state1);
        readiness();
        checkReady(state1, state2);

    }

    public void OneRes()
    {
        one = 3;
        state1 = !state1;
        player1chosen = one_res;
        turnoff1 = two_res;
        player1buttons(state1, state1, !state1, state1);
        readiness();
        checkReady(state1, state2);
    }

    public void OneTrans()
    {
        one = 4;
        state1 = !state1;
        player1chosen = one_trans;
        turnoff1 = two_trans;
        player1buttons(state1, state1, state1, !state1);
        readiness();
        checkReady(state1, state2);
    }


    //PLAYER TWO
    public void TwoCap()
    {
        two = 1;
        state2 = !state2;
        player2chosen = two_cap;
        turnoff2 = one_cap;
        player2buttons(!state2, state2, state2, state2);
        readiness();
        checkReady(state1, state2);
    }

    public void TwoLed()
    {
        two = 2;
        state2 = !state2;
        player2chosen = two_led;
        turnoff2 = one_led;
        player2buttons(state2, !state2, state2, state2);
        readiness();
        checkReady(state1, state2);
    }

    public void TwoRes()
    {
        two = 3;
        state2 = !state2;
        player2chosen = two_res;
        turnoff2 = one_res;
        player2buttons(state2, state2, !state2, state2);
        readiness();
        checkReady(state1, state2);

    }

    public void TwoTrans()
    {
        two = 4;
        state2 = !state2;
        player2chosen = two_trans;
        turnoff2 = one_trans;
        player2buttons(state2, state2, state2, !state2);
        readiness();
        checkReady(state1, state2);
    }

    public static int ONE
    {
        get { return one; }
        set { one = value; }
    }


    public static int TWO
    {
        get { return two; }
        set { two = value; }
    }

    public static int PNUM
    {
        get { return playernum; }
        set { playernum = value; }
    }

    IEnumerator ShowFeedback(GameObject feedback)
    {
        feedback.SetActive(true);
        yield return new WaitForSeconds(2.0f);
        feedback.SetActive(false);
    }
}