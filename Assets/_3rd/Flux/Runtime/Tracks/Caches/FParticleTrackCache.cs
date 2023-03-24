using UnityEngine;
using System.Collections.Generic;

namespace Flux
{
	public class FParticleTrackCache : FTrackCache {

		private List<KeyValuePair<int,ParticleSystem.Particle[]>> _particles = new List<KeyValuePair<int,ParticleSystem.Particle[]>>();

		public FParticleTrackCache(FTrack track)
			:base(track)
		{
		}

		protected override bool BuildInternal()
		{
			FSequence sequence = Track.Sequence;

			FParticleTrack particleTrack = (FParticleTrack)Track;

			float currentTime = sequence.CurrentTime;

			if( currentTime >= 0 )
			{
				sequence.Stop();
			}

			_particles.Clear();

			for( int frame = 0, numFrames = sequence.Length+1; frame != numFrames; ++frame )
			{
				particleTrack.UpdateEvents(frame, frame*particleTrack.Sequence.InverseFrameRate);

				ParticleSystem.Particle[] particleBuffer = new ParticleSystem.Particle[particleTrack.ParticleSystem.main.maxParticles];

				KeyValuePair<int, ParticleSystem.Particle[]> entry = new KeyValuePair<int, ParticleSystem.Particle[]>( particleTrack.ParticleSystem.GetParticles( particleBuffer ), particleBuffer );

				_particles.Add( entry );
			}

			sequence.Stop();

			if( currentTime >= 0 )
			{
				sequence.SetCurrentTime( currentTime );
			}

			return true;
		}

		protected override bool ClearInternal()
		{
            _particles.Clear();
			return true;
		}

		public override void GetPlaybackAt(float sequenceTime)
		{
			FParticleTrack particleTrack = (FParticleTrack)Track;
			int frame = (int)(sequenceTime*particleTrack.Sequence.FrameRate);
			particleTrack.ParticleSystem.SetParticles( _particles[frame].Value, _particles[frame].Key );
		}
	}
}
