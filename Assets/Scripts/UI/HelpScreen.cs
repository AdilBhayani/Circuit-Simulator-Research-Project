using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HelpScreen : MonoBehaviour
{
    public Text instructions, pagename;
    public int page = 0;
    public Button backbutton, nextbutton;
    public Image image;
    public Sprite[] images = new Sprite[15];

    void Start()
    {
        page = 0;
        images[0] = Resources.Load<Sprite>("GameSprite/create_one");
        images[1] = Resources.Load<Sprite>("GameSprite/create_two");
        images[2] = Resources.Load<Sprite>("GameSprite/create_three");
        images[3] = Resources.Load<Sprite>("GameSprite/create_four");
        images[4] = Resources.Load<Sprite>("GameSprite/create_five");
        images[5] = Resources.Load<Sprite>("GameSprite/create_six");
        images[6] = Resources.Load<Sprite>("GameSprite/create_seven");
        images[7] = Resources.Load<Sprite>("GameSprite/create_eight");
        images[8] = Resources.Load<Sprite>("GameSprite/create_nine");
        images[9] = Resources.Load<Sprite>("GameSprite/create_ten");
        images[10] = Resources.Load<Sprite>("GameSprite/eq_one");
        images[11] = Resources.Load<Sprite>("GameSprite/eq_two");
        images[12] = Resources.Load<Sprite>("GameSprite/eq_three");
        images[13] = Resources.Load<Sprite>("GameSprite/eq_four");
        images[14] = Resources.Load<Sprite>("GameSprite/new_playscreen");
    }

    public void Help_Back(string name)
    {
        SceneManager.LoadScene("Startscreen");
    }

    public void back()
    {
        page--;

        if (page == 0 || page == 10 || page == 14)
        {
            backbutton.gameObject.SetActive(false);
            image.sprite = images[0];

        }

        if (page < 15)
        {
            nextbutton.gameObject.SetActive(true);
        }
    }

    public void next()
    {
        page++;

        if (page > 0)
        {
            backbutton.gameObject.SetActive(true);
        }

        if (page == 14 || page == 9 || page == 13)
        {
            nextbutton.gameObject.SetActive(false);
        }
    }
    private void Update()
    {
        switch (page)
        {
            case 0:
                image.sprite = images[0];
                instructions.text = "\nThis is the screen you will see when you enter  \n'CIRCUIT CREATE'" +"\n";
                break;
            case 1:
                image.sprite = images[1];
                instructions.text = "\n< CLICK > then drag a 'RESISTOR' component and a ghost will appear. \nNow place the resistor anywhere on the field \n";
                break;
            case 2:
                image.sprite = images[2];
                instructions.text = "\nNow, < CLICK > then drag a 'node' component, a node ghost will appear. \nPlace the node between each resistor \n";
                break;
            case 3:
                image.sprite = images[3];
                instructions.text = "\nBe creative. Sketch your desired circuit \n";
                break;
            case 4:
                image.sprite = images[4];
                instructions.text = "\nWhen you are done, \n Hover over the 'node' component and a small circle will appear. \n< CLICK > on the little circle, it will turn orange \n";
                break;
            case 5:
                image.sprite = images[5];
                instructions.text = "\nNow, hover over a 'resistor' component. \n< CLICK > on the little circle to connect the two components together \n ";
                break;
            case 6:
                image.sprite = images[6];
                instructions.text = "\nConnect all components in the same way \n";
                break;
            case 7:
                image.sprite = images[7];
                instructions.text = "\nWhen you are finished, press the < SAVE > button. \n";
                break;
            case 8:
                image.sprite = images[8];
                instructions.text = "\nName your Circuit. An error will appear if your name isn't unique. \n";
                break;
            case 9:
                image.sprite = images[9];
                instructions.text = "\n'CIRCUIT SAVED' will appear at the top centre of your screen, to indicate your circuit has been successfully saved \n";
                break;
            case 10:
                image.sprite = images[10];
                instructions.text = "\nA random circuit will be loaded, whenever you enter ' EQUVIALENT RESISTANCE ' \n";
                break;
            case 11:
                image.sprite = images[11];
                instructions.text = "\nTo start, \n< CLICK > on two resistors. \nNow, < CLICK > on a Transformation \n";
                break;
            case 12:
                image.sprite = images[12];
                instructions.text = "\nIf successful, the circuit will redraw to match the chosen transformation\n Continue to Transform all resistors until only ONE is left.\n";
                break;
            case 13:
                image.sprite = images[13];
                instructions.text = "\nSUCCESS!  \nYou have completed the round. \nPress < NEXT > to move on or \nPress < RESET > to try again. \n";
                break;
            case 14:
                image.sprite = images[14];
                instructions.text = "< Objective >: Calculate and Enter the correct equvialent resistance of the circuit. \n<Score System >: Player to answer correctly will < + score >. \nIF, both players answer correctly within the give time, first person to answer will gain extra points. \n \n< EXIT > : | esc |";
                break;
            default:
                break;
        }
    }

        public void press()
      {
     
        pagename.text = "CIRCUIT CREATE";
        page = 0;
        backbutton.gameObject.SetActive(false);
        nextbutton.gameObject.SetActive(true);
    }

    public void press2 ()
    {
        pagename.text = "EQUVIALENT RESISTANCE";
        page = 10;
        backbutton.gameObject.SetActive(false);
        nextbutton.gameObject.SetActive(true);
    }


    public void press3 ()
     {
        page = 14;
        pagename.text = "MINI GAME";
        backbutton.gameObject.SetActive(false);
        nextbutton.gameObject.SetActive(false);
        image.sprite = images[14];
        instructions.text = "< Objective >: Calculate and Enter the correct equvialent resistance of the circuit. \n <Score System >: Player to answer correctly will < + score >. \n IF, both players answer correctly within the give time, first person to answer will gain extra points. \n \n < EXIT > : | esc |";
    }
   
}


