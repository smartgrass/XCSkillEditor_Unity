using UnityEngine;
using System.Collections.Generic;

namespace Flux
{	
	public class FRendererTrack : FTrack {

		private static Dictionary<int,MaterialPropertyBlockInfo> _materialPropertyBlocks = null;

		private static MaterialPropertyBlockInfo GetMaterialPropertyBlockInfo( int objInstanceId )
		{
			if( _materialPropertyBlocks == null )
				_materialPropertyBlocks = new Dictionary<int, MaterialPropertyBlockInfo>();

			MaterialPropertyBlockInfo matPropertyBlockInfo = null;
			
			if( _materialPropertyBlocks.TryGetValue( objInstanceId, out matPropertyBlockInfo ) )
				return matPropertyBlockInfo;
			
			matPropertyBlockInfo = new MaterialPropertyBlockInfo();
			
			_materialPropertyBlocks.Add( objInstanceId, matPropertyBlockInfo );
			
			return matPropertyBlockInfo;
		}

		private class MaterialPropertyBlockInfo
		{
			public MaterialPropertyBlock _materialPropertyBlock = new MaterialPropertyBlock();
			public int _frameGotCleared = 0;

			public void Clear( int frame )
			{
				_materialPropertyBlock.Clear();
				_frameGotCleared = frame;
			}
		}

		private MaterialPropertyBlockInfo _matPropertyBlockInfo = null;
		public MaterialPropertyBlock GetMaterialPropertyBlock() { return _matPropertyBlockInfo != null ? _matPropertyBlockInfo._materialPropertyBlock : null; }

		private Renderer _renderer = null;
		public Renderer Renderer { get { return _renderer; } }

		public override void Init()
		{
			_renderer = Owner != null ? Owner.GetComponent<Renderer>() : null;

			if( !(_renderer is SpriteRenderer) )
				_matPropertyBlockInfo = GetMaterialPropertyBlockInfo( Owner != null ? Owner.GetInstanceID() : -1 );

			base.Init();
		}

		public override void UpdateEvents (int frame, float time)
		{
			if( _matPropertyBlockInfo != null && _matPropertyBlockInfo._frameGotCleared != frame )
				_matPropertyBlockInfo.Clear( frame );
			base.UpdateEvents(frame, time);
			if( _matPropertyBlockInfo != null )
				_renderer.SetPropertyBlock( _matPropertyBlockInfo._materialPropertyBlock );
		}

		public override void UpdateEventsEditor (int currentFrame, float currentTime)
		{
			if( _matPropertyBlockInfo != null && _matPropertyBlockInfo._frameGotCleared != currentFrame )
				_matPropertyBlockInfo.Clear( currentFrame );
			base.UpdateEventsEditor (currentFrame, currentTime);
			if( _matPropertyBlockInfo != null )
				_renderer.SetPropertyBlock( _matPropertyBlockInfo._materialPropertyBlock );
		}

		public override void Stop ()
		{
			base.Stop();
			if( _matPropertyBlockInfo != null )
			{
//				Init();
				_matPropertyBlockInfo.Clear( _matPropertyBlockInfo._frameGotCleared );

				if( _renderer != null )
					_renderer.SetPropertyBlock( _matPropertyBlockInfo._materialPropertyBlock );
			}
		}
	}
}
