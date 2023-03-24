using UnityEngine;
using System.Collections.Generic;

namespace Flux
{
	public class FContainer : FObject {

		public static readonly Color DEFAULT_COLOR = new Color(0.14f, 0.14f, 0.14f, 0.7f);

		[SerializeField]
		private FSequence _sequence = null;

        [SerializeField]
		private Color _color;
		public Color Color { get { return _color; } set { _color = value; } }

		[SerializeField]
		private List<FTimeline> _timelines = new List<FTimeline>();
		public List<FTimeline> Timelines { get { return _timelines; } }

		public override FSequence Sequence { get { return _sequence; } }
		public override Transform Owner { get { return null; } }

		public static FContainer Create( Color color )
		{
			GameObject go = new GameObject("Default");
			FContainer container = go.AddComponent<FContainer>();
			container.Color = color;

			return container;
		}

		internal void SetSequence( FSequence sequence )
		{
			_sequence = sequence;
			if( _sequence )
				transform.parent = _sequence.Content;
			else
				transform.parent = null;
		}

		public override void Init()
		{
			foreach( FTimeline timeline in _timelines )
			{
				timeline.Init();
			}
		}

		public override void Stop()
        {

            foreach ( FTimeline timeline in _timelines )
			{
				timeline.Stop();
			}
		}

		public void Resume()
		{
			foreach( FTimeline timeline in _timelines )
			{
				timeline.Resume();
			}
		}

		public void Pause()
		{
			foreach( FTimeline timeline in _timelines )
			{
				timeline.Pause();
			}
		}

		public bool IsEmpty()
		{
			foreach( FTimeline timeline in _timelines )
			{
				if( !timeline.IsEmpty() )
				{
					return false;
				}
			}

			return true;
		}

		public void UpdateTimelines( int frame, float time )
		{
			for( int i = 0; i != _timelines.Count; ++i )
			{
				if( !_timelines[i].enabled ) continue;
				_timelines[i].UpdateTracks( frame, time );
			}
		}

		public void UpdateTimelinesEditor( int frame, float time )
		{
			for( int i = 0; i != _timelines.Count; ++i )
			{
				if( !_timelines[i].enabled ) continue;
				_timelines[i].UpdateTracksEditor( frame, time );
			}
		}
			
		/// @brief Adds new timeline at the end of the list.
		/// @param timeline New timeline.
		public void Add( FTimeline timeline )
		{
			int id = _timelines.Count;
			
			_timelines.Add( timeline );
			timeline.SetId( id );
			
//			timeline.SetSequence( this );
			timeline.SetContainer( this );
		}
		
		/// @brief Removes timeline and updates their ids.
		/// @param timeline CTimeline to remove.
		/// @note After calling this function, the ids of the timelines after this
		/// one in the list will have an id smaller by 1.
		public void Remove( FTimeline timeline )
		{
			for( int i = 0; i != _timelines.Count; ++i )
			{
				if( _timelines[i] == timeline )
				{
					Remove( i );
					break;
				}
			}
		}
		
		/// @brief Removes timeline with id.
		/// @oaram id Id of the CTimeline to remove.
		/// @note After calling this function, the ids of the timelines after this
		/// one in the list will have an id smaller by 1.
		/// @warning Does not check if id is valid (i.e. between -1 & GetTimelines().Count)
		public void Remove( int id )
		{
			FTimeline timeline = _timelines[id];
			_timelines.RemoveAt( id );
			timeline.SetContainer( null );
			
			UpdateTimelineIds();
		}

		public void Rebuild()
		{
			_timelines.Clear();
			Transform t = transform;
			for( int i = 0; i != t.childCount; ++i )
			{
				FTimeline timeline = t.GetChild(i).GetComponent<FTimeline>();
				if( timeline != null )
				{
					_timelines.Add( timeline );

					timeline.SetContainer( this );
					timeline.Rebuild();
				}
			}

			UpdateTimelineIds();
		}

		// Updates the ids of the timelines
		private void UpdateTimelineIds()
		{
			for( int i = 0; i != _timelines.Count; ++i )
			{
				_timelines[i].SetId( i );
			}
		}
	}
}
