using System;

public class Easing
{
    /// <summary>
    /// Функция, возрастающая практически моментально
    /// </summary>
    public static float easeOutExpo(float x)
    {
        return (float)(x == 1f ? 1f : 1f - Math.Pow(2f, -10f * x));
    }
    public static float easeInOutQuad(float x)
    {
        return (float)(x < 0.5 ? 2 * x * x : 1 - Math.Pow(-2 * x + 2, 2) / 2);
    }

    public static float easeInOutQuart(float x)
    {
        return x < 0.5 ? (float)(8 * x * x * x * x) : (float)(1 - Math.Pow(-2 * x + 2, 4) / 2);
    }

    public static float easeOutQuint(float x)
    {
        return (float)(1 - Math.Pow(1 - x, 5));
    }

    /// <summary>
    /// Плавная функция, возрастает линейно, под конец замедляется
    /// </summary>
    public static float easeOutSine(float x) {
      return (float) Math.Sin((x* Math.PI) / 2);
    }
}
