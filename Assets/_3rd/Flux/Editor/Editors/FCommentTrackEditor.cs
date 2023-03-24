using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.AnimatedValues;
using Flux;

namespace FluxEditor
{
	[FEditor(typeof(FCommentTrack))]
	public class FCommentTrackEditor : FTrackEditor
	{
		#region Texture Handling
		public const int INITIAL_TEXTURE_POOL_SIZE = 64;

		public const int TEXTURE_WIDTH = 160;
		public const int TEXTURE_HEIGHT = 100;
		public const int TEXTURE_DEPTH = 0;

		private static Stack<RenderTexture> __texturePool = null;
		public static Stack<RenderTexture> TexturePool 
		{
			get
			{
				if( __texturePool == null )
				{
					__texturePool = new Stack<RenderTexture>(INITIAL_TEXTURE_POOL_SIZE);
					for( int i = 0; i != INITIAL_TEXTURE_POOL_SIZE; ++i )
					{
						__texturePool.Push(CreateTexture());
					}
				}
				return __texturePool;
			}
		}

		public static RenderTexture RequestTexture()
		{
			if( TexturePool.Count == 0 )
			{
				for( int i = 0; i != INITIAL_TEXTURE_POOL_SIZE; ++i )
				{
					__texturePool.Push(CreateTexture());
				}
			}
			
			return TexturePool.Pop();
		}

		public static void ReleaseTexture( RenderTexture texture )
		{
			TexturePool.Push( texture );
		}

		private static RenderTexture CreateTexture()
		{
			RenderTexture texture = new RenderTexture(TEXTURE_WIDTH, TEXTURE_HEIGHT, 32, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
			texture.hideFlags = HideFlags.DontSave;
			return texture;
		}
		#endregion Texture Handling

		public override float Height { get { return Mathf.Lerp( DEFAULT_TRACK_HEIGHT, TEXTURE_HEIGHT, ShowTextures.faded ); } }

		public AnimBool ShowTextures { get; private set; }

		protected override void RenderHeader( Rect labelRect, GUIContent label )
		{
			if( Event.current.type == EventType.MouseDown && Event.current.clickCount > 1 && labelRect.Contains(Event.current.mousePosition) )
			{
				ShowTextures.target = !ShowTextures.target;
			}
			
			base.RenderHeader( labelRect, label );
		}

		public override void Init (FObject obj, FEditor owner)
		{
			ShowTextures = new AnimBool(!FUtility.CollapseCommentTracks);

			base.Init(obj, owner);

			ShowTextures.valueChanged.AddListener( SequenceEditor.Repaint );
		}
	}
}

