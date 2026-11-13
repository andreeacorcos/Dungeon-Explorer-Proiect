/*
Shuffle bag (curs C6 - Tips & Tricks, "aleatoriu controlat").
Pui jetoane intr-un sac, amesteci si extragi fara sa pui inapoi; cand sacul se goleste, se reumple.
Ex: 1 "coin" + 2 "nimic" => exact un coin la fiecare 3 inamici, dar intr-o ordine aleatoare,
deci "o data la 2-3 mobi", fara serii lungi fara drop ca la Random.value.
*/
using System.Collections.Generic;

public class ShuffleBag<T>
{
    private readonly List<T> items = new List<T>();
    private int cursor = -1; // -1 = sacul e gol si trebuie reamestecat

    public void Add(T item, int count)
    {
        for (int i = 0; i < count; i++)
            items.Add(item);
        cursor = -1;
    }

    public T Next()
    {
        if (items.Count == 0) return default;
        if (cursor < 0) Shuffle();

        T item = items[cursor];
        cursor--;
        return item;
    }

    private void Shuffle()
    {
        // Fisher-Yates
        for (int i = items.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (items[i], items[j]) = (items[j], items[i]);
        }
        cursor = items.Count - 1;
    }
}
