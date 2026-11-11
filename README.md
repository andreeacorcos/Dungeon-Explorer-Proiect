# Dungeon Explorer

Dungeon Explorer este un joc 3D, bazat pe niveluri (stil dungeon/peșteră), dezvoltat în Unity pentru un proiect academic. Jucătorul controlează un personaj care trebuie să supraviețuiască valurilor de monștri, să colecteze arme noi si monede pentru upgrade-uri.

## 🎮 Gameplay și Mecanici

*   **Progresie:** Jocul este format dintr-un șir de niveluri (camere). Pentru a trece la nivelul următor, jucătorul trebuie să învingă toți monștrii, ultimul urmand sa "drop-eze" o cheie pentru a deschide ușa de ieșire.
*   **Permadeath (Stil Arcade):** Dacă jucătorul moare, progresul se resetează și jocul reîncepe de la Nivelul 1. Există un Scoreboard care reține nivelul maxim atins.
*   **Arme și Combat:** 
    * La începutul scenei, jucătorul poate ridica o armă de pe jos (Weapon Pickup).
    * Diferite arme devin disponibile pe măsură ce avansezi în niveluri, fiecare având caracteristici unice (Damage, Attack Speed, Range).
    * Loviturile critice (Critical Hits) au o șansă de a produce damage suplimentar.
    * Mecanică de "Dodge" implementată pentru a evita atacurile.
*   **Sistem de Monede și Upgrade-uri:** Monștrii dropează ocazional monede (coins). Acestea sunt contorizate pe ecran și pot fi folosite într-un shop (care apare pe fundal în nivelurile superioare) pentru a cumpăra upgrade-uri, boost-uri și superputeri pentru arme.
*   **Bonusuri și Capcane:** În niveluri vor apărea iteme de vindecare (Heal), dar și iteme capcană (Trap) care au un efect negativ. Completarea unui nivel într-un timp scurt oferă puncte bonus.

## 🕹️ Inamicii (Mobs & Bosses)

*   Monștrii vin în valuri din stânga și din dreapta ecranului.
*   Nu folosesc arme (atac de bază tip melee); pot răni jucătorul doar când sunt în proximitate.
*   Hp-ul lor nu se regenerează.
*   Necesită un număr diferit de lovituri în funcție de nivelul jucătorului/armei.
*   Ultimul inamic al nivelului dropează cheia necesară progresiei.

## 🛠️ Interfață și Meniu (UI)

*   **In-Game:** Jucătorul are un Health Bar vizibil și un contor pentru monedele colectate.
*   **Main Menu:**
    *   Start Game
    *   Highest Level (Scoreboard)
    *   Settings (Volum muzică, Luminozitate, Sensibilitate mouse)

## 💡 Idei în Dezvoltare (Roadmap)

Sistemul de arme și upgrade-uri este planificat să fie extins cu efecte speciale:
*   **Arme speciale:** Armă de gheață (încetinește inamicii), Grenade AOE (knockback), Armă cu gloanțe penetrante (lovesc mai mulți inamici cu damage drop-off), etc.
*   **Upgrade-uri (Status Effects):** Muniție cu efecte de Poison, Fire (care se răspândește), sau Knockback direcțional.
*   **Abilități per nivel:** Barieră temporară de protecție (2 secunde, permite folosirea grenadelor/heal), super strenght/speed.

## 👥 Echipa de Dezvoltare

*   **Andreea** (Team Leader) - Concepție joc de bază, UI (Health bar, Coin counter).
*   **Rareș** (Scripter/Coder) - Movement, Dodge mechanic, Mob basic attacks, Timer bonus.
*   **Luca** - Sistem iteme de vindecare (Heal items).
*   **Victor** - Sistem iteme capcană (Trap items).
*   **Ștefan** - Sistem arme (Pickup, Stats, Critical hits) și magazin Upgrade-uri.

## MODIFICARI (Eventualele schimbari vor fi notate aici)

* 
