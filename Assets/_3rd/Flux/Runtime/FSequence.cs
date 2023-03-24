using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

/**
 * @brief Flux is top namespace for everything pertaining to the runtime.
 */
namespace Flux
{
	public enum ActionOnStart
	{
		None = 0,
		Initialize,
		Play
	}

	/**
	 * @brief FSequence is the main runtime class. It holds all the timelines and controls their behaviour.
	 * @sa FTimeline, FTrack, FEvent
	 */
	public class FSequence : FObject
	{
		public const int DEFAULT_FRAMES_PER_SECOND = 30;
		public const int DEFAULT_LENGTH = 10;

		public const float DEFAULT_SPEED = 1f;

        //public TrackAbleCache trackAbleCache = new TrackAbleCache();
        public static FSequence CreateSequence()
		{
			return CreateSequence( new GameObject("FSequence") );
		}

		public static FSequence CreateSequence( GameObject gameObject )
		{
			FSequence sequence = gameObject.AddComponent<FSequence>();

			sequence._content = new GameObject("SequenceContent").transform;
			//sequence._content.hideFlags |= HideFlags.HideInHierarchy;
			sequence._content.parent = sequence.transform;

			sequence.Add( FContainer.Create(FContainer.DEFAULT_COLOR) );

			sequence.Version = FUtility.FLUX_VERSION;

//			sequence.AddGlobalTimeline();

//			sequence.AddCommentTrack();
//			sequence.AddCommentTimeline();

			return sequence;
		}

		[SerializeField]
		private Transform _content = null;
		/// @brief Child Transform (hidden by default) that holds the containers.
		public Transform Content { get { return _content; } set { _content = value; _content.parent = transform; } }

		[SerializeField]
		private List<FContainer> _containers = new List<FContainer>();
		/// @brief Containers inside the sequence.
		public List<FContainer> Containers { get { return _containers; } }

//		[SerializeField]
//		[HideInInspector]
//		private FTimeline _commentTimeline = null;
//		public FTimeline CommentTimeline {
//			get {
//				if( _commentTimeline == null )
//					AddCommentTimeline();
//				return _commentTimeline;
//			}
//		}

//		[SerializeField]
//		[HideInInspector]
//		private FCommentTrack _commentTrack = null;
//		public FCommentTrack CommentTrack { 
//			get { 
//				if( _commentTrack == null ) 
//					AddCommentTrack(); 
//				return _commentTrack; 
//			}
//		}

		[SerializeField]
		[HideInInspector]
		private int _version = 0;
		public int Version { get { return _version; } set { _version = value; } }

		[SerializeField]
		[Tooltip("What should be the default action when Start() gets called?\n\tNone\n\tInitialize\n\tPlay")]
		private ActionOnStart _actionOnStart = ActionOnStart.None;
		/// @brief What should be the default action when Start() gets called?
		public ActionOnStart ActionOnStart { get{ return _actionOnStart; } set { _actionOnStart = value; } }

		[SerializeField]
		private bool _loop = false;
		/// @brief Does the sequence loop when it reaches the end?
		public bool Loop { get { return _loop; } set { _loop = value; } }

		[SerializeField]
		private float _defaultSpeed = DEFAULT_SPEED;
		/// @brief Speed that is used when sequence is loaded. When you want to change runtime see, use Speed instead
		public float DefaultSpeed { get { return _defaultSpeed; } set { _defaultSpeed = value; } }
		[SerializeField]
		private float _speed = DEFAULT_SPEED;
		/// @brief Current speed, used to control the speed of the sequence. If negative, sequence will play backwards.
		public float Speed { 
			get{ return _speed; } 
			set{ 
				bool wasPlayingForward = _isPlayingForward;
				_speed = value; 
				_isPlayingForward = Speed * Time.timeScale >= 0;
				// if we change the speed to playing back while it is playing already we need
				// to create the track caches to handle going backwards
				if( (wasPlayingForward != _isPlayingForward) && Application.isPlaying && IsInit ) 
					CreateTrackCaches();
			}
		}

		/// @brief Should it update on FixedUpdate? If false, it will update on Update.
		[SerializeField]
		private AnimatorUpdateMode _updateMode = AnimatorUpdateMode.Normal;
		public AnimatorUpdateMode UpdateMode { get { return _updateMode; } set { _updateMode = value; } }

		/// @brief The length of this sequence in frames.
		[SerializeField]
		private int _length = DEFAULT_LENGTH * DEFAULT_FRAMES_PER_SECOND;
		/// @brief Length of this sequence in frames.
		public int Length { get { return _length; } set { _length = value; } }
        [SerializeField]
        private string _skillId = "";
        public string SkillId { get { return _skillId; } set { _skillId = value; } }

		[SerializeField]
		private FSeqSetting _fSeqSetting;

		public FSeqSetting FSeqSetting { get { return _fSeqSetting; } }

		public Transform GetPlayerTF()
        {
			return this.Containers[0].Timelines[0].Owner;
        }

		/// @brief Returns the length of this sequence in seconds.
		/// @Warning This value isn't cached internally, so avoid calling it all the time.
		public float LengthTime { get { return (float)_length * _inverseFrameRate; } }

		[SerializeField]
		[HideInInspector]
		private SequenceFinishedEvent _onFinishedCallback = new SequenceFinishedEvent();
		/// @brief Callback when sequence reaches last frame (or frame 0 when moving backwards)
		public SequenceFinishedEvent OnFinishedCallback { get { return _onFinishedCallback; } }

//		private FTimeline _globalTimeline = null;
//		/// @brief Global Timeline. It is always visible, doesn't get processed at runtime, and used for things like comment track.
//		public FTimeline GlobalTimeline { get { return _globalTimeline; } }

        [SerializeField]
		[HideInInspector]
		private int _frameRate = DEFAULT_FRAMES_PER_SECOND; // frame rate

		/** @brief Frame Rate of this sequence.
		 * @Warning Although we allow you to change this value at runtime, you should be careful in how you use it. 
		 * Changing this value \b will not \b rescale the sequence. Rescaling the sequence should only be done in editor, 
		 * not at runtime.
		 * @sa CSequenceInspector.Rescale(CSequence, int)
		 * @sa InverseFrameRate
		 */
		public int FrameRate { get { return _frameRate; } set { _frameRate = value; _inverseFrameRate = 1f / _frameRate; } }

        [SerializeField]
		[HideInInspector]
        private float _inverseFrameRate = 1f / DEFAULT_FRAMES_PER_SECOND;

		/// @brief Returns 1 / FrameRate. This value is cached and set automatically when FrameRate is called.
		/// @sa FrameRate
		public float InverseFrameRate { get { return _inverseFrameRate; } }

		// has it been initialized?
		private bool _isInit = false;
		/// @brief Is the sequence initialized?
		public bool IsInit { get { return _isInit; } }

		// Is the sequence playing?
		private bool _isPlaying = false;
		/// @brief Is the sequence playing?
		public bool IsPlaying { get { return _isPlaying; } }

		// Is the sequence playing forward?
		private bool _isPlayingForward = true;
		/// @brief Is the sequence moving forward?
		public bool IsPlayingForward { get { return _isPlayingForward; } }

		// time we last updated
		private float _lastUpdateTime = 0;

		public override Transform Owner {
			get {
				return transform;
			}
		}

		public override FSequence Sequence {
			get {
				return this;
			}
		}

		public void Add( FContainer container )
		{
			int id = _containers.Count;
			_containers.Add( container );
			container.SetId( id );
			container.SetSequence( this );
		}

		public void Remove( FContainer container )
		{
			if( _containers.Remove( container ) )
			{
				container.SetSequence( null );

				UpdateContainerIds();
			}
		}

		// Current frame, i.e. (int)(_currentTime * frameRate)
		private int _currentFrame = -1;

		// Last frame, to determine if the frame has advanced or not
		private int _lastFrame = -1;

		// What's the current time
		private float _currentTime = -1;

		/** @brief Sets the current time, to be used when in editor mode, _not_ for runtime.
		 * @param time Time.
		 * @sa SetCurrentTime
		 */
		public void SetCurrentTimeEditor( float time )
		{
            if (time != _currentTime && _currentFrame != -1)
            {
                //	Speed = time > _currentTime ? Mathf.Abs(Speed) : -Mathf.Abs(Speed);
                //Debug.Log("yns Speed  " + Speed );
                //Debug.Log("yns speed Change");
            }

            CurrentTime = Mathf.Clamp( time, 0, LengthTime );

			for ( int i = 0; i != _containers.Count; ++i )
			{
				if( !_containers[i].enabled ) continue;
				_containers[i].UpdateTimelinesEditor( _currentFrame, _currentTime );
			}
		}


		/**
		 * @brief Sets the current time manually, e.g. if you want to jump to a specific point of the 
		 * sequence.
		 * @param time Time.
		 * @sa SetCurrentFrame, SetCurrentTimeEditor
		 */
		public void SetCurrentTime( float time )
		{
			if( !_isInit )
				Init();
			
			// if setting the time we're on, or we just started playing, ignore this
			if( time != _currentTime && _currentFrame != -1 )
			{
				// if we're changing time in a direction different from the one we're going,
				// we need to clear the tracks in order to be in the proper state when we move to 
				// a certain time
				if( ! (IsPlayingForward && time > _currentTime) )
				{
					for( int i = 0; i != _containers.Count; ++i )
						_containers[i].Stop();
				}
			}

			SetCurrentTimeInternal( time );
		}

		/** @brief Sets the current time based on the frame, but just calculates it internally for you.
		 * @param frame Frame.
		 * @sa SetCurrentTime
		 */
		public void SetCurrentFrame( int frame )
		{
			SetCurrentTime( frame * InverseFrameRate );
		}

		/// @brief Sets the current frame based on the time passed.
		/// @param time Time in seconds, this will set current frame with time x FrameRate.
		protected void SetCurrentTimeInternal( float time )
		{
			CurrentTime = Mathf.Clamp( time, 0, LengthTime );

			for( int i = 0; i != _containers.Count; ++i )
			{
				if( !_containers[i].enabled ) continue;
				_containers[i].UpdateTimelines( _currentFrame, _currentTime );
			}
		}

		/// @brief Returns the current frame.
		/// @sa SetCurrentFrame, GetCurrentFrame
		public int CurrentFrame {
			get {
				return _currentFrame;
			}
			private set {
				_lastFrame = value < 0 ? -1 : _currentFrame;
				_currentFrame = value;
			}
		}

		/** @brief Returns current time. */
		public float CurrentTime {
			get {
				return _currentTime; 
			}
			private set {
				_currentTime = value;
				if( _currentTime < 0 )
					CurrentFrame = -1;
				else
				{
					float epsilon = 0.001f;
					CurrentFrame = IsPlayingForward ? Mathf.FloorToInt(_currentTime * _frameRate + epsilon) : Mathf.CeilToInt(_currentTime*_frameRate - epsilon);
				}
			}
		}

		public bool FrameChanged { get { return _lastFrame != _currentFrame; } }

		/// @brief Initializes the sequence.
		/// This is called at the start of the sequence, it is meant for the
		/// user to setup all the cached variables.
		/// @note If you want to avoid frame drops, call this function before
		/// calling Play.
		public override void Init()
		{
#if FLUX_DEBUG
			Debug.Log("Init");
#endif

#if FLUX_PROFILE
			Profiler.BeginSample( "FSequence.Init()" );
#endif
			_isInit = true; // set init first so that cache doesn't go into infinite loop

			for( int i = 0; i != _containers.Count; ++i )
				_containers[i].Init();

			// build caches for tracks that require it at runtime
			CreateTrackCaches();

			_isInit = true; // force again init since create caches can clear it
#if FLUX_PROFILE
			Profiler.EndSample();
#endif
		}

		public void CreateTrackCaches()
		{
			for( int i = 0; i != _containers.Count; ++i )
			{
				List<FTimeline> timelines = _containers[i].Timelines;

				for( int j = 0; j != timelines.Count; ++j )
				{
					List<FTrack> tracks = timelines[j].Tracks;
					foreach( FTrack track in tracks )
					{
#if UNITY_EDITOR
						if( ((!Application.isPlaying && track.RequiresEditorCache) || (IsPlayingForward && track.RequiresForwardCache) || (!IsPlayingForward && track.RequiresBackwardsCache)) && !track.HasCache )
							track.CreateCache();
#else
						if( ((IsPlayingForward && track.RequiresForwardCache) || (!IsPlayingForward && track.RequiresBackwardsCache)) && !track.HasCache )
							track.CreateCache();
#endif
					}
				}
			}
		}

		public void GetAnimTrack(ref FAnimationTrack animationTrack)
		{
			foreach( FContainer container in Containers )
			{
				foreach( FTimeline timeline in container.Timelines )
				{
					foreach( FTrack track in timeline.Tracks )
					{
						if(track is FAnimationTrack)
                        {
							animationTrack = track as FAnimationTrack;
						}
					}
				}
			}
		}

		public void DestoryAllcache()
		{
			//foreach (FContainer container in Containers)
			//{
			//	foreach (FTimeline timeline in container.Timelines)
			//	{

			//		foreach (FTrack track in timeline.Tracks)
			//		{
			//			if(track is FTransformTrack)
   //                     {
			//				(track as FTransformTrack).ClearSnapshot();
			//			}

			//			//if (track.HasCache)
   //   //                  {
   //   //                      //Debug.Log($"yns  {track.Cache.IsBuilt}"); 
			//			//	track.ClearCache();
			//			//}

			//		}
			//	}
			//}
            Debug.Log($"yns  DestoryAllcache");
		}


		/// @override Starts playing on a specific frame.
		public void Play( int startFrame )
		{
			Play( startFrame * _inverseFrameRate );
		}

		/// @brief Starts playing at time.
		/// @param startTime What time to start playing from.
		/// @sa Init
		public void Play( float startTime )
		{
			if( _isPlaying )
				return;
			
			_isPlayingForward = Speed * Time.timeScale >= 0;
			
			if( !_isInit )
				Init();
			
			if( !IsStopped )
				Resume();
			
			_isPlaying = true;
			
			switch( _updateMode )
			{
			case AnimatorUpdateMode.Normal:
				_lastUpdateTime = Time.time;
				break;
			case AnimatorUpdateMode.AnimatePhysics:
				_lastUpdateTime = Time.fixedTime;
				break;
			case AnimatorUpdateMode.UnscaledTime:
				_lastUpdateTime = Time.unscaledTime;
				break;
			default:
				Debug.LogError("Unsupported Update Mode");
				_lastUpdateTime = Time.time;
				break;
			}
			
			SetCurrentTimeInternal( startTime );
		}

		/// @brief Starts playing from either the start or the end, depending on which 
		/// direction it is playing.
		/// @sa Init, Stop, Pause
		public void Play()
		{
			if( IsPlayingForward )
				Play( 0f );
			else
				Play( LengthTime );
		}

		/// @brief Stops sequence.
		public override void Stop()
		{
			Stop( false );
		}

		/** @brief Stops sequence.
		 * @param reset If true, it clears Init, i.e. it will force the Init phase to happen again.
		 */
		public void Stop( bool reset )
		{
			if( reset )
				_isInit = false;


			if( IsStopped && !reset )
				return;
			
#if FLUX_DEBUG
			Debug.Log ("Stop");
#endif
			_isPlaying = false;
			_isPlayingForward = Speed*Time.timeScale >= 0;

            for ( int i = 0; i != _containers.Count; ++i )
				_containers[i].Stop();

            CurrentTime = -1;
			_lastUpdateTime = 0;
		}

		/**
		 * @brief Pauses sequence.
		 * @sa FEvent.OnPause, FEvent.OnResume
		 */
		public void Pause()
		{
			if( !_isPlaying )
				return;
			_isPlaying = false;

			for( int i = 0; i != _containers.Count; ++i )
				_containers[i].Pause();
		}

		/**
		 * @brief Resumes a sequence that is paused.
		 * Doesn't work if the sequence is stopped.
		 * @sa Play, Stop, Pause
		 */
		public void Resume()
		{
			if( _isPlaying )
				return;

			_isPlaying = true;

			for( int i = 0; i !=_containers.Count; ++i )
				_containers[i].Resume();

			switch( _updateMode )
			{
			case AnimatorUpdateMode.Normal:
				_lastUpdateTime = Time.time;
				break;
			case AnimatorUpdateMode.AnimatePhysics:
				_lastUpdateTime = Time.fixedTime;
				break;
			case AnimatorUpdateMode.UnscaledTime:
				_lastUpdateTime = Time.unscaledTime;
				break;
			default:
				Debug.LogError("Unsupported Update Mode");
				_lastUpdateTime = Time.time;
				break;
			}
		}


		/// @brief Is the sequence paused?
		public bool IsPaused { get { return !_isPlaying && _currentFrame >= 0; } }

		/// @brief Is the sequence stopped?
		public bool IsStopped { get { return _currentFrame < 0; } }

		/// @brief Is the sequence finished? Does not finish when it loops.
		public bool IsFinished { get { return IsPaused && ((IsPlayingForward && _currentTime == LengthTime) || (!IsPlayingForward && _currentTime == 0)); } }

		/// @brief Does the sequence have no events?
		public bool IsEmpty()
		{
			foreach( FContainer container in _containers )
			{
				if( !container.IsEmpty() )
					return false;
			}

			return true;
		}

		/// @brief Determines wether it has any timelines.
		public bool HasTimelines()
		{
			foreach( FContainer container in _containers )
			{
				if( container.Timelines.Count > 0 )
					return true;
			}

			return false;
		}

		protected virtual void Awake()
		{
			Speed = DefaultSpeed;
			_isPlayingForward = Speed * Time.timeScale >= 0;
		}

		protected virtual void Start()
		{
			switch( _actionOnStart )
			{
			case ActionOnStart.None:
				// do nothing
				break;
			case ActionOnStart.Initialize:
				Init();
				break;
			case ActionOnStart.Play:
				Play();
				break;
			}
		}

		// Updates the sequence when update mode is NOT AnimatePhysics
		protected virtual void Update()
		{
			if( _updateMode == AnimatorUpdateMode.AnimatePhysics || !_isPlaying )
			{
				return;
			}

			InternalUpdate( _updateMode == AnimatorUpdateMode.Normal ? Time.time : Time.unscaledTime );
		}

		// Updates the sequence when update mode is AnimatePhysics
		protected virtual void FixedUpdate()
		{
			if( _updateMode != AnimatorUpdateMode.AnimatePhysics || !_isPlaying )
			{
				return;
			}

			InternalUpdate( Time.fixedTime );
		}

		// Internal update function, i.e. does the actual update of the sequence
		protected virtual void InternalUpdate( float time )
		{
//			float delta = time - _lastUpdateTime;
//			float timePerFrame = InverseFrameRate;
//			if( delta >= timePerFrame )
//			{
//				int numFrames = Mathf.RoundToInt(delta / timePerFrame);
//				SetCurrentFrame( _currentFrame + numFrames );
//				_lastUpdateTime = time - (delta - (timePerFrame * numFrames));
//			}

			float delta = time - _lastUpdateTime;

			if( delta != 0 )
			{
				SetCurrentTimeInternal( _currentTime + delta * Speed );

				/// @TODO take into account the "time lost" with clamping
				if( _isPlayingForward )
				{
					if( _currentTime == LengthTime )
					{
						OnFinishedCallback.Invoke( this );

						if( _loop ) 
						{
							Stop();
							Play();
						}
						else 
						{
							Pause();
						}
					}
				}
				else
				{
					if( _currentTime == 0 )
					{
						OnFinishedCallback.Invoke( this );

						if( _loop )
						{
							Stop();
							Play(); // @TODO Play backwards
						}
						else
						{
							Pause();
						}
					}
				}

				_lastUpdateTime = time;
			}
		}

		/// @brief Rebuilds the sequence. This is to be called whenever timelines,
		/// tracks, or events are added / removed from the sequence.
		/// @note You should only call this in editor mode, avoid calling it at runtime.
		public void Rebuild()
		{
#if FLUX_DEBUG
			Debug.Log("Rebuilding");
#endif
//			Transform t = TimelineContainer;
//			_timelines.Clear();
//
//			for( int i = 0; i != t.childCount; ++i )
//			{
//				FTimeline timeline = t.GetChild(i).GetComponent<FTimeline>();
//
//				if( timeline )
//				{
//					if( timeline.IsGlobal )
//					{
//						_globalTimeline = timeline;
//					}
//					else
//					{
//						_timelines.Add( timeline );
//						timeline.SetSequence( this );
//						timeline.Rebuild();
//					}
//				}
//			}

			_containers.Clear();

			Transform t = Content;

			for( int i = 0; i != t.childCount; ++i )
			{
				FContainer container = t.GetChild( i ).GetComponent<FContainer>();

				if( container != null )
				{
					_containers.Add( container );
					container.SetSequence( this );
					container.Rebuild();
				}
			}

			// fix sibling indexes
//			for( int i = 0; i != _containers.Count; ++i )
//			{
//				Transform containerTransform = _containers[i].transform;
//				if( containerTransform.GetSiblingIndex() != i )
//					containerTransform.SetSiblingIndex( i );
//			}

			UpdateContainerIds();

//			if( _globalTimeline == null )
//			{
//				AddGlobalTimeline();
//			}

//			_globalTimeline.Rebuild();

//			UpdateTimelineIds();
		}

		private void UpdateContainerIds()
		{
			for( int i = 0; i != _containers.Count; ++i )
			{
				_containers[i].SetId( i );
			}
		}

		public void ReplaceOwner( Transform oldOwner, Transform newOwner )
		{
			foreach( FContainer container in _containers )
			{
				foreach( FTimeline timeline in container.Timelines )
				{
					if( timeline.Owner == oldOwner )
						timeline.SetOwner( newOwner );
				}
			}
		}

		public void ReplaceOwner( string timelineName, Transform newOwner )
		{
			foreach( FContainer container in _containers )
			{
				foreach( FTimeline timeline in container.Timelines )
				{
					if( timeline.gameObject.name == timelineName )
						timeline.SetOwner( newOwner );
				}
			}
		}

		public void ReplaceOwnerByPath( string ownerPath, Transform newOwner )
		{
			foreach( FContainer container in _containers )
			{
				foreach( FTimeline timeline in container.Timelines )
				{
					if( timeline.OwnerPath == ownerPath )
						timeline.SetOwner( newOwner );
				}
			}
		}

		public void ReplaceOwner( params KeyValuePair<Transform, Transform>[] replacements )
		{
			foreach( FContainer container in _containers )
			{
				foreach( FTimeline timeline in container.Timelines )
				{
					foreach( KeyValuePair<Transform, Transform> replacement in replacements )
					{
						if( timeline.Owner == replacement.Key )
						{
							timeline.SetOwner( replacement.Value );
							break;
						}
					}
				}
			}
		}

#if FLUX_TRIAL

		private Rect _watermarkLabelRect = new Rect(0,0,0,0);
		private GUIStyle _watermarkLabelStyle;
		private float _watermarkEndTime;
		private float _watermarkAlpha;

		private void OnGUI()
		{
			if( !IsPlaying )
				return;

			float watermarkDuration = 3f;

			GUIContent watermark = new GUIContent("..::FLUX TRIAL::..");

			if( _watermarkLabelRect.width == 0 )
			{
				_watermarkLabelStyle = new GUIStyle(GUI.skin.label);
				_watermarkLabelStyle.fontSize = 24;

				Vector2 size = _watermarkLabelStyle.CalcSize( watermark );

				_watermarkLabelRect.x = Random.Range(0, Screen.width-size.x);
				_watermarkLabelRect.y = Random.Range(0, 2);
				if( _watermarkLabelRect.y == 1 )
					_watermarkLabelRect.y = Screen.height-size.y;
				_watermarkLabelRect.width = size.x;
				_watermarkLabelRect.height = size.y;

				_watermarkEndTime = Time.time + watermarkDuration;
				_watermarkAlpha = 1.6f;
			}
			GUI.color = new Color(1f, 1f, 1f, _watermarkAlpha + 0.4f );
			GUI.Label( _watermarkLabelRect, watermark, _watermarkLabelStyle );
			if( Time.time < _watermarkEndTime )
			{
				_watermarkAlpha -= Time.deltaTime / watermarkDuration;
				if( _watermarkAlpha < 0 )
					_watermarkAlpha = 0;
			}
		}
#endif
	}
	
	[System.Serializable]
	public class SequenceFinishedEvent : UnityEvent<FSequence>
	{
	}
}

