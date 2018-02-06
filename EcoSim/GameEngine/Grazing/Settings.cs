using System;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;

namespace GameEngine.Grazing
{
    //Pretty much done
    public class Settings
    {
        /// <summary>
        /// How many plants to create at the beginning of each simulation
        /// </summary>
        public int InitialPlantCount  = 20;

        /// <summary>
        /// How many plants can exist on the board at one time
        /// </summary>
        public int MaxPlantCount = 30;

        /// <summary>
        /// Turn interval between new plants
        /// </summary>
        public int NewPlantFrequency  = 5;
        /// <summary>
        /// Minimal plant mass cap
        /// </summary>
        public int MinPlantMass  = 10;
        /// <summary>
        /// Maximal plant mass cap
        /// </summary>
        public int MaxPlantMass  = 50;
        /// <summary>
        /// Minimal plant mass gain per turn
        /// </summary>
        public int MinGrowthRate  = 2;
        /// <summary>
        /// Maximal plant mass gain per turn
        /// </summary>
        public int MaxGrowthRate  = 8;
        /// <summary>
        /// Maximal animal speed in units per turn
        /// </summary>
        public float MovementSpeed  = 10;
        /// <summary>
        /// Range at which an animal may eat a plant
        /// </summary>
        public float InteractionDistance  = 5;
        /// <summary>
        /// The maximum ammount of food a creature can eat in one turn
        /// </summary>
        public int BiteSize  = 15;
        /// <summary>
        /// How much food a creature can hold in its stomach
        /// </summary>
        public int StomachCapacity  = 200;
        /// <summary>
        /// Range at which entities are visible
        /// </summary>
        public float SightRange  = 70;

        /// <summary>
        /// How much food the animal burns each turn
        /// </summary>
        public int PassiveWork  = 5;

        /// <summary>
        /// How much food movement burns
        /// </summary>
        public int MovementWork = 20;

        private Settings() { }

        public static Settings Default = new Settings();
        public float BrainRandomFactor = 0.001f;
        public float BrainRandomFactorMultiplier = 0.9999f;
        public string Brain = "Mark1";

        public static Settings Load(string fileName)
        {
            JObject settings;
            // read JSON directly from a file
            using(StreamReader file = File.OpenText(fileName))
            using(JsonTextReader reader = new JsonTextReader(file))
            {
                
                settings = JToken.ReadFrom(reader) as JObject;

                if (settings == null)
                    return null;
            }
            return settings.ToObject<Settings>();
        }
    }
}