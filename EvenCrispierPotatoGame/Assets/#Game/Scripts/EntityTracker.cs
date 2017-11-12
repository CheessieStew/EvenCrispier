using System.Collections;
using Scripts.GameModelDefinitions;
using Scripts.Utils;
using UnityEngine;

namespace Scripts
{
	public class EntityTracker : MonoBehaviour
	{
		public WorldTracker World;
		public AbstractEntity TrackedAnimal;
		public BubbleMaker BubbleMaker;
	
	
		public void UpdateExternal(float timeGranted)
		{
			BubbleMaker.MakeBubble(TrackedAnimal.LastInstruction?.ToString(), gameObject);
			StartCoroutine(LerpToPosition(timeGranted, transform.position, TrackedAnimal.Position.ToVector3()));
		}

		public IEnumerator LerpToPosition(float timeGranted, Vector3 pos1, Vector3 pos2)
		{
			float time = 0;
			while (time < timeGranted)
			{
				time += Time.deltaTime;
				transform.position = Vector3.Lerp(pos1, pos2, time / timeGranted);
				yield return null;
			}
		}
	}

	
}
