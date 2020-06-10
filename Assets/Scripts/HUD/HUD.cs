using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public enum GameStatus
    {
        OnGoing = 0,
        Pause = 1,
        Items = 2,
        GameOver = 3
    }
    [Header("Настройки спрайтов кнопок")]
    [Tooltip("Спрайт щита во время игры")]
    public Sprite Shield;
    [Tooltip("Спрайт кнопки назад во время игры")]
    public Sprite Menu;
    [Tooltip("Спрайт кнопки назад в меню")]
    public Sprite Back;
    [Tooltip("Спрайт кнопки вверх на пузе")]
    public Sprite UpBut;
    [Tooltip("Спрайт кнопки вниз на паузе")]
    public Sprite DownBut;
    [Tooltip("Спрайт кнопки выбрать на паузе")]
    public Sprite Select;
    [Tooltip("Спрайт 'покинуть игру'")]
    public Sprite Exit;


    [Header("Настройки HUD")]
    [Tooltip("Ссылка на игрока")]
    public GameObject Player;
    [Tooltip("Название сцены главного меню")]
    public string MenuScenename;

    [HideInInspector]
    public int TopItemId = -1;
    [HideInInspector]
    public int BottomItemId = -1;
    [HideInInspector]
    public int TopItemCount = 0;
    [HideInInspector]
    public int BottomItemCount = 0;

    [Tooltip("Указатель на гейм контроллер")]
    public GameController GameController;
    [Tooltip("Маска (накладывается при отсутствии предмета в инвентаре)")]
    public Sprite UIMask;

    private GameObject GameOver;

    private GameObject ItemStay = null; //предмет на котором стоит персонаж
    
    private GameObject pickUpButton;    //ссылка на кнопку "поднять предмет"

    private GameObject pause; //ссылка на окно паузы

    private Image topItemImage; //отображение спрайтов предметов
    private Image bottomItemImage;
    private Image shieldBut;
    private Image backBut;

    private Text topItemText;  //отображение количества верхних предметов
    private Text bottomItemText;   //отображение количества нижних предметов

    private List<Game_LongButton> buttons = new List<Game_LongButton>();
    private int selectBut = 0;
    private PickedUpItems items;
    private PickedUpInfo puInfo;

    private int IPUP;//item pick up pointer

    private GameStatus gs = GameStatus.OnGoing;

    private void Start()
    {
        IPUP = 0;
        
        Transform Root = transform.Find("Root");
        pickUpButton = Root.Find("PickUpItem").gameObject;
        topItemImage = Root.Find("TopItem").Find("Icon").GetComponent<Image>();
        topItemText = Root.Find("TopItem").Find("Count").GetComponent<Text>();
        bottomItemImage = Root.Find("BottomItem").Find("Icon").GetComponent<Image>();
        bottomItemText = Root.Find("BottomItem").Find("Count").GetComponent<Text>();
        shieldBut = Root.Find("Shield").Find("Icon").GetComponent<Image>();
        backBut = Root.Find("Pause").Find("Icon").GetComponent<Image>();


        GameOver = transform.Find("GameOver").gameObject;
        pause = transform.Find("Pause").gameObject;
        buttons.AddRange(transform.Find("Pause").GetComponentsInChildren<Game_LongButton>());
        items = transform.Find("Items").GetComponent<PickedUpItems>();
        puInfo = Root.Find("PickedUpInfo").GetComponent<PickedUpInfo>();
        DisplayActual();
    }

    /// <summary>
    /// Lock Player controller, while clicked HUD button
    /// </summary>
    public void SetLockControll(bool isLocking)
    {
        Player.GetComponent<PlayerController>().isPressedButton = isLocking;
    }


    //была нажата какая-то кнопка
    public void ClickedButton(ButConst butId)
    {
        switch(gs)
        {
            case GameStatus.OnGoing:
                if (butId == ButConst.Back)
                {
                    //игра поставлена на паузу
                    Time.timeScale = 0;
                    gs = GameStatus.Pause;
                    //блокировка тача
                    Player.GetComponent<PlayerController>().isPressedButton = true;
                    pause.SetActive(true);
                }

                if (!Player.GetComponent<PlayerController>().GetCapacity())
                {   //игрок не может выполнить это действие сейчас
                    return;
                }

                //Какие-то действия
                if (butId == ButConst.Up && TopItemId != -1)
                {
                    //кнопка верхнего предмета
                    TopItemCount--; //уменьшаем кол-во в инвентаре
                    GameController.ItemsList[TopItemId].Use(Player.GetComponent<PlayerController>());   //применяем предмет
                    if (TopItemCount <= 0)
                    {
                        //удалить предмет
                        TopItemId = -1;
                    }
                }
                else if (butId == ButConst.Bottom && BottomItemId != -1)
                {   //кнопка нижнего предмета
                    BottomItemCount--; //уменьшаем кол-во в инвентаре
                    GameController.ItemsList[BottomItemId].Use(Player.GetComponent<PlayerController>());   //применяем предмет
                    if (BottomItemCount <= 0)
                    {
                        //удалить предмет
                        BottomItemId = -1;
                    }
                }
                else if (butId == ButConst.Shield)
                {   //попытка поставить блок
                    Player.GetComponent<PlayerController>().SetBlock();
                }
                
                break;
            case GameStatus.Pause:
                if(butId == ButConst.Back)
                {
                    //игра снята с паузы
                    Time.timeScale = 1;
                    gs = GameStatus.OnGoing;
                    //разблокировка тача
                    pause.SetActive(false);
                    Player.GetComponent<PlayerController>().isPressedButton = false;
                }
                else if(butId == ButConst.Menu)
                {
                    Time.timeScale = 1;
                    gs = GameStatus.OnGoing;
                    //выход в меню
                    SceneManager.LoadScene(MenuScenename);
                }
                else if(butId == ButConst.Items)
                {
                    gs = GameStatus.Items;
                    selectBut = 0;
                    pause.SetActive(false);
                    items.gameObject.SetActive(true);

                }
                else if(butId == ButConst.Up)
                {
                    buttons[selectBut].UnPressed();
                    selectBut = (++selectBut)%buttons.Count;
                    buttons[selectBut].Press();
                }
                else if(butId == ButConst.Bottom)
                {
                    buttons[selectBut].UnPressed();
                    selectBut = selectBut > 0 ? selectBut - 1 : buttons.Count - 1;
                    buttons[selectBut].Press();
                }
                else if(butId == ButConst.Shield)
                {
                    buttons[selectBut].UnPress();
                    selectBut = 0;
                }
                break;
            case GameStatus.Items:
                if(butId == ButConst.Up)
                {
                    items.Up();
                }
                else if(butId == ButConst.Bottom)
                {
                    items.Down();
                }
                else if(butId == ButConst.Back)
                {
                    gs = GameStatus.Pause;
                    pause.SetActive(true);
                    items.gameObject.SetActive(false);
                }
                break;
            case GameStatus.GameOver:
                //выход в меню
                SceneManager.LoadScene(MenuScenename);
                return;
        }

        DisplayActual();
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
        SceneManager.LoadScene(MenuScenename);
    }

    //открывает меню статистики
    public void Statistics()
    {

    }

    //конец игры
    public void DisplayGameOver(float score)
    {   //плавно показать cover
        //текст GameOver должен быстро появится и как бы упасть сверхку
        //тест status должен быстро появится и как бы упасть сверху
        //StartCoroutine(_DisplayGameOver(score));


        //ставим статус "игра окончена"
        gs = GameStatus.GameOver;
        //отключаем значек взаимодействия
        pickUpButton.SetActive(false);
        
        //заполняем score
        Text scoreText = GameOver.transform.Find("Score").GetComponent<Text>();
        scoreText.text = score + "";
        //активируем GameOver - родитель всех компонентов
        GameOver.SetActive(true);
        GameOver.GetComponent<Animator>().Play("GameOver");
    }

    //поднять предмет, который лежит на земле
    public void PickUp()
    {
        if (!Player.GetComponent<PlayerController>().GetCapacity())
        {   //игрок не может выполнить это действие сейчас
            return;
        }
        
        bool isNeedDelete = true;
        if (ItemStay != null)
        {
            Item item = ItemStay.GetComponent<Item>();
            if(item != null)
            {
                puInfo.DisplayAbout(item);
                if(item.isActivate)
                {
                    if (TopItemCount == 0)
                    {   //врхний слот свободен, просто занесем
                        UpdateItem(item.id, item.count, 0);
                    }
                    else if (item.id == TopItemId && (TopItemCount < item.maxStack))
                    {   //такой предмет уже есть в инвентаре и что-то можно положить
                        isNeedDelete = !FillSlot(item, 0);
                    }
                    else if (BottomItemCount == 0)
                    {   //нижний слот свободен, просто занесем
                        UpdateItem(item.id, item.count, 1);
                    }
                    else if (item.id == BottomItemId && (BottomItemCount < item.maxStack))
                    {   //такой предмет уже есть в инвентаре и что-то можно положить
                        isNeedDelete = !FillSlot(item, 1);
                    }
                    else
                    {
                        //в остальных случаях нужно что-то выбросить
                        DropItem(item);
                    }
                    item.PickUp(isNeedDelete);
                }
                else
                {
                    item.JustUse(Player.GetComponent<PlayerController>());
                    items.PickedUp(GameController.ItemsList[item.id]);
                    item.PickUp(isNeedDelete);
                }
            }
            else
            {   //это не предмет
                //скорее всего это окружение. Нужно использовать
                Environment environment = ItemStay.GetComponent<Environment>();
                //собрав данные, используем
                environment.Use(Player.GetComponent<PlayerController>());
            }
            
        }
        
        DisplayActual();
        ItemStayClear();
    }

    /// <summary>
    /// Заполняет данные о предмете
    /// </summary>
    /// <param name="id">уи предмета</param>
    /// <param name="count">количество предметов</param>
    /// <param name="slot">заполняемые ячейка</param>
    private void UpdateItem(int id, int count, int slot)
    {
        if (slot == 0)
        {
            TopItemId = id;
            TopItemCount = count;
        }
        else if(slot == 1)
        {
            BottomItemId = id;
            BottomItemCount = count;
        }
    }

    //извлекает предметы из кучи и складывает их в указанную ячейку,
    //а когда она кончится, заполняет следующую
    //возвращает true, если в item остались предметы
    private bool FillSlot(Item item, int slot)
    {
        if(slot == 0)
        {   //работа с верхним слотом
            if(TopItemCount < item.maxStack)
            {
                TopItemCount = Item.Sum(TopItemCount, item);
            }
            if(item.count > 0)
            {
                if(BottomItemCount < item.maxStack && BottomItemId == item.id)
                {   //во втором слоте лежит такой же предмет, ЗАПОЛНИМ!
                    BottomItemCount = Item.Sum(BottomItemCount, item);
                }
                else if(BottomItemId == -1)
                {   //второй слот свободен. ЗАПОЛНИМ!
                    UpdateItem(item.id, item.count, 1);
                }
            }
            
        }
        else if(slot == 1)
        {
            if (BottomItemCount < item.maxStack)
            {
                BottomItemCount = Item.Sum(BottomItemCount, item);
            }
            if (item.count > 0)
            {
                if (item.count > 0 && (TopItemCount < item.maxStack) && (TopItemId == item.id))
                {   //в первом слоте лежит такой же предмет, ЗАПОЛНИМ!
                    TopItemCount = Item.Sum(TopItemCount, item);
                }
                else if (TopItemId == -1)
                {   //первый слот свободен. ЗАПОЛНИМ!
                    UpdateItem(item.id, item.count, 1);
                }
            }
        }
        return item.count > 0;
    }

    //бросить предмет на землю
    public void DropItem(Item taken)
    {   //taken - предмет, который окажется вместо выброшенного
        GameObject drop;

        if (IPUP == 0)
        {   //время дропнуть первый item
            drop = Instantiate(GameController.ItemsList[TopItemId].gameObject);
            drop.GetComponent<Item>().count = TopItemCount;
        }
        else
        {   //пора дропнуть второй item
            drop = Instantiate(GameController.ItemsList[BottomItemId].gameObject);
            drop.GetComponent<Item>().count = BottomItemCount;
        }
        drop.transform.position = Player.transform.position;
        //затираем инфу
        UpdateItem(taken.id, taken.count, IPUP);
        IPUP += 1; IPUP %= 2;
    }

    private void DisplayActual()
    {
        switch(gs)
        {
            case GameStatus.OnGoing:
                if (TopItemId != -1)
                {   //отображение верхнего предмета
                    topItemImage.sprite = GameController.ItemsList[TopItemId].ItemSprite;
                }
                else
                {
                    topItemImage.sprite = UIMask;
                }
                if (BottomItemId != -1)
                {   //отображение нижнего предмета
                    bottomItemImage.sprite = GameController.ItemsList[BottomItemId].ItemSprite;
                }
                else
                {
                    bottomItemImage.sprite = UIMask;
                }
                topItemText.text = TopItemCount > 1 ? TopItemCount + "" : "";
                bottomItemText.text = BottomItemCount > 1 ? BottomItemCount + "" : "";
                //отображение спрайтов щита и меню
                backBut.sprite = Menu;
                shieldBut.sprite = Shield;
                break;
            case GameStatus.Pause:
                //Отображение кнопок вверх, вниз, назад, ок вместо предметов и щита
                topItemImage.sprite = UpBut;
                bottomItemImage.sprite = DownBut;
                backBut.sprite = Back;
                shieldBut.sprite = Select;
                break;
            case GameStatus.Items:
                //Отображение кнопок вверх, вниз, назад, ок вместо предметов и щита
                topItemImage.sprite = UpBut;
                bottomItemImage.sprite = DownBut;
                backBut.sprite = Back;
                shieldBut.sprite = UIMask;
                break;
            case GameStatus.GameOver:
                //отображение кнопоки выйти в меню и др. вместо худа игры
                topItemImage.sprite = UIMask;
                bottomItemImage.sprite = UIMask;
                backBut.sprite = Exit;
                shieldBut.sprite = UIMask;
                break;
        }
        
    }

    //на змеле ничего не лежит
    public void ItemStayClear()
    {   //нужно убрать кнопку "поднять предмет"
        ItemStay = null;
        pickUpButton.SetActive(false);
    }

    //на земле что-то лежит
    public void ItemStaySet(GameObject Item)
    {   //есть предмет, который можно поднять
        pickUpButton.SetActive(true);
        //отобразим поверх спрайта спрайт предмета
        pickUpButton.transform.Find("Feature").GetComponent<Image>().sprite = 
            Item.transform.Find("Sprite").GetComponent<SpriteRenderer>().sprite;
        ItemStay = Item;
    }

    public void Use(GameObject feature)
    {
        pickUpButton.transform.Find("Feature").GetComponent<Image>().sprite =
            feature.transform.Find("Sprite").GetComponent<SpriteRenderer>().sprite;
    }

    
}
