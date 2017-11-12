using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Scripts.GameModelDefinitions.Ai;
using UnityEngine;

namespace Scripts.GameModelDefinitions
{
    public abstract class AbstractWorld : IWorld
    {
        public delegate void EntityEventHandler(AbstractEntity e);

        public event EntityEventHandler NewEntity;
        public event EntityEventHandler EntityRemoved;

        public IEnumerable<AbstractEntity> Entities => Species.Values.SelectMany(kvp => kvp.Values);
        protected Dictionary<int, Dictionary<int, AbstractEntity>> Species = new Dictionary<int, Dictionary<int, AbstractEntity>>();
        public Vector Dimensions { get; }

        protected AbstractWorld(Vector dimensions)
        {
            Dimensions = dimensions;
        }

        public virtual AbstractEntity GetEntity(int species, int id)
        {
            var speciesDisc = Species.ContainsKey(species) ? Species[species] : null;
            return speciesDisc?.ContainsKey(id) == true ? speciesDisc[id] : null;
        }
        
        public virtual void NextFrame()
        {
            foreach (var entity in Entities)
            {
                entity.Update();
            }
        }
        
        public void RemoveEntity(AbstractEntity e)
        {
            Species[e.Species].Remove(e.Id);
			EntityRemoved?.Invoke(e);
        }
        
        public void AddEntity(AbstractEntity e)
        {
            if (!Species.ContainsKey(e.Species))
                Species[e.Species] = new Dictionary<int, AbstractEntity>();
            Species[e.Species].Add(e.Id, e);
            NewEntity?.Invoke(e);
        }
    }
}