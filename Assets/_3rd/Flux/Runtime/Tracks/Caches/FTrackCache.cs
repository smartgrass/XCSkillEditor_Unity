using UnityEngine;

namespace Flux
{
	[System.Flags]
	public enum FTrackCacheType
	{
		None 	= 0,	// doesn't use cache, always evaluates
		Editor	= 1,	// uses cache when in editor scrubbing
		Runtime	= 2		// uses cache when in runtime
	}

	/// @brief Base for FTrack caching.
	public abstract class FTrackCache {

		private FTrack _track = null;
		/// @brief Track it's caching
		public FTrack Track { get { return _track; } protected set { _track = value; } }

		private bool _isBuilt = false;
		/// @brief Is the cache already built?
		public bool IsBuilt { get { return _isBuilt; } }

		public FTrackCache( FTrack track )
		{
			_track = track;
		}

		/**
		 * @brief Build cache
		 * @param rebuild Rebuild it if it already exists
		 */
		public void Build( bool rebuild )
		{
			if( IsBuilt )
			{
				if( !rebuild )
					return;
				Clear();
			}

			_isBuilt =  BuildInternal();
		}

		/// @override 
		public void Build()
		{
			Build( true );
		}

		/// @brief Handles the actual building of the cache
		protected abstract bool BuildInternal();

		/// @brief Clears the cache
		public void Clear()
		{
			if( !IsBuilt )
				return;
			_isBuilt = !ClearInternal();
		}

		/// @brief Handles the actual clearing of the cache
		protected abstract bool ClearInternal();

		/**
		 * @brief Used to playback the cached data
		 * @param sequenceTime Sequence time we're playing
		 */
		public abstract void GetPlaybackAt( float sequenceTime );
	}
}
