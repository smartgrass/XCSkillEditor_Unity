namespace FluxEditor
{
	public interface ISelectableElement
	{
		void OnSelect();
		void OnDeselect();

		bool IsSelected { get; }

		UnityEngine.Rect Rect { get; set; }
	}
}
