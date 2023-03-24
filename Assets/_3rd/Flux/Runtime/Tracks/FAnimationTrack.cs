using UnityEngine;
using System.Collections.Generic;

namespace Flux
{
    public class FAnimationTrack : FTransformTrack
    {
        // Animation Previews, stored in <sequenceInstanceId, <ownerInstanceId, animPreview>>
        private static Dictionary<int, Dictionary<int, FAnimationTrackCache>> _animPreviews = new Dictionary<int, Dictionary<int, FAnimationTrackCache>>();

        public override void OnEditorInit()
        {
            base.OnEditorInit();
            //_animatorController = Owner.GetComponent<Animator>()?.runtimeAnimatorController;
            if (_animatorController == null)
                _animatorController = Owner.GetComponentInChildren<Animator>()?.runtimeAnimatorController;
            _layerId = 0;
            (Events[0] as FPlayAnimationEvent)._animationClip = _animatorController.animationClips[0];
        }
        public static FAnimationTrackCache GetAnimationPreview(FSequence sequence, Transform owner)
        {
            Dictionary<int, FAnimationTrackCache> sequencePreviews = null;

            FAnimationTrackCache animationTrackCache = null;

            if (_animPreviews.TryGetValue(sequence.GetInstanceID(), out sequencePreviews))
            {
                sequencePreviews.TryGetValue(owner.GetInstanceID(), out animationTrackCache);
            }

            return animationTrackCache;
        }

        private static FAnimationTrackCache GetAnimationPreview(FAnimationTrack animTrack)
        {
            return GetAnimationPreview(animTrack, true);
        }

        private static FAnimationTrackCache GetAnimationPreview(FAnimationTrack animTrack, bool createIfDoesntExist)
        {
            Dictionary<int, FAnimationTrackCache> sequencePreviews = null;
            if (!_animPreviews.TryGetValue(animTrack.Sequence.GetInstanceID(), out sequencePreviews))
            {
                if (!createIfDoesntExist)
                    return null;
                sequencePreviews = new Dictionary<int, FAnimationTrackCache>();
                _animPreviews.Add(animTrack.Sequence.GetInstanceID(), sequencePreviews);
            }

            FAnimationTrackCache preview = null;
            if (!sequencePreviews.TryGetValue(animTrack.Owner.GetInstanceID(), out preview))
            {
                if (!createIfDoesntExist)
                    return null;
                preview = new FAnimationTrackCache(animTrack);
                sequencePreviews.Add(animTrack.Owner.GetInstanceID(), preview);
            }

            return preview;
        }

        private static void DeleteAnimationPreview(FAnimationTrack animTrack)
        {
            Dictionary<int, FAnimationTrackCache> sequencePreviews = null;
            if (_animPreviews.TryGetValue(animTrack.Sequence.GetInstanceID(), out sequencePreviews))
            {
                sequencePreviews[animTrack.Owner.GetInstanceID()].Clear();
                sequencePreviews.Remove(animTrack.Owner.GetInstanceID());
            }
        }

        public static void DeleteAnimationPreviews(FSequence sequence)
        {
            if (sequence == null)
                return;
            Dictionary<int, FAnimationTrackCache> sequencePreviews = null;
            if (_animPreviews.TryGetValue(sequence.GetInstanceID(), out sequencePreviews))
            {
                Dictionary<int, FAnimationTrackCache>.Enumerator e = sequencePreviews.GetEnumerator();
                while (e.MoveNext())
                {
                    e.Current.Value.Clear();
                }
                sequencePreviews.Clear();

                _animPreviews.Remove(sequence.GetInstanceID());
            }
        }

        // what is the animation controller that will be used to 
        // build this track's animation state machine
        [SerializeField]
        [HideInInspector]
        private RuntimeAnimatorController _animatorController = null;
        public RuntimeAnimatorController AnimatorController { get { return _animatorController; } }

        [SerializeField]
        [HideInInspector]
        private string _layerName = null;
        public string LayerName { get { return _layerName; } }

        [SerializeField]
        [HideInInspector]
        private int _layerId = -1;
        public int LayerId { get { return _layerId; } }

        //		private TransformSnapshot _snapshot = null;
        //		public TransformSnapshot Snapshot { get { return _snapshot; } }

        public override CacheMode RequiredCacheMode
        {
            get
            {
                return CacheMode.Editor | CacheMode.RuntimeBackwards;
            }
        }

        public override CacheMode AllowedCacheMode
        {
            get
            {
                return RequiredCacheMode | CacheMode.RuntimeForward;
            }
        }

        public override void Init()
        {
            if (Owner.GetComponent<Animator>() == null)
                Owner.gameObject.AddComponent<Animator>();

            base.Init();

            _snapshot.TakeChildSnapshots();
        }

        public override void Stop()
        {
            if (HasCache && Cache.Track == this)
            {
                ((FAnimationTrackCache)Cache).StopPlayback();
                Owner.GetComponent<Animator>().enabled = false;
            }

            base.Stop();
        }

        public override void UpdateEventsEditor(int frame, float time)
        {
            if (HasCache && Cache.Track == this)
            {
                GetPreviewAt(time);
            }
        }

        public override void UpdateEvents(int frame, float time)
        {
            if (Sequence.Speed != 1 && !HasCache)
                Owner.GetComponent<Animator>().speed = Sequence.Speed;
            if (HasCache)
            {
                if (Cache.Track == this) // only one of them needs to set the playback
                    GetPreviewAt(time);
            }
            else
                base.UpdateEvents(frame, time);
        }

        public override void CreateCache()
        {
            FAnimationTrackCache preview = GetAnimationPreview(this);
            preview.Build(true);
        }

        public override void ClearCache()
        {
            FAnimationTrackCache preview = (FAnimationTrackCache)Cache;//GetAnimationPreview( this, false );
            if (preview != null)
            {
                if (preview.NumberTracksCached <= 1)
                    DeleteAnimationPreview(this);
                else
                    preview.Build(true);
            }
            Cache = null;
        }

        public override bool CanCreateCache()
        {
            if (_animatorController == null)
                return false;

            List<FEvent> evts = Events;
            for (int i = 0; i != evts.Count; ++i)
            {
                if (((FPlayAnimationEvent)evts[i])._animationClip == null)
                    return false;
            }

            return true;
        }

        private void GetPreviewAt(float time)
        {
            Cache.GetPlaybackAt(time);
        }

        private bool HasAnimationOnFrame(int frame)
        {
            FEvent[] evts = new FEvent[2];
            int numEvents = GetEventsAt(frame, evts);
            if (numEvents == 0)
                return false;

            return ((FPlayAnimationEvent)evts[0])._animationClip != null || (numEvents == 2 && ((FPlayAnimationEvent)evts[1])._animationClip != null);
        }
    }
}
