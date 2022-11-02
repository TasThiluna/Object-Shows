using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

public partial class objectShows : MonoBehaviour
{
    public new KMAudio audio;
    public KMBombInfo bomb;
    public KMBombModule module;

    public KMSelectable[] buttons;
    public Texture[] contesttextures;
    public Renderer[] buttonrenders;
    public Texture[] charactermats;
    public Texture winner;
    public Renderer contestname;
    public Transform dummy;

    private static readonly string[] contestNames = new[] { "wipeout", "underwater basket weaving", "water balloon fight", "cave diving", "chariot race", "equestrian acrobatics", "gladiatorial fight", "the objective games", "escape the volcano", "jungle survival", "tiger taming", "cliff climbing", "sack race", "interpretive dance", "nose nabbing", "calvinball" };
    private static readonly string[] ordinals = new[] { "first", "second", "third", "fourth", "fifth", "sixth" };
    private static readonly string[] placementOrdinals = new[] { "last", "fifth", "fourth", "third", "second", "first" };
    private static readonly string[] contestStrings = new[] { "KIO68QU9ZCPDSJMEVRAT1X53B427HLG0YFWN", "CYXD7SVI0NUTLJMQOHERF45G2986P31KWZAB", "BMVF31QZ0Y4SXJ5GIW7H6A2EPRLNTKUDC98O", "MWC509QI31NOSJB2FHUDXZ6PLV7TYK8G4ERA" };
    private static readonly string[] characterNames = new[] { "Battleship", "Beer", "Big Circle", "Black Hole", "Block", "Bulb", "Calendar", "Clock", "Combination Lock", "Cookie Jar", "Domino", "Fidget Spinner", "Hypercube", "Ice Cream", "iPhone", "Jack O' Lantern", "Lego", "Moon", "Necronomicon", "Paint Brush", "Radio", "Resistor", "Rubik's Clock", "Rubik's Cube", "Snooker Ball", "Sphere", "Sticky Note", "Stopwatch", "Sun", "Tennis Racket" };

    private int startingTime;
    private int startingDay;
    private int stage;
    private contestant[] solution;
    private int[] publicAppeals = new int[30];
    private List<contestant> contestantsPresent = new List<contestant>();
    private List<int> contests = new List<int>();
    private List<int> unpressedButtons;

    private static int moduleIdCounter = 1;
    private int moduleId;
    private bool moduleSolved;

    private void Awake()
    {
        moduleId = moduleIdCounter++;
        foreach (KMSelectable button in buttons)
            button.OnInteract += delegate () { buttonPress(button); return false; };
    }

    private void Start()
    {
        startingTime = (int)bomb.GetTime();
        startingDay = (int)DateTime.Now.DayOfWeek;
        startingDay++;
        Reset();
    }

    private void Reset()
    {
        contestantsPresent.Clear();
        contests.Clear();
        for (int i = 0; i < 6; i++)
            buttons[i].gameObject.SetActive(true);
        contestname.gameObject.SetActive(true);
        stage = 0;
        unpressedButtons = Enumerable.Range(0, 6).ToList();
        GetAppeals();
        PickCharacters();
        GetSolution();
        contestname.material.mainTexture = contesttextures[contests[0]];
    }

    private void PickCharacters()
    {
        var ser = bomb.GetSerialNumber().ToCharArray();
        var alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        for (int i = 0; i < 6; i++)
            while (ser.Take(i).Contains(ser[i]))
                ser[i] = alphabet[(alphabet.IndexOf(ser[i]) + 1) % 36];
        int index;
        contestantsPresent.Clear();
        for (int i = 0; i < 6; i++)
        {
            index = rnd.Range(0, 30);
            while (contestantsPresent.Any(chr => chr.id == index))
                index = rnd.Range(0, 30);
            var character = new contestant { id = index, pos = i, scores = new int[5], appeal = publicAppeals[index], snchar = ser[i] };
            while (contestantsPresent.Any(chr => chr.appeal == character.appeal))
                character.appeal++;
            contestantsPresent.Add(character);
            buttonrenders[i].material.mainTexture = charactermats[index];
            Debug.LogFormat("[Object Shows #{0}] the {2} character is {1}, who has a public appeal of {3}.", moduleId, characterNames[index], ordinals[i], character.appeal);
        }
    }

    private void GetSolution()
    {
        var currentcharacters = contestantsPresent.ToList();
        solution = new contestant[5];
        var contestindices = Enumerable.Range(0, 16).ToList().Shuffle();
        for (int i = 0; i < 5; i++)
        {
            int contestindex = contestindices[i];
            int typeindex = contestindex % 4;
            int styleindex = contestindex / 4;
            if (i != 4)
            {
                Debug.LogFormat("[Object Shows #{0}] the {2} contest is {1}.", moduleId, contestNames[contestindex], ordinals[i]);
                contests.Add(contestindex);
            }
            for (int j = 0; j < currentcharacters.Count; j++)
            {
                if (styleindex == 2 || styleindex == 3)
                    currentcharacters[j].scores[i] = 35 - contestStrings[typeindex].IndexOf(currentcharacters[j].snchar);
                else
                    currentcharacters[j].scores[i] = contestStrings[typeindex].IndexOf(currentcharacters[j].snchar);
            }
            var sortedcharacters = currentcharacters.OrderBy(chr => chr.scores[i]).ToList();
            if (i < 3)
            {
                var lowestappeal = sortedcharacters.Take(i == 0 ? 3 : 2).Min(chr => chr.appeal);
                Debug.LogFormat("[Object Shows #{0}] The scores are: {1}.", moduleId, sortedcharacters.Select(chr => characterNames[chr.id] + " = " + chr.scores[i]).Join(", "));
                solution[i] = currentcharacters.First(chr => chr.appeal == lowestappeal);
            }
            else if (i == 3)
            {
                solution[i] = sortedcharacters[0];
                Debug.LogFormat("[Object Shows #{0}] The scores are: {1}.", moduleId, sortedcharacters.Select(chr => characterNames[chr.id] + " = " + chr.scores[i]).Join(", "));
            }
            else
            {
                var lowestappeal = sortedcharacters.Min(chr => chr.appeal);
                solution[i] = currentcharacters.First(chr => chr.appeal == lowestappeal);
            }
            currentcharacters.Remove(solution[i]);
            Debug.LogFormat("[Object Shows #{0}] the character in {2} place is {1}.", moduleId, characterNames[solution[i].id], placementOrdinals[i]);
        }
    }

    private void buttonPress(KMSelectable button)
    {
        if (!moduleSolved)
        {
            button.AddInteractionPunch(.5f);
            if (contestantsPresent[Array.IndexOf(buttons, button)] != solution[stage])
            {
                module.HandleStrike();
                audio.PlaySoundAtTransform("strike" + rnd.Range(1, 3), transform);
                Debug.LogFormat("[Object Shows #{0}] Strike! Resetting...", moduleId);
                Reset();
            }
            else
            {
                button.gameObject.SetActive(false);
                stage++;
                if (stage == 5)
                {
                    moduleSolved = true;
                    module.HandlePass();
                    audio.PlaySoundAtTransform("solve1", transform);
                    Debug.LogFormat("[Object Shows #{0}] Module solved!", moduleId);
                    StartCoroutine(SolveAnimation());
                }
                else if (stage == 4)
                {
                    contestname.gameObject.SetActive(false);
                    audio.PlaySoundAtTransform("elimination", button.transform);
                    unpressedButtons.Remove(Array.IndexOf(buttons, button));
                }
                else
                {
                    contestname.material.mainTexture = contesttextures[contests[stage]];
                    audio.PlaySoundAtTransform("elimination", button.transform);
                    unpressedButtons.Remove(Array.IndexOf(buttons, button));
                }
            }
        }
    }

    private void GetAppeals()
    {
        var ser = bomb.GetSerialNumber();
        publicAppeals[0] = (bomb.GetBatteryCount() + bomb.GetIndicators().Count()) % 7; // Battleship
        publicAppeals[1] = bomb.GetModuleNames().Count() - bomb.GetSolvableModuleNames().Count(); // Beer
        publicAppeals[2] = bomb.GetPortCount(Port.Parallel) + bomb.GetPortCount(Port.DVI); // Big Circle
        publicAppeals[3] = ser[2] - '0' + ser[5] - '0'; // Black Hole
        publicAppeals[4] = bomb.GetPortCount(Port.Serial); // Block
        publicAppeals[5] = bomb.GetIndicators().Count(); // Bulb
        publicAppeals[6] = ((bomb.GetSerialNumberNumbers().Sum() - 1) % 9) + 1; // Calendar
        publicAppeals[7] = bomb.GetPortCount(Port.Serial) + bomb.GetPortCount(Port.Parallel); // Clock
        publicAppeals[8] = bomb.GetTwoFactorCounts(); // Combination Lock
        publicAppeals[9] = bomb.GetModuleNames().Count() % 10; // Cookie Jar
        publicAppeals[10] = bomb.GetOnIndicators().Count(); // Domino
        publicAppeals[11] = startingTime / 60; // Fidget Spinner
        publicAppeals[12] = bomb.GetBatteryCount(); // Hypercube
        publicAppeals[13] = ser[5] - '0'; // Ice Cream
        publicAppeals[14] = bomb.GetBatteryHolderCount(); // iPhone
        publicAppeals[15] = bomb.GetPortPlates().Count(); // Jack O' Lantern
        publicAppeals[16] = bomb.GetOffIndicators().Count(); // Lego
        publicAppeals[17] = bomb.GetPortCount(Port.PS2); // Moon
        publicAppeals[18] = (ser[4] - 'A' + 1) % 10; // Necronomicon
        publicAppeals[19] = bomb.GetIndicators().Count(ind => ser.Intersect(ind).Any()); // Paint Brush
        publicAppeals[20] = bomb.GetBatteryCount(Battery.AA); // Radio
        publicAppeals[21] = bomb.GetSerialNumberNumbers().Sum(); // Resistor
        publicAppeals[22] = (bomb.GetSerialNumberLetters().Select(let => let - 'A' + 1).Sum() - 1) % 9 + 1; // Rubik's Clock
        publicAppeals[23] = bomb.GetPortCount(Port.RJ45); // Rubik's Cube
        publicAppeals[24] = bomb.GetModuleNames().Count(mdl => new string[] { "color", "colour", "colo(u)r" }.Any(s => mdl.ContainsIgnoreCase(s))); // Snooker Ball
        publicAppeals[25] = bomb.GetBatteryCount(Battery.D); // Sphere
        publicAppeals[26] = startingDay; // Sticky Note
        publicAppeals[27] = ((startingTime - 1) % 9) + 1; // Stopwatch
        publicAppeals[28] = bomb.GetModuleNames().Count(mdl => mdl.ContainsIgnoreCase("simon") || mdl.ContainsIgnoreCase("maze") || mdl.ContainsIgnoreCase("morse")); // Sun
        publicAppeals[29] = (ser[3] - 'A' + 1) % 10; // Tennis Racket
    }

    private IEnumerator SolveAnimation()
    {
        var elapsed = 0f;
        var duration = .75f;
        foreach (KMSelectable button in buttons)
            button.Highlight.gameObject.SetActive(false);
        var finalButton = buttons[Enumerable.Range(0, 6).Where(x => !solution.Select(chr => chr.pos).ToArray().Contains(x)).First()].transform;
        finalButton.SetParent(dummy);
        var startPosition = finalButton.localPosition;
        var finalPosition = new Vector3(0f, .0101f, 0f);
        var startScale = finalButton.localScale;
        var finalScale = new Vector3(.12f, .12f, 1f);
        while (elapsed < duration)
        {
            finalButton.localPosition = new Vector3(Mathf.Lerp(startPosition.x, 0f, elapsed / duration), Mathf.Lerp(startPosition.y, .0101f, elapsed / duration), Mathf.Lerp(startPosition.z, 0f, elapsed / duration));
            finalButton.localScale = new Vector3(Mathf.Lerp(startScale.x, .12f, elapsed / duration), Mathf.Lerp(startScale.y, .12f, elapsed / duration), 1f);
            yield return null;
            elapsed += Time.deltaTime;
        }
        finalButton.localPosition = finalPosition;
        finalButton.localScale = finalScale;
    }

    // Twitch Plays
#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} press <pos> [Presses the specified object in position 'pos'] | Valid object positions are tl, tr, ml, mr, bl, br";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        string[] parameters = command.Split(' ');
        if (Regex.IsMatch(parameters[0], @"^\s*press\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (parameters.Length > 2)
            {
                yield return "sendtochaterror Too many parameters!";
            }
            else if (parameters.Length == 2)
            {
                if (parameters[1].EqualsIgnoreCase("tl"))
                {
                    buttons[0].OnInteract();
                }
                else if (parameters[1].EqualsIgnoreCase("tr"))
                {
                    buttons[1].OnInteract();
                }
                else if (parameters[1].EqualsIgnoreCase("mr"))
                {
                    buttons[2].OnInteract();
                }
                else if (parameters[1].EqualsIgnoreCase("br"))
                {
                    buttons[3].OnInteract();
                }
                else if (parameters[1].EqualsIgnoreCase("bl"))
                {
                    buttons[4].OnInteract();
                }
                else if (parameters[1].EqualsIgnoreCase("ml"))
                {
                    buttons[5].OnInteract();
                }
                else
                {
                    yield return "sendtochaterror The specified position of the object to press '" + parameters[1] + "' is invalid!";
                }
            }
            else if (parameters.Length == 1)
            {
                yield return "sendtochaterror Please specify the position of the object to press!";
            }
            yield break;
        }
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        int start = solution.Length - (unpressedButtons.Count() - 1);
        for (int i = start; i < solution.Length; i++)
        {
            buttons[solution[i].pos].OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
    }
}
