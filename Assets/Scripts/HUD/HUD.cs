using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public GameObject Player; //ссылка на игрока
    public string MenuScenename;

    public float CoverShowSpeed = 2f;
    public float GOSShowSpeed = 2f;

    public float GameOverShift = 1f;
    public float StatusShift = 1f;
    
    public int TopItemId = -1;
    public int BottomItemId = -1;

    public int TopItemCount = 0;
    public int BottomItemCount = 0;

    public GameController gameController; //указатель на гейм контроллер
    public Sprite UIMask; //маска (накладывается при отсутствии предмета в инвентаре)

    private GameObject GameOver;

    private GameObject ItemStay = null; //предмет на котором стоит персонаж
    
    private GameObject pickUpButton;    //ссылка на кнопку "поднять предмет"

    private Image topItemImage; //отображение спрайтов предметов
    private Image bottomItemImage;

    private Text topItemText;  //отображение количества верхних предметов
    private Text bottomItemText;   //отображение количества нижних предметов

    private int IPUP;//item pick up pointer

    private bool isGameOver = false;

    private void Start()
    {
        IPUP = 0;
        
        Transform Root = transform.Find("Root");
        pickUpButton = Root.Find("PickUpItem").gameObject;
        topItemImage = Root.Find("TopItem").Find("Icon").GetComponent<Image>();
        topItemText = Root.Find("TopItem").Find("Count").GetComponent<Text>();
        bottomItemImage = Root.Find("BottomItem").Find("Icon").GetComponent<Image>();
        bottomItemText = Root.Find("BottomItem").Find("Count").GetComponent<Text>();
        GameOver = transform.Find("GameOver").gameObject;
    }

    /// <summary>
    /// Lock Player controller, while clicked HUD button
    /// </summary>
    public void SetLockControll(bool isLocking)
    {
        Player.GetComponent<PlayerController>().isPressedButton = isLocking;
    }

    //была нажата какая-то кнопка худа
    public void ClickedButton(ButConst butId)
    {
        if (butId == ButConst.Back)
        {   //попытка выйти в меню
            //пока игра не закончена, при нажатии на эту кнопку после смерти игра перезапускается
            if (isGameOver)
            {   //выход в меню
                SceneManager.LoadScene(MenuScenename);
                return;
            }
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
            gameController.ItemsList[TopItemId].Use(Player.GetComponent<PlayerController>());   //применяем предмет
            if (TopItemCount <= 0)
            {
                //удалить предмет
                TopItemId = -1;
            }
        }
        else if(butId == ButConst.Bottom && BottomItemId != -1)
        {   //кнопка нижнего предмета
            BottomItemCount--; //уменьшаем кол-во в инвентаре
            gameController.ItemsList[BottomItemId].Use(Player.GetComponent<PlayerController>());   //применяем предмет
            if (BottomItemCount <= 0)
            {
                //удалить предмет
                BottomItemId = -1;
            }
        }
        else if(butId == ButConst.Shield)
        {   //попытка поставить блок
            Player.GetComponent<PlayerController>().SetBlock();
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


        //включаем кнопку выйти из игры
        isGameOver = true;
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
        
        DisplayActual();
        ItemStayClear();
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
            drop = Instantiate(gameController.ItemsList[TopItemId].gameObject);
            drop.GetComponent<Item>().count = TopItemCount;
        }
        else
        {   //пора дропнуть второй item
            drop = Instantiate(gameController.ItemsList[BottomItemId].gameObject);
            drop.GetComponent<Item>().count = BottomItemCount;
        }
        drop.transform.position = Player.transform.position;
        //затираем инфу
        UpdateItem(taken.id, taken.count, IPUP);
        IPUP += 1; IPUP %= 2;
    }

    private void DisplayActual()
    {
        if(TopItemId != -1)
        {   //отображение верхнего предмета
            topItemImage.sprite = gameController.ItemsList[TopItemId].ItemSprite;
        }
        else
        {
            topItemImage.sprite = UIMask;
        }
        if (BottomItemId != -1)
        {   //отображение нижнего предмета
            bottomItemImage.sprite = gameController.ItemsList[BottomItemId].ItemSprite;
        }
        else
        {
            bottomItemImage.sprite = UIMask;
        }
        topItemText.text = TopItemCount > 1 ? TopItemCount + "" : "";
        bottomItemText.text = BottomItemCount > 1 ? BottomItemCount + "" : "";
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
}
