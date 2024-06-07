using UnityEngine;
using System.Collections.Generic;

namespace GreatClock.Common.IconAtlas {

	internal class AtlasChecker : MonoBehaviour {

		private static bool inited = false;
		private static AtlasChecker instance = null;

		public static void Register(IOnUpdate obj) {
			if (!inited) {
				inited = true;
				GameObject g = new GameObject("RenderTextureAtlasChecker");
				DontDestroyOnLoad(g);
				instance = g.AddComponent<AtlasChecker>();
			}
			if (instance != null && !instance.Equals(null)) {
				instance.mUpdaters.Add(obj);
			}
		}

		public static void Unregister(IOnUpdate obj) {
			if (instance != null && !instance.Equals(null)) {
				instance.mUpdaters.Remove(obj);
			}
		}

		private List<IOnUpdate> mUpdaters = new List<IOnUpdate>();

		void Update() {
			for (int i = 0, imax = mUpdaters.Count; i < imax; i++) {
				IOnUpdate obj = mUpdaters[i];
				if (obj == null) { continue; }
				try {
					obj.OnUpdate();
				} catch (System.Exception e) {
					Debug.LogException(e);
				}
			}
		}

	}

	public interface IOnUpdate {
		void OnUpdate();
	}

}