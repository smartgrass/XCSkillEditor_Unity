using UnityEngine;
using System.Collections.Generic;

namespace Flux
{
	//[FEvent("Renderer/Tween Color", typeof(FRendererTrack))]
	public class FRendererColorEvent : FRendererEvent
	{
		[SerializeField]
		private FTweenColor _tween = null;

		private Renderer _renderer = null;

		private Color _spriteRendererColor;

		protected override void OnInit ()
		{
			base.OnInit ();
			_renderer = ((FRendererTrack)_track).Renderer;

			if( _renderer is SpriteRenderer )
				_spriteRendererColor = ((SpriteRenderer)_renderer).color;
		}

		protected override void SetDefaultValues()
		{
			if( PropertyName == null )
				PropertyName = "_Color";
			
			if( _tween == null )
				_tween = new FTweenColor( new Color(1f, 1f, 1f, 0f), Color.white );
		}

		protected override void ApplyProperty( float t )
		{
			if( _renderer )
			{
				Color color = _tween.GetValue( t );

				if( _renderer is SpriteRenderer )
				{
					((SpriteRenderer)_renderer).color = color;
				}
				else if( _renderer is ParticleSystemRenderer )
				{
					_renderer.sharedMaterial.SetColor( PropertyName, color );
				}
				else
				{
    	           _matPropertyBlock.SetColor(PropertyName, color);
	            }
			}
		}

		protected override void OnStop ()
		{
			if( _renderer is SpriteRenderer )
			{
				((SpriteRenderer)_renderer).color = _spriteRendererColor;
			}
			else
				ApplyProperty( 0f );
		}
	}
}
