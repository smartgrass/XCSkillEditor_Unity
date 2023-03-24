using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

using Flux;

namespace FluxEditor
{

	[FEditor(typeof(FCommentEvent))]
	public class FCommentEventEditor : FEventEditor {

		private const float REBUILD_DELTA_TIME = 0.2f;

		private const int LABEL_PADDING_TOP_EXPANDED = 8;
		private const int LABEL_PADDING_TOP_COLLAPSED = 2;

		private GUIStyle _textStyle = null;

		private RenderTexture[] _previewTextures = null;

		private float _prevPixelsPerFrame = 0;

		private double _nextTimeChanged = 0;

		private Texture2D _filmStripTexture = null;

		protected override void OnDestroy()
		{
			base.OnDestroy();
			Undo.postprocessModifications -= OnUndoPropertyModifications;

			if( TrackEditor != null )
			{
				((FCommentTrackEditor)TrackEditor).ShowTextures.valueChanged.RemoveListener( OnShowTexturesChanged );
			}

			if( _previewTextures != null )
			{
				for( int i = 0; i != _previewTextures.Length; ++i )
				{
					FCommentTrackEditor.ReleaseTexture(_previewTextures[i]);
				}
				_previewTextures = null;
			}
		}

		public override void Init (FObject obj, FEditor owner)
		{
			base.Init(obj,owner);
			Undo.postprocessModifications += OnUndoPropertyModifications;

			((FCommentTrackEditor)TrackEditor).ShowTextures.valueChanged.AddListener( OnShowTexturesChanged );
		}

		private void OnShowTexturesChanged()
		{
			if( _textStyle != null )
				_textStyle.padding.top = ((FCommentTrackEditor)TrackEditor).ShowTextures.value ? LABEL_PADDING_TOP_EXPANDED : LABEL_PADDING_TOP_COLLAPSED;
		}

		protected override void RenderEvent (FrameRange viewRange, FrameRange validKeyframeRange)
		{
			base.RenderEvent (viewRange, validKeyframeRange);

			if( _prevPixelsPerFrame != SequenceEditor.PixelsPerFrame )
			{
                Debug.LogWarning($"yns  MarkNeedsRebuild");
				MarkNeedsRebuild();
			}

			if( _nextTimeChanged != double.MaxValue )
				SequenceEditor.Repaint();

			if( _previewTextures == null || (_previewTextures.Length > 0 && _previewTextures[0] == null) || _nextTimeChanged <= EditorApplication.timeSinceStartup )
				RebuildPreviewTextures();

			FCommentTrackEditor commentTrackEditor = (FCommentTrackEditor)TrackEditor;

			if( _previewTextures.Length == 0 || !commentTrackEditor.ShowTextures.value )
				return;

			Color guiColor = GUI.color;

			Rect screenshotRect = Rect;
			screenshotRect.width = FCommentTrackEditor.TEXTURE_WIDTH;
			for( int i = 0; i != _previewTextures.Length; ++i )
			{
				if( screenshotRect.xMax < _eventRect.xMin )
				{
					screenshotRect.x += FCommentTrackEditor.TEXTURE_WIDTH;
					continue; // not visible yet
				}
				if( screenshotRect.xMin > _eventRect.xMax )
				{
					break; // no more visible
				}

				Rect visibleScreenshotRect = screenshotRect;
				Rect uvRect = new Rect(0f, 0f, 1f, 1f);
				if( screenshotRect.xMin < _eventRect.xMin )
				{
					visibleScreenshotRect.xMin = _eventRect.xMin;
					uvRect.xMin += (visibleScreenshotRect.xMin - screenshotRect.xMin) / FCommentTrackEditor.TEXTURE_WIDTH;
				}
				if( screenshotRect.xMax > _eventRect.xMax )
				{
					visibleScreenshotRect.xMax = _eventRect.xMax;
					uvRect.xMax -= (screenshotRect.xMax - visibleScreenshotRect.xMax) / FCommentTrackEditor.TEXTURE_WIDTH;
				}

				GUI.color = IsSelected ? guiColor : new Color(0.85f, 0.85f, 0.85f, guiColor.a);
				GUI.DrawTextureWithTexCoords(visibleScreenshotRect, _previewTextures[i], uvRect);
				GUI.DrawTextureWithTexCoords(visibleScreenshotRect, _filmStripTexture, uvRect);

				GUI.color = guiColor;

				screenshotRect.x += FCommentTrackEditor.TEXTURE_WIDTH;
			}

			string text = Evt.Text;
			if( text != null )
				GUI.Label( _eventRect, text, GetTextStyle() );
		}

		public override void OnEventFinishedMoving (FrameRange oldFrameRange)
		{
			base.OnEventFinishedMoving(oldFrameRange);

			RebuildPreviewTextures();
		}

		private UndoPropertyModification[] OnUndoPropertyModifications( UndoPropertyModification[] modifications )
		{
			if( TrackEditor == null ) // can happen when it is deleted
				return modifications; 
			
			for( int i = 0; i != modifications.Length; ++i )
			{
				if( modifications[i].currentValue.target is Transform )
				{
					_prevPixelsPerFrame = 0;
					SequenceEditor.Repaint();
					break;
				}
			}

			return modifications;
		}
			
		private void MarkNeedsRebuild()
		{
			if( !Application.isPlaying && SequenceEditor != null && !SequenceEditor.IsPlaying && _prevPixelsPerFrame != SequenceEditor.PixelsPerFrame )
			{
				_nextTimeChanged = EditorApplication.timeSinceStartup + REBUILD_DELTA_TIME;
				_prevPixelsPerFrame = SequenceEditor.PixelsPerFrame;
			}
		}

		private void RebuildPreviewTextures()
		{
			if( AnimationMode.InAnimationMode() )
			{
				_nextTimeChanged = EditorApplication.timeSinceStartup + REBUILD_DELTA_TIME;
				return;
			}
			
			if( _filmStripTexture == null )
				_filmStripTexture = FUtility.GetFluxTexture("FilmStrip.png");
				
			_nextTimeChanged = double.MaxValue;
			_prevPixelsPerFrame = SequenceEditor.PixelsPerFrame;
			int numScreenshots = Mathf.CeilToInt(Rect.width / FCommentTrackEditor.TEXTURE_WIDTH);
			if( _previewTextures == null )
			{
				_previewTextures = new RenderTexture[numScreenshots];
			}
			else
			{
				while( _previewTextures.Length < numScreenshots )
					ArrayUtility.Add<RenderTexture>( ref _previewTextures, FCommentTrackEditor.RequestTexture() );
				
				while( _previewTextures.Length > numScreenshots )
				{
					FCommentTrackEditor.ReleaseTexture( _previewTextures[_previewTextures.Length-1] );
					ArrayUtility.RemoveAt<RenderTexture>(ref _previewTextures, _previewTextures.Length-1);
				}
			}

			int currentFrame = SequenceEditor.Sequence.CurrentFrame;

			int screenshotFrame = Evt.Start;

			HideFlags cameraHideFlags = Camera.main.hideFlags;
			Camera.main.hideFlags |= HideFlags.DontSave;
			for( int i = 0; i < _previewTextures.Length; ++i )
			{
				SequenceEditor.SetCurrentFrame(screenshotFrame);
				_previewTextures[i] = FCommentTrackEditor.RequestTexture();
				Camera.main.targetTexture = _previewTextures[i];
				Camera.main.Render();
				Camera.main.targetTexture = null;

				screenshotFrame += (int)(FCommentTrackEditor.TEXTURE_WIDTH/SequenceEditor.PixelsPerFrame);
			}
			Camera.main.hideFlags = cameraHideFlags;

			if( currentFrame < 0 )
				SequenceEditor.Stop();
			else
				SequenceEditor.SetCurrentFrame( currentFrame );
		}

		private void InitTextStyle()
		{
			_textStyle = new GUIStyle(EditorStyles.whiteLabel);
			
			_textStyle.padding.left = 2;
			_textStyle.padding.right = 2;
			_textStyle.padding.top = ((FCommentTrackEditor)TrackEditor).ShowTextures.value ? LABEL_PADDING_TOP_EXPANDED : LABEL_PADDING_TOP_COLLAPSED;
			
			_textStyle.alignment = TextAnchor.UpperCenter;
			_textStyle.fontStyle = FontStyle.Bold;
		}

		public override Color GetColor ()
		{
			return ((FCommentEvent)Evt).Color;
		}

		public override GUIStyle GetEventStyle()
		{
			return FUtility.GetCommentEventStyle();
		}

		public override GUIStyle GetTextStyle()
		{
			if( _textStyle == null ) InitTextStyle();
			return _textStyle;
		}
	}
}
