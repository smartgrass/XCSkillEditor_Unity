using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
//using UnityEditorInternal;
using System.Collections.Generic;
using Flux;
using FluxEditor;

namespace FluxEditor
{
	[CustomEditor(typeof(FPlayAnimationEvent))]
	public class FAnimationEventInspector : FEventInspector
	{
		private FPlayAnimationEvent _animEvent = null;

		private SerializedProperty _animationClip = null;

		private SerializedProperty _frameRange = null;

		private SerializedProperty _controlsAnimation = null;

		private SerializedProperty _startOffset = null;
		private SerializedProperty _blendLength = null;

		protected override void OnEnable()
		{
			base.OnEnable();

			if( target == null )
			{
				DestroyImmediate( this );
				return;

			}
			_animEvent = (FPlayAnimationEvent)target;
			_animationClip = serializedObject.FindProperty("_animationClip");

			_controlsAnimation = serializedObject.FindProperty("_controlsAnimation");

			_startOffset = serializedObject.FindProperty("_startOffset");
			_blendLength = serializedObject.FindProperty("_blendLength");

	        _frameRange = serializedObject.FindProperty("_frameRange");
		}

		public override void OnInspectorGUI ()
		{
			bool rebuildTrack = false;

			base.OnInspectorGUI();

			serializedObject.Update();

			if( _animationClip.objectReferenceValue == null )
			{
				EditorGUILayout.HelpBox("There's no Animation Clip", MessageType.Warning);
				Rect helpBoxRect = GUILayoutUtility.GetLastRect();
				
				float yCenter = helpBoxRect.center.y;
				
				helpBoxRect.xMax -= 3;
				helpBoxRect.xMin = helpBoxRect.xMax - 50;
				helpBoxRect.yMin = yCenter - 12.5f;
				helpBoxRect.height = 25f;

				FAnimationTrack animTrack = (FAnimationTrack)_animEvent.Track;

				if( animTrack.AnimatorController == null || animTrack.LayerId == -1 )
					GUI.enabled = false;

				if( GUI.Button( helpBoxRect, "Create" ) )
				{
					CreateAnimationClip( _animEvent );
				}

				GUI.enabled = true;
			}

			EditorGUI.BeginChangeCheck();
			//EditorGUILayout.PropertyField( _animationClip );

			Rect animationClipRect = GUILayoutUtility.GetLastRect();

			if( Event.current.type == EventType.DragUpdated && animationClipRect.Contains( Event.current.mousePosition ) )
			{
				int numAnimations = NumAnimationsDragAndDrop( _animEvent.Sequence.FrameRate );
				if( numAnimations > 0 )
					DragAndDrop.visualMode = DragAndDropVisualMode.Link;
			}
			else if( Event.current.type == EventType.DragPerform && animationClipRect.Contains( Event.current.mousePosition ) )
			{
				if( NumAnimationsDragAndDrop( _animEvent.Sequence.FrameRate ) > 0 )
				{
					DragAndDrop.AcceptDrag();
					AnimationClip clip = GetAnimationClipDragAndDrop( _animEvent.Sequence.FrameRate );
					if( clip != null )
						_animationClip.objectReferenceValue = clip;
				}
			}

			if( EditorGUI.EndChangeCheck() )
			{
				AnimationClip clip = (AnimationClip)_animationClip.objectReferenceValue;
				if( clip )
				{
					if( clip.frameRate != _animEvent.Track.Timeline.Sequence.FrameRate )
					{
						Debug.LogError(string.Format("Can't add animation, it has a different frame rate from the sequence ({0} vs {1})", 
						                             clip.frameRate, _animEvent.Track.Timeline.Sequence.FrameRate ) );

						_animationClip.objectReferenceValue = null;
					}
					else
					{
						SerializedProperty frameRangeEnd = _frameRange.FindPropertyRelative( "_end" );
	                    FrameRange maxFrameRange = _animEvent.GetMaxFrameRange();
						frameRangeEnd.intValue = Mathf.Min( _animEvent.FrameRange.Start + Mathf.RoundToInt( clip.length * clip.frameRate ), maxFrameRange.End );
					}

					rebuildTrack = true;
				}
				else
				{
					CheckDeleteAnimation( _animEvent );
				}
			}

			bool isAnimationEditable = Flux.FUtility.IsAnimationEditable( _animEvent._animationClip );

			if( isAnimationEditable )
			{
				EditorGUILayout.PropertyField( _controlsAnimation );
			}
			else
			{
				if( _controlsAnimation.boolValue )
					_controlsAnimation.boolValue = false;
			}

			if( !_controlsAnimation.boolValue )
			{

                //if( _animEvent.IsBlending() )
                if (true)
                {
                    float offset = _startOffset.intValue / _animEvent._animationClip.frameRate;
					float blendFinish = offset + (_blendLength.intValue  / _animEvent._animationClip.frameRate);

					EditorGUILayout.BeginHorizontal();

					EditorGUILayout.BeginHorizontal( GUILayout.Width(EditorGUIUtility.labelWidth-5) ); // hack to get around some layout issue with imgui
					_showBlendAndOffsetContent = EditorGUILayout.Foldout( _showBlendAndOffsetContent, new GUIContent("Offset+Blend") );
					EditorGUILayout.EndHorizontal();

					EditorGUI.BeginChangeCheck();
					EditorGUILayout.MinMaxSlider( ref offset, ref blendFinish, 0, _animEvent.GetMaxStartOffset() / _animEvent._animationClip.frameRate + _animEvent.LengthTime );
					if( EditorGUI.EndChangeCheck() )
					{
						_startOffset.intValue = Mathf.Clamp( Mathf.RoundToInt( offset * _animEvent.Sequence.FrameRate ), 0, _animEvent.GetMaxStartOffset() );
						_blendLength.intValue = Mathf.Clamp( Mathf.RoundToInt( (blendFinish - offset) * _animEvent.Sequence.FrameRate ), 0, _animEvent.Length );
						rebuildTrack = true;
					}

					EditorGUILayout.EndHorizontal();

					if( _showBlendAndOffsetContent )
					{
						EditorGUI.BeginChangeCheck();
						++EditorGUI.indentLevel;
						if( !isAnimationEditable )
							EditorGUILayout.IntSlider( _startOffset, 0, _animEvent.GetMaxStartOffset() );
						
						//			if( _animEvent.IsBlending() )
						{
							EditorGUILayout.IntSlider( _blendLength, 0, _animEvent.Length );
						}
						--EditorGUI.indentLevel;
						if( EditorGUI.EndChangeCheck() )
							rebuildTrack = true;
					}
				}
				else
				{
					EditorGUI.BeginChangeCheck();
					EditorGUILayout.IntSlider( _startOffset, 0, _animEvent.GetMaxStartOffset() );
					if( EditorGUI.EndChangeCheck() )
						rebuildTrack = true;
				}
			}

			serializedObject.ApplyModifiedProperties();

			if( rebuildTrack )
				FAnimationTrackInspector.RebuildStateMachine( (FAnimationTrack)_animEvent.Track );
		}

		public static void CheckDeleteAnimation( FPlayAnimationEvent animEvt )
		{
			if( animEvt._animationClip != null && Flux.FUtility.IsAnimationEditable( animEvt._animationClip ) /*AssetDatabase.IsSubAsset( animEvt._animationClip )*/
			   && EditorUtility.DisplayDialog("Delete animation?", 
			                               string.Format("The animation clip '{0}' is stored inside an animator controller, do you want to delete it?",animEvt._animationClip.name), 
			                               "Delete", "Keep") )
			{
				Undo.DestroyObjectImmediate( animEvt._animationClip );
				AssetDatabase.SaveAssets();
			}
		}

		private static bool _showBlendAndOffsetContent = false;

		public static int NumAnimationsDragAndDrop()
		{
			return NumAnimationsDragAndDrop( -1 );
		}

		public static int NumAnimationsDragAndDrop( int frameRate )
		{
			int numAnimations = 0;
			
			if( DragAndDrop.objectReferences.Length == 0 )
				return numAnimations;
			
			string pathToAsset = AssetDatabase.GetAssetPath(DragAndDrop.objectReferences[0].GetInstanceID());
			
			if( string.IsNullOrEmpty(pathToAsset) )
				return numAnimations;
            Debug.Log("yns no IsNullOrEmpty ");
			if( DragAndDrop.objectReferences[0] is AnimationClip && (frameRate >= 0 || Mathf.Approximately( ((AnimationClip)DragAndDrop.objectReferences[0]).frameRate, frameRate)) )
				++numAnimations;
            Debug.Log("yns objectReferences " + numAnimations);
			Object[] objs = AssetDatabase.LoadAllAssetRepresentationsAtPath( pathToAsset );
			
			foreach( Object obj in objs )
			{
				if( obj is AnimationClip && (frameRate >= 0 || Mathf.Approximately(((AnimationClip)obj).frameRate, frameRate)) )
					++numAnimations;
			}
			
			return numAnimations;
		}

		private static AnimationClip _dragAndDropSelectedAnimation = null;

		public static AnimationClip GetAnimationClipDragAndDrop( int frameRate )
		{
			_dragAndDropSelectedAnimation = null;

			if( DragAndDrop.objectReferences.Length == 0 )
				return null;

			string pathToAsset = AssetDatabase.GetAssetPath(DragAndDrop.objectReferences[0].GetInstanceID());

			if( string.IsNullOrEmpty(pathToAsset) )
				return null;

			List<AnimationClip> animationClips = new List<AnimationClip>();

	        if( DragAndDrop.objectReferences[0] is AnimationClip && (frameRate >= 0 || Mathf.Approximately( ((AnimationClip)DragAndDrop.objectReferences[0]).frameRate, frameRate)) )
	            animationClips.Add( (AnimationClip)DragAndDrop.objectReferences[0] );
			else
			{
				Object[] objs = AssetDatabase.LoadAllAssetRepresentationsAtPath( pathToAsset );
				
				foreach( Object obj in objs )
				{
					if( obj is AnimationClip && (frameRate >= 0 || Mathf.Approximately( ((AnimationClip)obj).frameRate, frameRate)) )
		            {
		                animationClips.Add( (AnimationClip)obj );
		            }
				}
			}

			if( animationClips.Count == 0 )
				return null;
			else if( animationClips.Count == 1 )
				return animationClips[0];

			GenericMenu menu = new GenericMenu();
			for( int i = 0; i != animationClips.Count; ++i )
			{
				menu.AddItem( new GUIContent(animationClips[i].name), false, SetAnimationClip, animationClips[i] );
			}

			menu.ShowAsContext();
			
			return _dragAndDropSelectedAnimation;
		}

		private static void SetAnimationClip( object data )
		{
			if( data == null )
				_dragAndDropSelectedAnimation = null;
			else
				_dragAndDropSelectedAnimation = (AnimationClip)data;
		}



		private void SetAnimationClip( AnimationClip animClip )
		{
			_animationClip.objectReferenceValue = animClip;
			_animEvent.ControlsAnimation = AssetDatabase.IsSubAsset( animClip ) && AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(animClip)) is AnimatorController;
			_controlsAnimation.boolValue = _animEvent.ControlsAnimation;
	        serializedObject.ApplyModifiedProperties();
		}

		public static void SetAnimationClip( FPlayAnimationEvent animEvent, AnimationClip animClip )
		{
			FAnimationEventInspector editor = (FAnimationEventInspector)CreateEditor( animEvent, typeof( FAnimationEventInspector ) );
			editor.SetAnimationClip( animClip );

			DestroyImmediate( editor );
	        FAnimationTrackInspector.RebuildStateMachine( (FAnimationTrack)animEvent.Track );
		}

		// animation editing
		public static void ScaleAnimationClip( AnimationClip clip, FrameRange range )
		{
	        if( clip == null )
	            return;
			float oldLength = clip.length;
			float newLength = range.Length / clip.frameRate;

			Undo.RecordObject( clip, "resize Animation" );

			EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings( clip );

			// when scaling, we need to adjust the tangents otherwise the curves will get completely distorted
			float tangentMultiplier = oldLength / newLength;

			for( int i = 0; i != curveBindings.Length; ++i )
			{
				AnimationCurve curve = GetAnimationCurve( clip, ref curveBindings[i] );
				if( curve == null )
					continue;

				if( newLength < oldLength )
				{

					for( int j = 0; j < curve.keys.Length; ++j )
					{
//						Keyframe newKeyframe = new Keyframe( Mathf.RoundToInt( (curve.keys[j].time / oldLength) * newLength * clip.frameRate) / clip.frameRate, curve.keys[j].value, curve.keys[j].inTangent*tangentMultiplier, curve.keys[j].outTangent*tangentMultiplier );
						Keyframe newKeyframe = new Keyframe( (curve.keys[j].time / oldLength) * newLength, curve.keys[j].value, curve.keys[j].inTangent*tangentMultiplier, curve.keys[j].outTangent*tangentMultiplier );
						newKeyframe.tangentMode = curve.keys[j].tangentMode;
						curve.MoveKey( j, newKeyframe );
					}
				}
				else
				{
					for( int j = curve.keys.Length-1; j >= 0; --j )
					{
//						Keyframe newKeyframe = new Keyframe( Mathf.RoundToInt( (curve.keys[j].time / oldLength) * newLength * clip.frameRate ) /clip.frameRate, curve.keys[j].value, curve.keys[j].inTangent*tangentMultiplier, curve.keys[j].outTangent*tangentMultiplier );
						Keyframe newKeyframe = new Keyframe( (curve.keys[j].time / oldLength) * newLength, curve.keys[j].value, curve.keys[j].inTangent*tangentMultiplier, curve.keys[j].outTangent*tangentMultiplier );

						newKeyframe.tangentMode = curve.keys[j].tangentMode;
						curve.MoveKey( j, newKeyframe );
					}
				}
				AnimationUtility.SetEditorCurve( clip, curveBindings[i], curve );
			}

			clip.EnsureQuaternionContinuity();
	        EditorApplication.RepaintAnimationWindow();
			EditorUtility.SetDirty( clip );
		}

		private static AnimationCurve GetAnimationCurve( AnimationClip clip, ref EditorCurveBinding curveBinding )
		{
			AnimationCurve curve = null;

			if( curveBinding.propertyName.StartsWith("m_LocalRotation.") )
			{
				char axis = curveBinding.propertyName[curveBinding.propertyName.Length-1];
				if( axis == 'w' )
					return curve; // ignore w
				curveBinding.propertyName = "localEulerAngles." + axis;

				curve = AnimationUtility.GetEditorCurve( clip, curveBinding );

				if( curve == null )
				{
					curveBinding.propertyName = "localEulerAnglesBaked." + axis;

					curve = AnimationUtility.GetEditorCurve( clip, curveBinding );
				}
			}
			else
			{
				curve = AnimationUtility.GetEditorCurve( clip, curveBinding );
			}

			return curve;
		}


		public static AnimationClip CreateAnimationClip( FPlayAnimationEvent animEvent )
		{
			FAnimationTrack track = (FAnimationTrack)animEvent.Track;

			string animName = ((FAnimationTrack)animEvent.Track).LayerName + "_" + animEvent.GetId().ToString();

			AnimationClip clip = AnimatorController.AllocateAnimatorClip( animName );
			clip.frameRate = animEvent.Sequence.FrameRate;

			Transform ownerTransform = animEvent.Owner;

			Vector3 pos = ownerTransform.localPosition;

			Vector3 rot = ownerTransform.localRotation.eulerAngles; //ownerTransform.localEulerAngles;

			Keyframe[] xPosKeys = new Keyframe[]{ new Keyframe(0, pos.x), new Keyframe(animEvent.LengthTime, pos.x) };
			Keyframe[] yPosKeys = new Keyframe[]{ new Keyframe(0, pos.y), new Keyframe(animEvent.LengthTime, pos.y) };
			Keyframe[] zPosKeys = new Keyframe[]{ new Keyframe(0, pos.z), new Keyframe(animEvent.LengthTime, pos.z) };

			Keyframe[] xRotKeys = new Keyframe[]{ new Keyframe(0, rot.x), new Keyframe(animEvent.LengthTime, rot.x) };
			Keyframe[] yRotKeys = new Keyframe[]{ new Keyframe(0, rot.y), new Keyframe(animEvent.LengthTime, rot.y) };
			Keyframe[] zRotKeys = new Keyframe[]{ new Keyframe(0, rot.z), new Keyframe(animEvent.LengthTime, rot.z) };

			// set the tangent mode
			int tangentMode = 10; // 10 is unity auto tangent mode

			for( int i = 0; i != xPosKeys.Length; ++i )
			{
				xPosKeys[i].tangentMode = tangentMode;
				yPosKeys[i].tangentMode = tangentMode;
				zPosKeys[i].tangentMode = tangentMode;

				xRotKeys[i].tangentMode = tangentMode;
				yRotKeys[i].tangentMode = tangentMode;
				zRotKeys[i].tangentMode = tangentMode;
			}

			AnimationCurve xPos = new AnimationCurve( xPosKeys );
			AnimationCurve yPos = new AnimationCurve( yPosKeys );
			AnimationCurve zPos = new AnimationCurve( zPosKeys );

			AnimationCurve xRot = new AnimationCurve( xRotKeys );
			AnimationCurve yRot = new AnimationCurve( yRotKeys );
			AnimationCurve zRot = new AnimationCurve( zRotKeys );

			AnimationUtility.SetEditorCurve( clip, EditorCurveBinding.FloatCurve( string.Empty, typeof(Transform), "m_LocalPosition.x"), xPos );
			AnimationUtility.SetEditorCurve( clip, EditorCurveBinding.FloatCurve( string.Empty, typeof(Transform), "m_LocalPosition.y"), yPos );
			AnimationUtility.SetEditorCurve( clip, EditorCurveBinding.FloatCurve( string.Empty, typeof(Transform), "m_LocalPosition.z"), zPos );

			// workaround unity bug of spilling errors if you just set localEulerAngles
			Quaternion quat = ownerTransform.localRotation;
			Keyframe[] xQuatKeys = new Keyframe[] { new Keyframe(0, quat.x), new Keyframe(animEvent.LengthTime, quat.x) };
			Keyframe[] yQuatKeys = new Keyframe[] { new Keyframe(0, quat.y), new Keyframe(animEvent.LengthTime, quat.y) };
			Keyframe[] zQuatKeys = new Keyframe[] { new Keyframe(0, quat.z), new Keyframe(animEvent.LengthTime, quat.z) };
			Keyframe[] wQuatKeys = new Keyframe[] { new Keyframe(0, quat.w), new Keyframe(animEvent.LengthTime, quat.w) };
			AnimationCurve xQuat = new AnimationCurve( xQuatKeys );
			AnimationCurve yQuat = new AnimationCurve( yQuatKeys );
			AnimationCurve zQuat = new AnimationCurve( zQuatKeys );
			AnimationCurve wQuat = new AnimationCurve( wQuatKeys );
			AnimationUtility.SetEditorCurve( clip, EditorCurveBinding.FloatCurve( string.Empty, typeof(Transform), "m_LocalRotation.x"), xQuat );
			AnimationUtility.SetEditorCurve( clip, EditorCurveBinding.FloatCurve( string.Empty, typeof(Transform), "m_LocalRotation.y"), yQuat );
			AnimationUtility.SetEditorCurve( clip, EditorCurveBinding.FloatCurve( string.Empty, typeof(Transform), "m_LocalRotation.z"), zQuat );
			AnimationUtility.SetEditorCurve( clip, EditorCurveBinding.FloatCurve( string.Empty, typeof(Transform), "m_LocalRotation.w"), wQuat );

			AnimationUtility.SetEditorCurve( clip, EditorCurveBinding.FloatCurve( string.Empty, typeof(Transform), "localEulerAngles.x"), xRot );
			AnimationUtility.SetEditorCurve( clip, EditorCurveBinding.FloatCurve( string.Empty, typeof(Transform), "localEulerAngles.y"), yRot );
			AnimationUtility.SetEditorCurve( clip, EditorCurveBinding.FloatCurve( string.Empty, typeof(Transform), "localEulerAngles.z"), zRot );

			clip.EnsureQuaternionContinuity();

			AssetDatabase.AddObjectToAsset( clip, track.AnimatorController );

			AnimationClipSettings clipSettings = AnimationUtility.GetAnimationClipSettings( clip );
			clipSettings.loopTime = false;
			AnimationUtility.SetAnimationClipSettings( clip, clipSettings );

			AssetDatabase.SaveAssets();

			FAnimationEventInspector.SetAnimationClip( animEvent, clip );

			return clip;
		}
	}
}
