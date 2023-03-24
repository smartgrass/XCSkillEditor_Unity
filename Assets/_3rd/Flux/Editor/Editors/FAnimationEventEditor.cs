using UnityEngine;
using UnityEditor.Animations;
using UnityEditor;
//using UnityEditorInternal;

using System;
using System.Collections.Generic;

using Flux;

namespace FluxEditor
{
    [FEditor( typeof( FPlayAnimationEvent ) )]
    public class FAnimationEventEditor : FEventEditor
    {

//        [SerializeField]
		protected FPlayAnimationEvent AnimEvt { get { return (FPlayAnimationEvent)Obj; } }
//        [SerializeField]
		protected FAnimationTrack AnimTrack { get { return (FAnimationTrack)Owner.Obj; } }

		protected AnimatorState _animState;

		protected AnimatorStateTransition _transitionToState;

        private SerializedObject _animEvtSO;
        private SerializedProperty _blendLength;
        private SerializedProperty _startOffset;

        private SerializedObject _transitionSO;
		private SerializedProperty _transitionExitTime;
        private SerializedProperty _transitionDuration;
        private SerializedProperty _transitionOffset;

        private static int _mouseDown = int.MinValue;

        //		private UndoPropertyModification[] UndoProperties( UndoPropertyModification[] modifications )
        //		{
        //			Debug.Log ("UndoProperties");
        //			foreach( UndoPropertyModification modification in modifications )
        //			{
        //				Debug.Log ( "obj ref: " + modification.propertyModification.objectReference + "; path: " + modification.propertyModification.propertyPath + " target:" + modification.propertyModification.target + "; value:" + modification.propertyModification.value );
        //			}
        //			return modifications;
        //		}


		public override void OnDelete()
		{
			FAnimationEventInspector.CheckDeleteAnimation( AnimEvt );
		}
			
		private void UpdateEventFromController()
		{
			bool isBlending = AnimEvt.IsBlending();

			if( isBlending )
			{
				if( _transitionToState == null )
				{
					_transitionToState = FAnimationTrackInspector.GetTransitionTo( AnimEvt );

					if( _transitionToState == null || _transitionToState.conditions.Length > 0 )
					{
						FAnimationTrackInspector.RebuildStateMachine( (FAnimationTrack)TrackEditor.Track );

						_transitionToState = FAnimationTrackInspector.GetTransitionTo( AnimEvt );
					}
				}

				if(  _transitionSO == null )
				{
					if( _transitionToState != null )
					{
						_transitionSO = new SerializedObject( _transitionToState );
						_transitionExitTime = _transitionSO.FindProperty( "m_ExitTime" );
						_transitionDuration = _transitionSO.FindProperty( "m_TransitionDuration" );
						_transitionOffset = _transitionSO.FindProperty( "m_TransitionOffset" );
					}
				}
				else if( _transitionSO.targetObject == null )
				{
					_transitionExitTime = null;
					_transitionDuration = null;
					_transitionOffset = null;
					_transitionSO = null;
				}
			}
			else
			{
				if( _transitionToState != null || _transitionSO != null )
				{
					_transitionToState = null;
					_transitionSO = null;
					_transitionExitTime = null;
					_transitionOffset = null;
					_transitionDuration = null;
				}
			}

			if( _transitionSO != null )
			{
				_transitionSO.Update();
				_animEvtSO.Update();
				
				FPlayAnimationEvent prevAnimEvt = (FPlayAnimationEvent)AnimTrack.Events[AnimEvt.GetId() - 1];

				AnimationClip prevAnimEvtClip = prevAnimEvt._animationClip;
				if( prevAnimEvtClip != null )
				{
					float exitTimeNormalized = (prevAnimEvt.Length + prevAnimEvt._startOffset) / (prevAnimEvtClip.frameRate * prevAnimEvtClip.length);
										
					if( !Mathf.Approximately( exitTimeNormalized, _transitionExitTime.floatValue ) )
					{
						_transitionExitTime.floatValue = exitTimeNormalized;
					}
					
					float blendNormalized = (_blendLength.intValue / prevAnimEvtClip.frameRate) / prevAnimEvtClip.length;
					
					if( !Mathf.Approximately( blendNormalized, _transitionDuration.floatValue ) )
					{
						_blendLength.intValue = Mathf.Clamp( Mathf.RoundToInt( _transitionDuration.floatValue * prevAnimEvtClip.length * prevAnimEvtClip.frameRate ), 0, AnimEvt.Length);
						
						_transitionDuration.floatValue = (_blendLength.intValue / prevAnimEvtClip.frameRate) / prevAnimEvtClip.length;

						_animEvtSO.ApplyModifiedProperties();
						
					}

					float startOffsetNorm = (_startOffset.intValue / AnimEvt._animationClip.frameRate) / AnimEvt._animationClip.length;

					if( !Mathf.Approximately( startOffsetNorm, _transitionOffset.floatValue ) )
					{
						_startOffset.intValue = Mathf.RoundToInt( _transitionOffset.floatValue * AnimEvt._animationClip.length * AnimEvt._animationClip.frameRate );
						_transitionOffset.floatValue = (_startOffset.intValue / AnimEvt._animationClip.frameRate) / AnimEvt._animationClip.length;

						_animEvtSO.ApplyModifiedProperties();
					}
				}

				_transitionSO.ApplyModifiedProperties();
			}
		}

        protected override void RenderEvent( FrameRange viewRange, FrameRange validKeyframeRange )
        {
            if( _animEvtSO == null )
            {
                _animEvtSO = new SerializedObject( AnimEvt );
                _blendLength = _animEvtSO.FindProperty( "_blendLength" );
                _startOffset = _animEvtSO.FindProperty( "_startOffset" );
            }

			UpdateEventFromController();

			_animEvtSO.Update();

			FAnimationTrackEditor animTrackEditor = (FAnimationTrackEditor)TrackEditor;

            Rect transitionOffsetRect = _eventRect;

            int startOffsetHandleId = EditorGUIUtility.GetControlID( FocusType.Passive );
            int transitionHandleId = EditorGUIUtility.GetControlID( FocusType.Passive );

			bool isBlending = AnimEvt.IsBlending();
            bool isAnimEditable = Flux.FUtility.IsAnimationEditable(AnimEvt._animationClip);

			if( true )
			{
				transitionOffsetRect.xMin = Rect.xMin + SequenceEditor.GetXForFrame( AnimEvt.Start + AnimEvt._blendLength ) - 3;
				transitionOffsetRect.width = 6;
				transitionOffsetRect.yMin = transitionOffsetRect.yMax - 8;
			}

	        switch( Event.current.type )
	        {
            case EventType.MouseDown:
                if( EditorGUIUtility.hotControl == 0 && Event.current.alt && !isAnimEditable )
                {
                    if( isBlending && transitionOffsetRect.Contains( Event.current.mousePosition ) )
                    {
                        EditorGUIUtility.hotControl = transitionHandleId;

						AnimatorWindowProxy.OpenAnimatorWindowWithAnimatorController( (AnimatorController)AnimTrack.AnimatorController );

                        if( Selection.activeObject != _transitionToState )
                            Selection.activeObject = _transitionToState;

                        Event.current.Use();
                    }
                    else if( _eventRect.Contains( Event.current.mousePosition ) )
	                {
	                    _mouseDown = SequenceEditor.GetFrameForX( Event.current.mousePosition.x ) - AnimEvt.Start;

	                    EditorGUIUtility.hotControl = startOffsetHandleId;

	                    Event.current.Use();
	                }
                }
                break;

			case EventType.Ignore:
            case EventType.MouseUp:
                if( EditorGUIUtility.hotControl == transitionHandleId
                   || EditorGUIUtility.hotControl == startOffsetHandleId )
                {
                    EditorGUIUtility.hotControl = 0;
                    Event.current.Use();
                }
                break;

            case EventType.MouseDrag:
                if( EditorGUIUtility.hotControl == transitionHandleId )
                {
                    int mouseDragPos = Mathf.Clamp( SequenceEditor.GetFrameForX( Event.current.mousePosition.x - Rect.x ) - AnimEvt.Start, 0, AnimEvt.Length );

                    if( _blendLength.intValue != mouseDragPos )
                    {
                        _blendLength.intValue = mouseDragPos;

						FPlayAnimationEvent prevAnimEvt = (FPlayAnimationEvent)animTrackEditor.Track.GetEvent( AnimEvt.GetId()-1 );

						if( _transitionDuration != null )
                        	_transitionDuration.floatValue = (_blendLength.intValue / prevAnimEvt._animationClip.frameRate) / prevAnimEvt._animationClip.length;

                        Undo.RecordObject( this, "Animation Blending" );
                    }
                    Event.current.Use();
                }
                else if( EditorGUIUtility.hotControl == startOffsetHandleId )
                {
                    int mouseDragPos = Mathf.Clamp( SequenceEditor.GetFrameForX( Event.current.mousePosition.x - Rect.x ) - AnimEvt.Start, 0, AnimEvt.Length );

                    int delta = _mouseDown - mouseDragPos;

                    _mouseDown = mouseDragPos;

                    _startOffset.intValue = Mathf.Clamp( _startOffset.intValue + delta, 0, AnimEvt._animationClip.isLooping ? AnimEvt.Length : Mathf.RoundToInt( AnimEvt._animationClip.length * AnimEvt._animationClip.frameRate ) - AnimEvt.Length );

					if( _transitionOffset != null )
                   		_transitionOffset.floatValue = (_startOffset.intValue / AnimEvt._animationClip.frameRate) / AnimEvt._animationClip.length;

                    Undo.RecordObject( this, "Animation Offset" );
                    
                    Event.current.Use();
                }
                break;
	        }

			_animEvtSO.ApplyModifiedProperties();
			if( _transitionSO != null )
				_transitionSO.ApplyModifiedProperties();


            switch( Event.current.type )
            {
	        case EventType.DragUpdated:
	            if( _eventRect.Contains( Event.current.mousePosition ) )
	            {
	                int numAnimationsDragged = FAnimationEventInspector.NumAnimationsDragAndDrop( Evt.Sequence.FrameRate );
	                DragAndDrop.visualMode = numAnimationsDragged > 0 ? DragAndDropVisualMode.Link : DragAndDropVisualMode.Rejected;
	                Event.current.Use();
	            }
	            break;
	        case EventType.DragPerform:
	            if( _eventRect.Contains( Event.current.mousePosition ) )
	            {
					AnimationClip animationClipDragged = FAnimationEventInspector.GetAnimationClipDragAndDrop( Evt.Sequence.FrameRate );
	                if( animationClipDragged )
	                {
						int animFrameLength = Mathf.RoundToInt(animationClipDragged.length * animationClipDragged.frameRate);
						
	                    FAnimationEventInspector.SetAnimationClip( AnimEvt, animationClipDragged );

						FrameRange maxRange = AnimEvt.GetMaxFrameRange();

						SequenceEditor.MoveEvent( AnimEvt, new FrameRange( AnimEvt.Start, Mathf.Min( AnimEvt.Start+animFrameLength, maxRange.End ) ) );

	                    DragAndDrop.AcceptDrag();
	                    Event.current.Use();
	                }
	                else
	                {
	                    Event.current.Use();
	                }
	            }
	            break;
            }

//            FrameRange currentRange = Evt.FrameRange;

            base.RenderEvent( viewRange, validKeyframeRange );

//            if( isAnimEditable && currentRange.Length != Evt.FrameRange.Length )
//            {
//                FAnimationEventInspector.ScaleAnimationClip( AnimEvt._animationClip, Evt.FrameRange );
//            }

	        if( Event.current.type == EventType.Repaint )
	        {
				if( isBlending && !isAnimEditable && viewRange.Contains( AnimEvt.Start+AnimEvt._blendLength ) )
	            {
	                GUISkin skin = FUtility.GetFluxSkin();

	                GUIStyle transitionOffsetStyle = skin.GetStyle( "BlendOffset" );

					Texture2D t = FUtility.GetFluxTexture( "EventBlend.png" );

					Rect r = new Rect( _eventRect.xMin, _eventRect.yMin+1, transitionOffsetRect.center.x - _eventRect.xMin, _eventRect.height-2 );

					Color guiColor = GUI.color;

					Color c = new Color(1f, 1f, 1f, 0.3f);
					c.a *= guiColor.a;
					GUI.color = c;

					GUI.DrawTexture( r, t );

					if( Event.current.alt )
						GUI.color = Color.white;

                	transitionOffsetStyle.Draw( transitionOffsetRect, false, false, false, false );

					GUI.color = guiColor;
	            }
					
	            if( EditorGUIUtility.hotControl == transitionHandleId )
	            {
	                Rect transitionOffsetTextRect = transitionOffsetRect;
	                transitionOffsetTextRect.y -= 16;
	                transitionOffsetTextRect.height = 20;
	                transitionOffsetTextRect.width += 50;
	                GUI.Label( transitionOffsetTextRect, AnimEvt._blendLength.ToString(), EditorStyles.label );
	            }

	            if( EditorGUIUtility.hotControl == startOffsetHandleId )
	            {
	                Rect startOffsetTextRect = _eventRect;
	                GUI.Label( startOffsetTextRect, AnimEvt._startOffset.ToString(), EditorStyles.label );
	            }
	        }
        }

		public override void OnEventFinishedMoving( FrameRange oldFrameRange )
		{
			if( AnimEvt.ControlsAnimation && oldFrameRange.Length != AnimEvt.FrameRange.Length && Flux.FUtility.IsAnimationEditable(AnimEvt._animationClip) )
			{
				FAnimationEventInspector.ScaleAnimationClip( AnimEvt._animationClip, AnimEvt.FrameRange );
			}
		}

		private TransformCurves _transformCurves = null;

		public void CreateTransformCurves()
		{
			if( _transformCurves == null )
			{
				_transformCurves = new TransformCurves( AnimEvt.Owner, AnimEvt._animationClip );
			}
		}

		public void RenderTransformCurves( int samplesPerSecond )
		{
			if( _transformCurves == null || _transformCurves.clip == null )
				CreateTransformCurves();
			else
				_transformCurves.RefreshCurves();

			float totalTime = AnimEvt.LengthTime;
			float timePerSample = totalTime / samplesPerSecond;

			int numSamples = Mathf.RoundToInt( totalTime / timePerSample ) + 1;

			Vector3[] pts = new Vector3[numSamples];
			float t = 0;

			for( int i = 0; i < numSamples; ++i )
			{
				pts[i] = _transformCurves.GetWorldPosition( t );
				t += timePerSample;
			}

			Handles.DrawPolyLine( pts );

			FAnimationTrackEditor animTrackEditor = (FAnimationTrackEditor)TrackEditor;

			if( animTrackEditor.ShowKeyframes || animTrackEditor.ShowKeyframeTimes )
			{
				int animFramerate = Mathf.RoundToInt( _transformCurves.clip.frameRate );

				Keyframe[] keyframes = _transformCurves.GetPositionKeyframes();
				for( int i = 0; i != keyframes.Length; ++i )
				{
					Keyframe keyframe = keyframes[i];

					Vector3 pos = _transformCurves.GetPosition( keyframe.time );
					Quaternion rot = _transformCurves.GetRotation( keyframe.time );

					Quaternion toolRot = rot;
					bool isGlobalRotation = Tools.pivotRotation == PivotRotation.Global;
					if( isGlobalRotation )
						toolRot = _globalRotation;

					if( animTrackEditor.ShowKeyframes )
					{

						if( Tools.current == Tool.Move )
						{
							Vector3 newPos = Handles.DoPositionHandle(pos, toolRot );//Handles.PositionHandle( pos, rot );
							if( newPos != pos )
							{
								Undo.RecordObject( _transformCurves.clip, "Change Keyframe" );
								_transformCurves.SetPosition( newPos, keyframe.time );
							}
						}
						else if( Tools.current == Tool.Rotate )
						{
							Quaternion newRot = Handles.DoRotationHandle( toolRot, pos );
							if( newRot != toolRot )
							{
								Undo.RecordObject( _transformCurves.clip, "Change Keyframe" );
								_transformCurves.SetRotation( isGlobalRotation ? (newRot*Quaternion.Inverse(toolRot)) * rot : newRot, keyframe.time );
								if( isGlobalRotation )
									_globalRotation = newRot;
							}
						}
					}

					if( Event.current.type == EventType.MouseUp || Event.current.type == EventType.Ignore )
						_globalRotation = Quaternion.identity;

					if( animTrackEditor.ShowKeyframeTimes )
					{
						int frame = Mathf.RoundToInt(keyframe.time * animFramerate);
						Handles.Label( pos + new Vector3(0, .25f, 0), FUtility.GetTime( AnimEvt.Start+frame, animFramerate) , EditorStyles.toolbarButton );
					}
				}
			}
		}

		private static Quaternion _globalRotation = Quaternion.identity;

        private void RebuildAnimationTrack()
        {
            FAnimationTrackInspector.RebuildStateMachine( (FAnimationTrack)AnimEvt.Track );
        }
    }

	public class TransformCurves
	{
		public Transform bone = null;
		public AnimationClip clip = null;

		public AnimationCurve xPos = null;
		public AnimationCurve yPos = null;
		public AnimationCurve zPos = null;
		
		public AnimationCurve xRot = null;
		public AnimationCurve yRot = null;
		public AnimationCurve zRot = null;
		public AnimationCurve wRot = null;

		public AnimationCurve xScale = null;
		public AnimationCurve yScale = null;
		public AnimationCurve zScale = null;
		
		public TransformCurves( Transform transform, AnimationClip clip )
		{
			bone = transform;
			this.clip = clip;
			
			RefreshCurves();
		}

		public void RefreshCurves()
		{
         EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);

         foreach (EditorCurveBinding b in bindings)
            ParseCurve(b.type, b.propertyName, AnimationUtility.GetEditorCurve(clip, b));
      }

      private void ParseCurve(Type type, string propertyName, AnimationCurve curve)
      {
			System.Type transformType = typeof(Transform);

			if( type != transformType )
				return;
				
			switch( propertyName )
			{
			case "m_LocalPosition.x":
				xPos = curve;
				break;
			case "m_LocalPosition.y":
				yPos = curve;
				break;
			case "m_LocalPosition.z":
				zPos = curve;
				break;
			case "m_LocalRotation.x":
				xRot = curve;
				break;
			case "m_LocalRotation.y":
				yRot = curve;
				break;
			case "m_LocalRotation.z":
				zRot = curve;
				break;
			case "m_LocalRotation.w":
				wRot = curve;
				break;
			case "m_LocalScale.x":
				xScale = curve;
				break;
			case "m_LocalScale.y":
				yScale = curve;
				break;
			case "m_LocalScale.z":
				zScale = curve;
				break;
			}
		}

		public Vector3 GetWorldPosition( float t )
		{
			Vector3 localPos = GetPosition( t );

			if( bone.parent == null )
				return localPos;

			return bone.parent.TransformPoint( localPos );
		}
		
		public Vector3 GetPosition( float t )
		{
			Vector3 pos = Vector3.zero;
			
			if( xPos != null )
				pos.x = xPos.Evaluate(t);
			if( yPos != null )
				pos.y = yPos.Evaluate(t);
			if( zPos != null )
				pos.z = zPos.Evaluate(t);
			
			return pos;
		}

		public void SetPosition( Vector3 pos, int keyframeIndex )
		{
			Keyframe keyframe;
			if( xPos != null )
			{
				keyframe = xPos.keys[keyframeIndex];
				keyframe.value = pos.x;
				xPos.MoveKey( keyframeIndex, keyframe );
				AnimationUtility.SetEditorCurve( clip, EditorCurveBinding.FloatCurve( string.Empty, typeof(Transform), "m_LocalPosition.x"), xPos );
			}
			if( yPos != null )
			{
				keyframe = yPos.keys[keyframeIndex];
				keyframe.value = pos.y;
				yPos.MoveKey( keyframeIndex, keyframe );
				AnimationUtility.SetEditorCurve( clip, EditorCurveBinding.FloatCurve( string.Empty, typeof(Transform), "m_LocalPosition.y"), yPos );
			}
			if( zPos != null )
			{
				keyframe = zPos.keys[keyframeIndex];
				keyframe.value = pos.z;
				zPos.MoveKey( keyframeIndex, keyframe );
				AnimationUtility.SetEditorCurve( clip, EditorCurveBinding.FloatCurve( string.Empty, typeof(Transform), "m_LocalPosition.z"), zPos );
			}
		}

		public void SetPosition( Vector3 pos, float t )
		{
			Keyframe keyframe = new Keyframe();
			int keyframeIndex = 0;
			if( xPos != null  )
			{
				if( TryGetKeyframeAt(xPos, t, ref keyframe, ref keyframeIndex ) )
				{
					keyframe.value = pos.x;
					xPos.MoveKey( keyframeIndex, keyframe );
				}
				else
				{
					xPos.AddKey( t, pos.x );
				}
				AnimationUtility.SetEditorCurve( clip, EditorCurveBinding.FloatCurve( string.Empty, typeof(Transform), "m_LocalPosition.x"), xPos );
			}
			if( yPos != null )
			{
				if( TryGetKeyframeAt(yPos, t, ref keyframe, ref keyframeIndex ) )
				{
					keyframe.value = pos.y;
					yPos.MoveKey( keyframeIndex, keyframe );
				}
				else
				{
					yPos.AddKey( t, pos.y );
				}
				AnimationUtility.SetEditorCurve( clip, EditorCurveBinding.FloatCurve( string.Empty, typeof(Transform), "m_LocalPosition.y"), yPos );
			}
			if( zPos != null )
			{
				if( TryGetKeyframeAt(zPos, t, ref keyframe, ref keyframeIndex ) )
				{
					keyframe.value = pos.z;
					zPos.MoveKey( keyframeIndex, keyframe );
				}
				else
				{
					zPos.AddKey( t, pos.z );
				}
				AnimationUtility.SetEditorCurve( clip, EditorCurveBinding.FloatCurve( string.Empty, typeof(Transform), "m_LocalPosition.z"), zPos );
			}
			AnimationWindowProxy.AnimationWindow.Repaint();
		}

		public void SetRotation( Quaternion rot, float t )
		{
			Keyframe keyframe = new Keyframe();
			int keyframeIndex = 0;
			if( xRot != null  )
			{
				if( TryGetKeyframeAt(xRot, t, ref keyframe, ref keyframeIndex ) )
				{
					keyframe.value = rot.x;
					xRot.MoveKey( keyframeIndex, keyframe );
				}
				else
				{
					xRot.AddKey( t, rot.x );
				}
				AnimationUtility.SetEditorCurve( clip, EditorCurveBinding.FloatCurve( string.Empty, typeof(Transform), "m_LocalRotation.x"), xRot );
			}
			if( yRot != null  )
			{
				if( TryGetKeyframeAt(yRot, t, ref keyframe, ref keyframeIndex ) )
				{
					keyframe.value = rot.y;
					yRot.MoveKey( keyframeIndex, keyframe );
				}
				else
				{
					yRot.AddKey( t, rot.y );
				}
				AnimationUtility.SetEditorCurve( clip, EditorCurveBinding.FloatCurve( string.Empty, typeof(Transform), "m_LocalRotation.y"), yRot );
			}
			if( zRot != null  )
			{
				if( TryGetKeyframeAt(zRot, t, ref keyframe, ref keyframeIndex ) )
				{
					keyframe.value = rot.z;
					zRot.MoveKey( keyframeIndex, keyframe );
				}
				else
				{
					zRot.AddKey( t, rot.z );
				}
				AnimationUtility.SetEditorCurve( clip, EditorCurveBinding.FloatCurve( string.Empty, typeof(Transform), "m_LocalRotation.z"), zRot );
			}
			if( wRot != null  )
			{
				if( TryGetKeyframeAt(wRot, t, ref keyframe, ref keyframeIndex ) )
				{
					keyframe.value = rot.w;
					wRot.MoveKey( keyframeIndex, keyframe );
				}
				else
				{
					wRot.AddKey( t, rot.w );
				}
				AnimationUtility.SetEditorCurve( clip, EditorCurveBinding.FloatCurve( string.Empty, typeof(Transform), "m_LocalRotation.w"), wRot );
			}
			AnimationWindowProxy.AnimationWindow.Repaint();
			
		}

		public bool TryGetKeyframeAt( AnimationCurve curve, float t, ref Keyframe keyframe, ref int keyframeIndex )
		{
			for( int i = 0; i != curve.keys.Length; ++i )
			{
				if( Mathf.Approximately( t, curve.keys[i].time ) )
				{
					keyframe = curve.keys[i];
					keyframeIndex = i;
					return true;
				}
			}

			return false;
		}

		public Keyframe[] GetPositionKeyframes()
		{
			Keyframe[] keyframes = xPos != null ? xPos.keys : (yPos != null ? yPos.keys : (zPos != null ? zPos.keys : new Keyframe[0]));

			bool addedFrames = false;

			if( yPos != null )
			{
				foreach( Keyframe k in yPos.keys )
				{
					if( !HasKeyframe( ref keyframes, k.time ) )
					{
						ArrayUtility.Add<Keyframe>( ref keyframes, k );
						addedFrames = true;
					}
				}
			}

			if( zPos != null )
			{
				foreach( Keyframe k in zPos.keys )
				{
					if( !HasKeyframe( ref keyframes, k.time ) )
					{
						ArrayUtility.Add<Keyframe>( ref keyframes, k );
						addedFrames = true;
					}
				}
			}

			if( addedFrames )
			{
				Array.Sort<Keyframe>( keyframes, delegate(Keyframe a, Keyframe b) {
					return a.time.CompareTo( b.time );
				} );
			}

			return keyframes;
		}

		private bool HasKeyframe( ref Keyframe[] keyframes, float t )
		{
			foreach( Keyframe k in keyframes )
			{
				if( Mathf.Approximately( k.time, t ) )
					return true;
				if( k.time > t )
					break;
			}

			return false;
		}
		
		public Quaternion GetRotation( float t )
		{
			Quaternion rot = Quaternion.identity;
			
			// since the quaternion may not be normalized, we have to do it ourselves
			//				Vector4 rotVec = new Vector4( xRot.Evaluate(t), yRot.Evaluate(t), zRot.Evaluate(t), wRot.Evaluate(t) );
			//				rotVec.Normalize();
			
			//				Quaternion rot = new Quaternion( rotVec.x, rotVec.y, rotVec.z, rotVec.w );
			
			// probably this doesn't make sense to check? You need to have the 4 channels
			if( xRot != null )
				rot.x = xRot.Evaluate( t );
			if( yRot != null )
				rot.y = yRot.Evaluate( t );
			if( zRot != null )
				rot.z = zRot.Evaluate( t );
			if( wRot != null )
				rot.w = wRot.Evaluate( t );

//			rot.eulerAngles = new Vector3( xRot2.Evaluate(t), yRot2.Evaluate(t), zRot2.Evaluate(t) );

			return rot;
		}
		
		public Vector3 GetScale( float t )
		{
			Vector3 scale = Vector3.one;
			
			if( xScale != null )
				scale.x = xScale.Evaluate(t);
			if( yScale != null )
				scale.y = yScale.Evaluate(t);
			if( zScale != null )
				scale.z = zScale.Evaluate(t);
			
			return scale;
		}
	}
}
