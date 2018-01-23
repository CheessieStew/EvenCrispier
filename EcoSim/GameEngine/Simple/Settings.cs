using System;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;

namespace GameEngine.Simple
{
    public class Settings
    {
        public int MinPlantMass = 10;
        public int MaxPlantMass = 50;
        public int NewPlantFrequency = 5;
        public int MinGrowthRate = 2;
        public int MaxGrowthRate = 8;
        public float MovementDistanceScale = 10;

        public float InteractionDistance = 15;

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