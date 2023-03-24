using UnityEngine;
using System;
using System.Collections.Generic;

namespace Flux
{
	[Serializable]
	/**
	 * @brief Range of frames.
	 * @note Start is _not_ guaranteed to be smaller or equal to End, it is up to the user to make sure.
	 */
	public struct FrameRange
	{
		// start frame
		[SerializeField]
		private int _start;

		// end frame
		[SerializeField]
		private int _end;

		/// @brief Returns the start frame.
		public int Start
		{ 
			get { return _start; } 
			set 
			{ 
				_start = value;
			}
		}

		/// @brief Returns the end frame.
		public int End
		{ 
			get { return _end; } 
			set 
			{ 
				_end = value;
			}
		}

		/// @brief Sets / Gets the length.
		/// @note It doesn't cache the value.
		public int Length { set{ End = _start + value; } get{ return _end - _start; } }

		/**
		 * @brief Create a frame range
		 * @param start Start frame
		 * @param end End frame
		 * @note It is up to you to make sure start is smaller than end.
		 */
		public FrameRange( int start, int end )
		{
			this._start = start;
			this._end = end;
		}

		/// @brief Returns \e i clamped to the range.
		public int Cull( int i )
		{
			return Mathf.Clamp( i, _start, _end );
		}

		/// @brief Returns if \e i is inside [start, end], i.e. including borders
		public bool Contains( int i )
		{
			return i >= _start && i <= _end;
		}

		/// @brief Returns if \e i is inside ]start, end[, i.e. excluding borders
		public bool ContainsExclusive( int i )
		{
			return i > _start && i < _end;
		}

		/// @brief Returns if the ranges intersect, i.e. touching returns false
		/// @note Assumes They are both valid
		public bool Collides( FrameRange range )
		{
			return _start < range._end && _end > range._start;
//			return (range.start > start && range.start < end) || (range.end > start && range.end < end );
		}

		/// @brief Returns if the ranges overlap, i.e. touching return true
		/// @note Assumes They are both valid
		public bool Overlaps( FrameRange range )
		{
			return range.End >= _start && range.Start <= _end;
		}

		/// @brief Returns what kind of overlap it has with \e range.
		/// @note Assumes They are both valid
		public FrameRangeOverlap GetOverlap( FrameRange range )
		{
			if( range._start >= _start )
			{
				// contains, left or none
				if( range._end <= _end )
				{
					return FrameRangeOverlap.ContainsFull;
				}
				else
				{
					if( range._start > _end )
					{
						return FrameRangeOverlap.MissOnRight;
					}
					return FrameRangeOverlap.ContainsStart;
				}
			}
			else
			{
				// contained, right or none
				if( range._end < _start )
				{
					return FrameRangeOverlap.MissOnLeft;
				}
				else
				{
					if( range._end > _end )
					{
						return FrameRangeOverlap.IsContained;
					}

					return FrameRangeOverlap.ContainsEnd;
				}
			}
		}

        public static bool operator ==( FrameRange a, FrameRange b )
        {
            return a._start == b._start && a._end == b._end;
        }

        public static bool operator !=( FrameRange a, FrameRange b )
        {
            return !(a == b);
        }

		public override bool Equals( object obj )
		{
			if( obj.GetType() != GetType() )
				return false;

			return (FrameRange)obj == this;
		}

		public override int GetHashCode()
		{
			return _start + _end;
		}

		public override string ToString()
		{
			return string.Format("[{0}; {1}]", _start, _end);
		}
	}

	/// @brief Types of range overlap
	public enum FrameRangeOverlap
	{
		MissOnLeft = -2,	/// @brief missed and is to the left of the range passed
		MissOnRight,		/// @brief missed and is to the right of the range passed
		IsContained,		/// @brief overlaps and is contained by the range passed
		ContainsFull,		/// @brief overlaps and contains the range passed
		ContainsStart,		/// @brief overlaps and contains the start of the range passed
		ContainsEnd			/// @brief overlaps and contains the end of the range passed
	}

	/**
	 * @brief Base class for Events
	 * @sa FSequence, FTimeline, FTrack.
	 */
	public class FEvent : FObject
	{
		public override Transform Owner { get { return _track.Owner; } }

		public override FSequence Sequence { get { return _track.Sequence; } }

        public bool EnableEvent = true;

		//isLocalTrueOnly
		public bool isLocalTrueOnly;

		// track that owns this event
		[SerializeField]
		protected FTrack _track = null;
        /// @brief Returns the track it belongs to

        [HideInInspector]
        public FTrack Track { get { return _track; } }

		[SerializeField]
		[HideInInspector]
		private bool _triggerOnSkip = true;
		/// @brief Should this event trigger if you skip it?
		public bool TriggerOnSkip { get { return _triggerOnSkip; } set { _triggerOnSkip = value; } }

        [SerializeField]
        [HideInInspector]
        private FrameRange _frameRange;
		/// @brief Range of the event.
		public FrameRange FrameRange { get { return _frameRange; } 
			set { 
				FrameRange oldFrameRange = _frameRange;
				_frameRange = value; OnFrameRangeChanged( oldFrameRange ); 
			} 
		}

		// has this event called Trigger already?
		private bool _hasTriggered = false;
		/// @brief Has Trigger been called already?
		public bool HasTriggered { get { return _hasTriggered; } }

		// has this event called Finish already?
		private bool _hasFinished = false;
		/// @brief Has Finish been called already?
		public bool HasFinished { get { return _hasFinished; } }

		public virtual string Text { get { return null; } set { } }

		/**
		 * @brief Create an event. Should be used to create events since it also 
		 * calls SetDefaultValues.
		 * @param range Range of the event.
		 */
		public static T Create<T>( FrameRange range ) where T : FEvent
		{
			GameObject go = new GameObject( typeof(T).ToString() );

			T evt = go.AddComponent<T>();

			evt._frameRange = new FrameRange(range.Start, range.End );

			evt.SetDefaultValues();

			return evt;
		}

		/// @overload
		public static FEvent Create( Type evtType, FrameRange range )
		{
			GameObject go = new GameObject( evtType.ToString() );
			
			FEvent evt = (FEvent)go.AddComponent(evtType);
			
			evt._frameRange = new FrameRange( range.Start, range.End );

			evt.SetDefaultValues();
	
			return evt;
		}

		// sets the track this event belongs to, to be called only by FTrack
		internal void SetTrack( FTrack track )
		{
			_track = track;
			if( _track )
			{
				transform.parent = _track.transform;
			}
			else
			{
				transform.parent = null;
			}
		}

		/// @brief Use this function to setup default values for when events get created
		protected virtual void SetDefaultValues()
		{
		}

		/// @brief Use this function if you want to do something to the event when the frame range
		/// changed, e.g. adjust some variables to the new event size.
		/// @param oldFrameRange Previous FrameRange, the current one is set on the event.
		protected virtual void OnFrameRangeChanged( FrameRange oldFrameRange )
		{
		}

		/**
		 * @brief Called when the event gets reached.
		 * The reason we pass the time is because they may have been frames skipped
		 * or simply we may have jumped into the middle of an event, and that allows you 
		 * to skip to the right point. E.g. useful when you want to play an animation, 
		 * if you jumped to the middle of it you want to tell mecanim to start in the middle,
		 * not at the start.
		 * @param frameSinceTrigger Frames that passed since the actual TriggerFrame
		 * @param timeSinceTrigger Time passed since the actual TriggerFrame
		 * @sa TriggerFrame, TriggerTime, Finish
		 */
		public void Trigger( float timeSinceTrigger )
		{
			_hasTriggered = true;

			OnTrigger( timeSinceTrigger );
		}

		/// @brief At which frame will the event trigger, basically the start of it's range.
		public int TriggerFrame { get { return _frameRange.Start; } }
		/// @brief At which time the event triggers.
		/// @note Value isn't cached.
		public float TriggerTime { get { return _frameRange.Start * Sequence.InverseFrameRate; } }

		/**
		 * @brief Used to setup your own code when Trigger is called.
		 * @param framesSinceTrigger Frames passed since TriggerFrame
		 * @param timeSinceTrigger Time passed since timeSinceTrigger
		 */
		protected virtual void OnTrigger( float timeSinceTrigger ){ }

		/**
		 * @brief Called when the event ends, i.e. we pass the end of it's range.
		 * @sa Trigger
		 */
		public void Finish()
		{
			_hasFinished = true;

#if UNITY_EDITOR
			PreEvent();
#endif
			if( Sequence.IsPlayingForward )
				OnFinish(); // only do this code if we're moving forward, otherwise it doesn't really matter

#if UNITY_EDITOR
			PostEvent();
#endif
		}

		/// @brief Used to setup your own code when Finish is called.
		protected virtual void OnFinish(){ }

		public sealed override void Init()
		{
			_hasTriggered = false;
			_hasFinished = false;

			// init doesn't get wrapped between Pre/PostEvent because 
			// it is here that vars will be initialized
			OnInit();
		}

		/// @brief Used to setup your own code for when the sequence is initialized
		protected virtual void OnInit() { }

		public void Pause()
		{
#if UNITY_EDITOR
			PreEvent();
#endif

			OnPause();

#if UNITY_EDITOR
			PostEvent();
#endif
		}

		/// @brief Used to setup your own code for when the sequence is paused
		protected virtual void OnPause() { }

		public void Resume()
		{
#if UNITY_EDITOR
			PreEvent();
#endif
			
			OnResume();

#if UNITY_EDITOR
			PostEvent();
#endif
		}

		/// @brief Used to setup your own code for when the sequence is resumed
		protected virtual void OnResume() { }

		public sealed override void Stop()
		{
			_hasTriggered = false;
			_hasFinished = false;

#if UNITY_EDITOR
			PreEvent();
#endif

			OnStop();

#if UNITY_EDITOR
			PostEvent();
#endif
		}

		/// @brief Used to setup your own code for when the sequence is stopped
		protected virtual void OnStop(){}
	
		/**
		 * @brief Called each time the sequence gets updated, if the current frame is in this event's range.
		 * @param framesSinceTrigger How many frames have passed since TriggerFrame
		 * @param timeSinceTrigger How much time passed since TriggerFrame
		 */
        public void UpdateEvent( int framesSinceTrigger, float timeSinceTrigger )
		{
#if UNITY_EDITOR
			PreEvent();
#endif

			if( !_hasTriggered )
			{
				Trigger( timeSinceTrigger );
			}
			//Debug.Log($"yns  {framesSinceTrigger}, {timeSinceTrigger}");
			OnUpdateEvent( timeSinceTrigger );

			if( framesSinceTrigger == Length ) 
			{
				Finish();
			}

#if UNITY_EDITOR
			PostEvent();
#endif
		}


		/**
		 * @brief Used to setup your code that gets called when the event updates.
		 * @param framesSinceTrigger How many frames passed since TriggerFrame
		 * @param timeSinceTrigger How much time passed since TriggerFrame
		 */
		protected virtual void OnUpdateEvent( float timeSinceTrigger )
		{

		}
			
		/**
		 * @brief Used to mark objects used as not to be saved, in order to not make the scene dirty when 
		 * scrubbing the editor.
		 * @note This is called before every call to FEvent, i.e. Trigger, UpdateEvent, Stop, etc.
		 */
		protected virtual void PreEvent()
		{
#if UNITY_EDITOR
			if( !Application.isPlaying )
				Owner.gameObject.hideFlags = HideFlags.DontSave;
#endif
		}

		/**
		 * @brief Used to mark objects used as to be saved again.
		 * @note This is called after every call to FEvent, i.e. Trigger, UpdateEvent, Stop, etc.
		 */
		protected virtual void PostEvent()
		{
#if UNITY_EDITOR
			if( !Application.isPlaying )
				Owner.gameObject.hideFlags = HideFlags.None;
#endif
		}

		/// @brief Returns \e true if it is the first event of the track it belongs to.
		public bool IsFirstEvent { get { return GetId() == 0; } }

		/// @brief Returns \e true if it is the last event of the track it belongs to.
		public bool IsLastEvent { get { return GetId() == _track.Events.Count-1; } }

		/// @brief Shortcut to FrameRange.Start
		public int Start
		{
			get{ return _frameRange.Start; }
			set{ _frameRange.Start = value; }
		}

		/// @brief Shortcut to FrameRange.End
		public int End
		{
			get { return _frameRange.End; }
			set{ _frameRange.End = value; }
		}

		/// @brief Shortcut to FrameRange.Length
		public int Length
		{
			get{ return _frameRange.Length; } 
			set{ _frameRange.Length = value; }
		}

		/// @brief What this the event starts.
		/// @note This value isn't cached.
        public float StartTime
        {
            get { return _frameRange.Start * Sequence.InverseFrameRate; }
        }

		/// @brief What this the event ends.
		/// @note This value isn't cached.
        public float EndTime
        {
            get { return _frameRange.End * Sequence.InverseFrameRate; }
        }

		/// @brief Length of the event in seconds.
		/// @note This value isn't cached.
        public float LengthTime
        {
            get { return _frameRange.Length * Sequence.InverseFrameRate; }
        }

		/// @brief What's the minimum length this event can have?
		/// @warning Events cannot be smaller than 1 frame.
		public virtual int GetMinLength()
		{
			return 1;
		}

		/// @brief What's the maximum length this event can have?
		public virtual int GetMaxLength()
		{
			return int.MaxValue;
		}

		/// @brief Does the Event collides the \e e?
		public bool Collides( FEvent e )
		{
			return _frameRange.Collides( e.FrameRange );
		}

		/// @brief Returns the biggest frame range this event can have
		public FrameRange GetMaxFrameRange()
		{
			FrameRange range = new FrameRange(0, 0);

			int id = GetId();

			if( id == 0 )
			{
				range.Start = 0;
			}
			else
			{
				range.Start = _track.Events[id-1].End;
			}

			if( id == _track.Events.Count-1 ) // last one?
			{
				range.End = _track.Timeline.Sequence.Length;
			}
			else
			{
				range.End = _track.Events[id+1].Start;
			}

			return range;
		}

		/// @brief Compares events based on their start frame, basically used to order them.
		/// @param e1 Event
		/// @param e2 Event
		public static int Compare( FEvent e1, FEvent e2 )
		{
			return e1.Start.CompareTo( e2.Start );
		}
	}

	/**
	 * @brief Attribute that adds an Event to the add event menu.
	 */
	public class FEventAttribute : System.Attribute
	{
		// menu path
		public string menu;

		// type of track to be used
		public Type trackType;

//		public object _color = null;


		public FEventAttribute( string menu )
			:this( menu, typeof(FTrack) )
		{
		}

		public FEventAttribute( string menu, Type trackType )
		{
			this.menu = menu;
			this.trackType = trackType;
		}

//		public FEventAttribute( string menu, Color color )
//			:this( menu )
//		{
//			_color = color;
//		}
	}
}
