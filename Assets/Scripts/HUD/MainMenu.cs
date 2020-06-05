using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Найстройка меню")]
    [Tooltip("Основная сцена игры")]
    public string LoadSceneName;
    [Tooltip("Поле, в котором дублируется информация о нажатой кнопке")]
    public Text SecondString;
    [Tooltip("Выбранная кнопка (изменение стартовой кнопки)")]
    public ButConst TurnedBut;

    private MenuButton newGameBut;
    private MenuButton continueBut;
    private MenuButton statisticsBut;

    private void Start()
    {
        Transform Root = transform.Find("Root");
        newGameBut = transform.Find("MainMenu").Find("NewGame").GetComponent<MenuButton>();
        continueBut = transform.Find("MainMenu").Find("ContinueGame").GetComponent<MenuButton>();
        statisticsBut = transform.Find("MainMenu").Find("Statistics").GetComponent<MenuButton>();
        newGameBut.transform.SetAsLastSibling();
        continueBut.transform.SetAsLastSibling();
        statisticsBut.transform.SetAsLastSibling();
        GetButton(TurnedBut).Pressed();
        UpdateSecondString();
    }

    public MenuButton GetButton(ButConst id)
    {
        switch (id)
        {
            case ButConst.NewGame:
                return newGameBut;
            case ButConst.Continue:
                return continueBut;
            case ButConst.Statistics:
                return statisticsBut;
            default:
                return null;
        }

    }

    //была нажата какая-то кнопка худа
    public void ClickedButton(ButConst butId)
    {
        if (butId == ButConst.Up)
        {   //нажата кнопка "вверх"
            GetButton(TurnedBut).UnPressed();
            if (TurnedBut == ButConst.NewGame)
            {
                TurnedBut = ButConst.Statistics;
            }
            else
            {
                TurnedBut--;
            }
            GetButton(TurnedBut).Pressed();
        }
        else if (butId == ButConst.Bottom)
        {   //нажата кнопка "вниз"
            GetButton(TurnedBut).UnPressed();
            if (TurnedBut == ButConst.Statistics)
            {
                TurnedBut = ButConst.NewGame;
            }
            else
            {
                TurnedBut++;
            }
            GetButton(TurnedBut).Pressed();

        }
        else if (butId == ButConst.Shield)
        {   //нажата кнопка "выбрать"
            GetButton(TurnedBut).Action();
        }
        else if (butId == ButConst.Back)
        {   //нажата кнопка "выйти"
            Application.Quit();
        }
        else if (butId == ButConst.NewGame)
        {   //нажата кнопка "начать игру"
            StartNewGame();
        }
        else if (butId == ButConst.Continue)
        {   //нажата кнопка "продолжить игру"

        }
        else if (butId == ButConst.Statistics)
        {   //нажата кнопка "статистика"

        }
        UpdateSecondString();
    }

    private void UpdateSecondString()
    {
        Text but = GetButton(TurnedBut).GetComponentInChildren<Text>();
        SecondString.text = but.text;
    }

    //запустить сохраненную игру
    public void ContinueGame()
    {

    }

    //запускает новую игру
    public void StartNewGame()
    {
        StartGame(true);
    }

    //Запускает игру
    private void StartGame(bool isNewGame = false)
    {
        //пока просто загружаем сцену
        SceneManager.LoadScene(LoadSceneName);
    }

    //открывает меню статистики
    public void Statistics()
    {

    }
}
