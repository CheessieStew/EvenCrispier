using GameEngine.Api;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameEngine.Simple
{
    public class AnimalSpecies
    {
        public string Name;
        public int SpeciesId;
        public AnimalTraits Traits;
        public int InitialCount;
        public List<EntityVariable> Descriptions;
        
        public void SetDescriptions()
        {
            Descriptions = new List<EntityVariable>()
            {
                new EntityVariable("Species ID", () => SpeciesId),
                new EntityVariable("Species Name", () => Name),
                new EntityVariable("Diet", () => Traits.Diet.ToString()),
                new EntityVariable("Strength", () => Traits.Strength),
                new EntityVariable("Metabolism", () => Traits.Metabolism),
                new EntityVariable("Stomach Capacity", () => Traits.StomachCapacity),
                new EntityVariable("Bite Size", () => Traits.BiteSize),
                new EntityVariable("Speed", () => Traits.Speed),
                new EntityVariable("Sight Range", () => Traits.SightRange),
                new EntityVariable("Mass", () => Traits.Mass),
            };
        }

        public static List<AnimalSpecies> Load(string fileName)
        {
            JArray speciesList;
            // read JSON directly from a file
            using (StreamReader file = File.OpenText(fileName))
            using (JsonTextReader reader = new JsonTextReader(file))
            {

                speciesList = JToken.ReadFrom(reader) as JArray;

                if (speciesList == null)
                    return null;
            }
            return speciesList.Select(jt => 
            {
                var res = jt.ToObject<AnimalSpecies>();
                res.SetDescriptions();
                return res;
            }).ToList();
        }
    }

    public class AnimalTraits
    {
        public Diet Diet;
        public int Strength;
        public int Metabolism;
        public int StomachCapacity;
        public int MaxHealth;
        public int BiteSize;
        public float Speed;
        public float SightRange;
        public int Mass;
    }

    [Flags]
    public enum Diet
    {
        None = 0,
        Carnivore = 1,
        Herbivore = 2,
        Omnivore = 3
    }
}