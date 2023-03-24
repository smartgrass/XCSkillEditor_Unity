using UnityEngine;

namespace Flux
{
	//[FEventAttribute("Misc/Comment", typeof(FCommentTrack))]
	public class FCommentEvent : FEvent {

		[SerializeField]
		private string _comment = "!Comment!";
		public override string Text {
			get { return _comment; }
			set { _comment = value; }
		}

		[SerializeField]
		private Color _color = new Color( 0.15f, 0.6f, 0.95f, 0.8f );
		public Color Color { get { return _color; } set { _color = value; } }

		protected override void OnTrigger( float timeSinceTrigger )
		{
			FCommentTrack commentTrack = (FCommentTrack)Track;
			if (commentTrack.Label != null) 
				commentTrack.Label.text = Text;
		}

		protected override void OnFinish ()
		{
			base.OnFinish();
			ClearText();
		}

		protected override void OnStop ()
		{
			base.OnStop();
			ClearText();
		}

		private void ClearText()
		{
			FCommentTrack commentTrack = (FCommentTrack)Track;
			if (commentTrack.Label != null) 
				commentTrack.Label.text = string.Empty;
		}
	}
}