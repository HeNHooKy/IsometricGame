using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{

    public int TopItemId = -1;
    public int BottomItemId = -1;

    public int TopItemCount = 0;
    public int BottomItemCount = 0;

    public GameController gameController; //указатель на гейм контроллер
    public Sprite UIMask; //маска (накладывается при отсутствии предмета в инвентаре)

    private GameObject ItemStay = null; //предмет на котором стоит персонаж
    private GameObject player; //ссылка на игрока
    private GameObject pickUpButton;    //ссылка на кнопку "поднять предмет"

    private Image topItemImage; //отображение спрайтов предметов
    private Image bottomItemImage;

    private Text topItemText;  //отображение количества верхних предметов
    private Text bottomItemText;   //отображение количества нижних предметов

    private int IPUP;//item pick up pointer

    private void Start()
    {
        IPUP = 0;
        player = transform.Find("/Player").gameObject;
        Transform Root = transform.Find("Root");
        pickUpButton = Root.Find("PickUpItem").gameObject;
        topItemImage = Root.Find("TopItem").Find("Icon").GetComponent<Image>();
        topItemText = Root.Find("TopItem").Find("Count").GetComponent<Text>();
        bottomItemImage = Root.Find("BottomItem").Find("Icon").GetComponent<Image>();
        bottomItemText = Root.Find("BottomItem").Find("Count").GetComponent<Text>();
    }

    //была нажата какая-то кнопка худа
    public void ClickedButton(int butId)
    {
        if (!player.GetComponent<PlayerController>().GetCapacity())
        {   //игрок не может выполнить это действие сейчас
            return;
        }

        //Какие-то действия
        if (butId == 0 && TopItemId != -1)
        {   
            //кнопка верхнего предмета
            TopItemCount--; //уменьшаем кол-во в инвентаре
            gameController.ItemsList[TopItemId].Use(player.GetComponent<PlayerController>());   //применяем предмет
            if (TopItemCount <= 0)
            {
                //удалить предмет
                TopItemId = -1;
            }
        }
        else if(butId == 1 && BottomItemId != -1)
        {   //кнопка нижнего предмета
            BottomItemCount--; //уменьшаем кол-во в инвентаре
            gameController.ItemsList[BottomItemId].Use(player.GetComponent<PlayerController>());   //применяем предмет
            if (BottomItemCount <= 0)
            {
                //удалить предмет
                BottomItemId = -1;
            }
        }

        DisplayActual();
    }

    //поднять предмет, который лежит на земле
    public void PickUp()
    {
        if (!player.GetComponent<PlayerController>().GetCapacity())
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
        drop.transform.position = player.transform.position;
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
        ItemStay = Item;
        Debug.Log("ITEM!");
    }
}








































    /*
    //поднять предмет, который лежит на земле
    public void PickUp()
    {
        if (!player.GetComponent<PlayerController>().GetCapacity())
        {   //игрок не может выполнить это действие сейчас
            return;
        }
        *//*
        if (ItemStay != null)
        {
            if (TopItem == null)
            {   //врхний слот свободен, просто занесем
                ItemStay.GetComponent<Item>().PickUp(); //вызвать анимацию поднимания выбранного предмета
                TopItem = new Item(ItemStay.GetComponent<Item>()).gameObject; //поднять 
                ItemStayClear();//убрать информацию из ItemStay
                DisplayActual();
                return;
            }

            Item item = ItemStay.GetComponent<Item>();
            if (item.id == TopItem.GetComponent<Item>().id && (TopItem.GetComponent<Item>().count < Item.maxStack))
            {   //такой предмет уже есть в инвентаре и что-то можно положить
                SetNewCounter(TopItem.GetComponent<Item>(), item);
                ItemStay.GetComponent<Item>().PickUp(); //вызвать анимацию поднимания выбранного предмета
                ItemStayClear();//убрать информацию из ItemStay
                DisplayActual();
                return;
            }

            if (BottomItem == null)
            {   //врхний слот свободен, просто занесем
                ItemStay.GetComponent<Item>().PickUp(); //вызвать анимацию поднимания выбранного предмета
                BottomItem = new Item(ItemStay.GetComponent<Item>()).gameObject; //поднять
                ItemStayClear();//убрать информацию из ItemStay
                DisplayActual();
                return;
            }

            if (item.id == BottomItem.GetComponent<Item>().id && (BottomItem.GetComponent<Item>().count < Item.maxStack))
            {   //такой предмет уже есть в инвентаре и что-то можно положить
                SetNewCounter(BottomItem.GetComponent<Item>(), item);
                ItemStay.GetComponent<Item>().PickUp(); //вызвать анимацию поднимания выбранного предмета
                ItemStayClear();//убрать информацию из ItemStay
                DisplayActual();
                return;
            }

            //в остальных случаях нужно что-то выбросить
            DropItem(item.gameObject);
            ItemStay.GetComponent<Item>().PickUp(); //вызвать анимацию поднимания выбранного предмета
            ItemStayClear();//убрать информацию из ItemStay
            DisplayActual();



        }
        */
/*
    /// <summary>
    /// Add to cell count item count and over add to second cell count
    /// </summary>
    /// <param name="cell">Top or Bottom item</param>
    /// <param name="item">StayItem</param>
    private bool SetNewCounter(Item cell, Item item)
    {
        if(cell.count < Item.maxStack)
        {
            Item.Sum(cell, item);
        }
        

        if(cell == TopItem.GetComponent<Item>() && item.count > 0)
        {
            if (BottomItem == null)
            {
                BottomItem = cell.gameObject;
            }
            else if(cell.id == BottomItem.GetComponent<Item>().id)
            {
                cell = BottomItem.GetComponent<Item>();
                Item.Sum(cell, item);
            }
        }
        else
        {
            if (TopItem == null)
            {
                TopItem = cell.gameObject;
            }
            else if (cell.id == TopItem.GetComponent<Item>().id)
            {
                cell = TopItem.GetComponent<Item>();
                Item.Sum(cell, item); //добавляем оставшиеся предметы
            }
        }
        return item.count > 0;
    }


    

    //бросить предмет на землю
    public void DropItem(GameObject taken)
    {   //taken - предмет, который окажется вместо выброшенного
        if (IPUP == 0)
        {   //время дропнуть первый item
            TopItem = Instantiate(TopItem);
            TopItem.transform.position = player.transform.position;
            TopItem = new Item(taken.GetComponent<Item>()).gameObject; //поднять
        }
        else
        {   //пора дропнуть второй item
            BottomItem = Instantiate(BottomItem);
            BottomItem.transform.position = player.transform.position;
            BottomItem = new Item(taken.GetComponent<Item>()).gameObject; //поднять
        }
        IPUP += 1; IPUP %= 2;

    }

    //на змеле ничего не лежит
    public void ItemStayClear()
    {   //нужно убрать кнопку "поднять предмет", если она больше не нужна
        ItemStay = null;
        pickUpButton.SetActive(false);

        RaycastHit hit;
        if (Physics.Raycast(player.transform.position, Vector3.up, out hit, 2f, (1 << 12)))
        {   //тут что-то лежит
            pickUpButton.SetActive(true);
            ItemStay = hit.collider.gameObject;
        }
    }

    //на земле что-то лежит
    public void ItemStaySet(GameObject Item)
    {   //есть предмет, который можно поднять
        pickUpButton.SetActive(true);
        ItemStay = Item;
    }
}
*/

/*
            if(TopItem != null && BottomItem != null)
            {   //все слоты заняты
                if(TopItem.GetComponent<Item>().id == ItemStay.GetComponent<Item>().id)
                {   //такой предмет есть в первом слоте
                    Item i = TopItem.GetComponent<Item>();
                    if (i.count >= Item.maxStack)
                    {   //этот слот уже переполнен -> нужно попробовать добавить во второй
                        i = BottomItem.GetComponent<Item>();
                        if(i.id == ItemStay.GetComponent<Item>().id)
                        {   //в этом слоте лежит тот же предмет, что и поднимаемый
                            if(i.count >= Item.maxStack)
                            {   //к сожалению, этот стек тоже переполнен
                                //неудалось поднять предмет
                                return;
                            }
                            else
                            {   //в этом слоте есть место
                                i.PickUp(); //вызвать анимацию поднимания выбранного предмета
                                i = i + ItemStay.GetComponent<Item>();  //поднимаем столько сколько нужно
                            }
                        }
                        else
                        {   //в этом слоте лежит другой предмет
                            //нужно выкинуть этот предмет и взять тот, что на земле
                            DropItem(i.gameObject); //выкинуть
                            i.PickUp(); //вызвать анимацию поднимания выбранного предмета
                            BottomItem = ItemStay; //поднять
                            ItemStayClear();//убрать информацию из ItemStay
                        }
                    }
                    else
                    {   //в этом слоте еще есть место
                        i.PickUp(); //вызвать анимацию поднимания выбранного предмета
                        i = i + ItemStay.GetComponent<Item>();  //поднимаем столько сколько нужно
                    }
                }
                else if(BottomItem.GetComponent<Item>().id == ItemStay.GetComponent<Item>().id)
                {   //такой предмет уже есть во втором слоте
                    //этот слот уже переполнен -> нужно попробовать добавить во второй
                    Item i = BottomItem.GetComponent<Item>();
                    if (i.id == ItemStay.GetComponent<Item>().id)
                    {   //в этом слоте лежит тот же предмет, что и поднимаемый
                        if (i.count >= Item.maxStack)
                        {   //к сожалению, этот стек тоже переполнен
                            //неудалось поднять предмет
                            return;
                        }
                        else
                        {   //в этом слоте есть место
                            i.PickUp(); //вызвать анимацию поднимания выбранного предмета
                            i = i + ItemStay.GetComponent<Item>();  //поднимаем столько сколько нужно
                        }
                    }
                }
                else
                {   //ни в одном из слотов не лежит тот предмет, который лежит на земле
                    DropItem(ItemStay); //выкинуть любой
                    ItemStay.GetComponent<Item>().PickUp(); //вызвать анимацию поднимания выбранного предмета
                    BottomItem = ItemStay; //поднять
                    ItemStayClear();//убрать информацию из ItemStay
                }
            }
            else
            {
                if(TopItem == null)
                {
                    ItemStay.GetComponent<Item>().PickUp();     //вызвать анимацию поднимания выбранного предмета
                    TopItem = ItemStay;     //поднять
                    ItemStayClear();    //убрать информацию из ItemStay
                }
                else
                {
                    ItemStay.GetComponent<Item>().PickUp(); //вызвать анимацию поднимания выбранного предмета
                    BottomItem = ItemStay; //поднять
                    ItemStayClear();//убрать информацию из ItemStay
                }
            }
            */
