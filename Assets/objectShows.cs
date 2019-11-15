using System;
﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

public class objectShows : MonoBehaviour
 {
	 	public KMAudio Audio;
		public KMBombInfo bomb;

    public KMSelectable[] buttons;
    public Renderer[] buttonrenders;
    public Material[] charactermats;
    public Material[] othermats;
    public TextMesh maintext;

    private static readonly string[] contestnames = new string[16] {"wipeout", "underwater basket weaving", "water balloon fight", "cave diving", "chariot race", "equestrian acrobatics", "gladiatorial fight", "the objective games", "escape the volcano", "jungle survival", "tiger taming", "cliff climbing", "sack race", "interpretive dance", "nose nabbing", "calvinball" };
    private static readonly string[] ordinals = new string[5] {"first", "second", "third", "fourth", "fifth"};
    private static readonly string[] charlists = new string[4] {"KI68QU9ZCPDSJMEVRAT1X53B427HLG0FWN", "CXD7SVI0NUTLJMQHERF45G2986P31KWZAB", "BMVF31QZ04SJ5GIW7H6A2EPRLNTKUDC98X", "MWC509QI31NSJB2FHUDXZ6PLV7TK8G4ERA"};

    private int startingtime;
    private int startingday;
    private int stage;
    private int typeindex;
    private int styleindex;
    private int[] solution;
    private int[] publicappeals = new int[30];
    private int[] relevantpublicappeals = new int[6];
    private List <int> chosencharacters = new List <int>();

		static int moduleIdCounter = 1;
		int moduleId;
		private bool moduleSolved;

		void Awake()
		{
        moduleId = moduleIdCounter++;
        startingtime = (int)bomb.GetTime();
        startingday = (int)DateTime.Now.DayOfWeek;
        if (startingday == 0)
          startingday = 7;
		}

		void Start()
		{
      chosencharacters.Clear();
      stage = 0;
      getAppeals();
      pickCharacters();
      getSolution();
    }

    void pickCharacters()
    {
      int index;
      chosencharacters.Clear();
      for(int i = 0; i < 6; i++)
      {
        index = UnityEngine.Random.Range(0,30);
        while(chosencharacters.Contains(index))
          index = UnityEngine.Random.Range(0,30);
        chosencharacters.Add(index);
        buttonrenders[i].material = charactermats[index];
        relevantpublicappeals[i] = publicappeals[index];
		  }
    }

    void getSolution()
    {
      var ser = bomb.GetSerialNumber();
      solution = new int[5];
      for (int i = 0; i < 5; i++)
      {
          typeindex = rnd.Range(0,4);
          styleindex = rnd.Range(0,4);
          Debug.LogFormat("[Object Shows #{0}] the {2} contest is {1}.", moduleId, contestnames[styleindex * 4 + typeindex], ordinals[i]);
          var scores = new List <int>(chosencharacters.Count);
          for (int j = 0; j < chosencharacters.Count; j++)
            scores.Add(charlists[typeindex].IndexOf(ser[j]));
          var sortedscores = scores.ToList();
          if ( i == 0)
          {
            var ufe = sortedscores.Take(3);
            var scoringpubs = new int[3];
          //solution[i] =
          }
          else if (i == 1 || i == 2)
          {
            var ufe = sortedscores.Take(2);
            //solution[i] =
          }
          else if (i == 3)
          {
            //solution[i] = scores.IndexOf(sortedscores.First());
          }
          else
          {
            //solution[i] =
          }
      }
    }

    void getAppeals()
    {
      var ser = bomb.GetSerialNumber();
      var alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
      publicappeals[0] = (bomb.GetBatteryCount() + bomb.GetIndicators().Count()) % 7; //Battleship
      publicappeals[1] = bomb.GetModuleNames().Count() - bomb.GetSolvableModuleNames().Count(); //Beer
      publicappeals[2] = bomb.GetPortCount(Port.Parallel) + bomb.GetPortCount(Port.DVI); //Big Circle
      publicappeals[3] = ser[2] - '0' + ser[5] - '0'; //Black Hole
      publicappeals[4] = bomb.GetPortCount(Port.Serial); //Block
      publicappeals[5] = bomb.GetIndicators().Count(); //Bulb
      publicappeals[6] = (bomb.GetSerialNumberNumbers().Sum() - 1) % 9 + 1; //Calendar
      publicappeals[7] = bomb.GetPortCount(Port.Serial) + bomb.GetPortCount(Port.Parallel); //Clock
      publicappeals[8] = bomb.GetTwoFactorCounts(); //Combination Lock
      publicappeals[9] = bomb.GetModuleNames().Count() % 10; //Cookie Jar
      publicappeals[10] = bomb.GetOnIndicators().Count(); //Domino
      publicappeals[11] = startingtime / 60; //Fidget Spinner
      publicappeals[12] = bomb.GetBatteryCount(); //Hypercube
      publicappeals[13] = ser[5] - '0'; //Ice Cream
      publicappeals[14] = bomb.GetBatteryHolderCount(); //iPhone
      publicappeals[15] = bomb.GetPortPlates().Count(); //Jack O' Lantern
      publicappeals[16] = bomb.GetOffIndicators().Count(); //Lego
      publicappeals[17] = bomb.GetPortCount(Port.PS2); //Moon
      publicappeals[18] = (ser[4] - 'A' + 1) % 10; //Necronomicon
      publicappeals[19] = bomb.GetIndicators().Count(ind => ser.Intersect(ind).Any()); //Paint Brush
      publicappeals[20] = bomb.GetBatteryCount(Battery.AA); //Radio
      publicappeals[21] = bomb.GetSerialNumberNumbers().Sum(); //Resistor
      publicappeals[22] = (bomb.GetSerialNumberLetters().Select(let => let - 'A' + 1).Sum() - 1) % 9 + 1; //Rubik's Clock
      publicappeals[23] = bomb.GetPortCount(Port.RJ45); //Rubik's Cube
      publicappeals[24] = bomb.GetModuleNames().Count(mdl => !mdl.ContainsIgnoreCase("e")); //Snooker Ball
      publicappeals[25] = bomb.GetBatteryCount(Battery.D); //Sphere
      publicappeals[26] = startingday; //Sticky Note
      publicappeals[27] = (startingtime - 1) % 9 + 1; //Stopwatch
      publicappeals[28] = bomb.GetModuleNames().Count(mdl => mdl.ContainsIgnoreCase("simon") || mdl.ContainsIgnoreCase("maze") || mdl.ContainsIgnoreCase("morse")); //Sun
      publicappeals[29] = (ser[3] - 'A' + 1) % 10; //Tennis Racket
    }
}
