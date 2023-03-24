using UnityEngine;
using System.Collections;

namespace Flux
{
	public class FParticleTrack : FTrack {

//		public override CacheMode AllowedCacheMode { get { return CacheMode.Editor | CacheMode.RuntimeBackwards | CacheMode.RuntimeForward; } }
//		public override CacheMode RequiredCacheMode { get { return CacheMode.Editor | CacheMode.RuntimeBackwards; } }
		public override CacheMode AllowedCacheMode { get { return 0; } }
		public override CacheMode RequiredCacheMode { get { return 0; } }

        public override void OnEditorInit()
        {
            base.OnEditorInit();
            ParticleSystem = Owner.GetComponentInChildren<ParticleSystem>();
        }

        public ParticleSystem ParticleSystem { get; private set; }

		public override void Init()
		{
			ParticleSystem = Owner.GetComponentInChildren<ParticleSystem>();
			base.Init();
		}

        public override void OnOwnerChange(Transform owner)
        {
            base.OnOwnerChange(owner);
            Init();
            Debug.Log("yns ReInit");
        }

        //		public override void CreateCache()
        //		{
        //			FParticleTrackCache particleTrackCache = new FParticleTrackCache(this);
        //			particleTrackCache.Build(true);
        //
        //			Cache = particleTrackCache;
        //		}
        //
        //		public override void ClearCache()
        //		{
        //			FParticleTrackCache particleTrackCache = (FParticleTrackCache)Cache;
        //			if( particleTrackCache != null )
        //			{
        //				particleTrackCache.Clear();
        //			}
        //			Cache = null;
        //		}
        //
        //		public override bool CanCreateCache()
        //		{
        //			return ParticleSystem != null;
        //		}	

        //		public override void UpdateEvents(int frame, float time)
        //		{
        //			if( HasCache )
        //			{
        //				Cache.GetPlaybackAt(time);
        //			}
        //			else
        //			{
        //				base.UpdateEvents(frame, time);
        //			}
        //		}
    }
}
