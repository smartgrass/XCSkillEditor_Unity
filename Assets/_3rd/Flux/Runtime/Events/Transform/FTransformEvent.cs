using UnityEngine;


namespace Flux
{
	/**
	 * @brief Base of transform property changes.
	 * @sa FPositionEvent, FRotationEvent, FScaleEvent
	 */
	public abstract class FTransformEvent : FTweenEvent<FTweenVector3>
	{
	}
}
