using System;
using UnityEngine;
using UnityEngine.UI;

public class Compas : MonoBehaviour
{
    public static int SpriteCount = 16; //количество спрайтов на компас
    public Sprite[] Sprites = new Sprite[SpriteCount];
    public Transform Player; //сюды следует передать игрока
    public Vector3 Target;  //цель

    private Image compasImage; //изображение компаса
    private Vector3 nulVect = new Vector3(1, 0, 0);//условный ноль
    private float measure = 360 / SpriteCount;  //делитель угла поворота

    private void Start()
    {
        compasImage = transform.Find("Icon").GetComponent<Image>();
    }


    //возможна оптимизация: вызывать из PlayerController при движении
    private void Update()
    {   //найдем a вектор - единчиный направленный в сторону target
        Vector3 pPos = Player.position; pPos.y = 0;
        Vector3 a = (Target - Player.position).normalized;
        //найдем угол между условным нулем (1, 0) и вектором a
        double mpl = (nulVect.x * a.x + nulVect.z * a.z);
        float angle = (float)(Math.Acos(mpl) / Math.PI) * 180f;
        //вычислим коэфициент D, указывающий на сторону поворота
        float D = (nulVect.z * a.x) - (a.z * nulVect.x);

        int sNum;
        if (D > 0)
        {   //вектор a справа от nulVect -> sin отриц.
            sNum = (int)Math.Round((360f - angle) / measure);
        }
        else
        {   //вектора a слева от nulVect -> sin положит.
            sNum = (int)Math.Round(angle / measure);
        }
        //отобразим
        compasImage.sprite = Sprites[(SpriteCount + sNum - 2) % SpriteCount];
    }

   



}
