using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace FluxEditor
{
	public class FSettings : ScriptableObject {

		[MenuItem("Window/Flux/Create Flux Settings",false, 101)]
		public static void CreateColorSettings()
		{
			string settingsPath = FluxEditor.FUtility.GetFluxEditorPath()+"FluxSettings.asset";

			if( AssetDatabase.LoadMainAssetAtPath( settingsPath ) != null )
			{
				if( !EditorUtility.DisplayDialog("Warning", "Flux Settings already exist, are you sure you want to replace them?", "Replace", "Cancel" ) )
					return;
			}

			FSettings settings = CreateInstance<FSettings>();
			AssetDatabase.CreateAsset( settings, settingsPath );
		}

		[SerializeField]
		private List<FColorSetting> _eventColors = new List<FColorSetting>();
		public List<FColorSetting> EventColors { get { return _eventColors; } }

		private Dictionary<string, FColorSetting> _eventColorsHash = null;

		[SerializeField]
		private List<FColorSetting> _defaultContainers = new List<FColorSetting>();
		public List<FColorSetting> DefaultContainers { get { return _defaultContainers; } }

		public void Init()
		{
			if( _eventColorsHash == null )
				_eventColorsHash = new Dictionary<string, FColorSetting>();
			else
				_eventColorsHash.Clear();

			foreach( FColorSetting colorSetting in _eventColors )
			{
				if( string.IsNullOrEmpty( colorSetting._str ) )
					return;
				
				if( _eventColorsHash.ContainsKey( colorSetting._str ) )
					return; // can't add duplicates!
				
				_eventColorsHash.Add( colorSetting._str, colorSetting );
			}
		}

		public Color GetEventColor( string str )
		{
			if( _eventColorsHash == null )
				Init();
//			Debug.Log ( eventTypeStr );
			FColorSetting c;
			if( !_eventColorsHash.TryGetValue( str, out c ) )
				return FGUI.GetEventColor();
			return c._color;
		}
	}

	[System.Serializable]
	public class FColorSetting
	{
		public string _str;
		public Color _color;

		public FColorSetting( string str, Color color )
		{
			_str = str;
			_color = color;
		}
	}
}
