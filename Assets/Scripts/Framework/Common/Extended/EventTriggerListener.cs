using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

#region EnumEventSystemEventType

public enum EnumEventSystemEventType
{
    OnClick,
    OnDoubleClick,
    OnDown,
    OnEnter,
    OnExit,
    OnUp,
    OnSelect,
    OnUpdateSelect,
    OnDeSelect,
    OnDrag,
    OnDragBegin,
    OnDragEnd,
    OnDrop,
    OnScroll,
    OnMove
}

#endregion

public delegate void EventSystemEventHandler(GameObject listener, object eventData, params object[] _params);

public class EventSystemHandler
{
    private event EventSystemEventHandler eventSystemEvent = null;

    private object[] eventParams;

    public EventSystemHandler()
    {

    }

    public EventSystemHandler(EventSystemEventHandler _event, params object[] _params)
    {
        SetListener(_event, _params);
    }

    public void SetListener(EventSystemEventHandler _event, params object[] _params)
    {
        RemoveListener();
        eventSystemEvent += _event;
        eventParams = _params;
    }

    public void CallBackEvent(GameObject _listener, object eventData)
    {
        if (null != eventSystemEvent)
        {
            eventSystemEvent(_listener, eventData, eventParams);
        }
    }

    public void RemoveListener()
    {
        if (eventSystemEvent != null)
        {
            eventSystemEvent -= eventSystemEvent;
            eventSystemEvent = null;
        }
        eventParams = null;
    }


}

public class EventTriggerListener : MonoBehaviour,
    IPointerClickHandler,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerEnterHandler,
    IPointerExitHandler,

    ISelectHandler,
    IUpdateSelectedHandler,
    IDeselectHandler,

    IDragHandler,
    IBeginDragHandler,
    IEndDragHandler,
    IDropHandler,
    IScrollHandler,
    IMoveHandler
{

    public EventSystemHandler onClick;
    public EventSystemHandler onDoubleClick;
    public EventSystemHandler onUp;
    public EventSystemHandler onDown;
    public EventSystemHandler onEnter;
    public EventSystemHandler onExit;

    public EventSystemHandler onSelect;
    public EventSystemHandler onUpdateSelect;
    public EventSystemHandler onDeSelect;

    public EventSystemHandler onDrag;
    public EventSystemHandler onDragBegin;
    public EventSystemHandler onDragEnd;
    public EventSystemHandler onDrop;
    public EventSystemHandler onScroll;
    public EventSystemHandler onMove;

    #region DoubleClick 

    private static readonly float doubleClickDuration = 0.5f;
    private float timer = 0;
    private bool firstClick = false;

    void Update()
    {
        timer += Time.deltaTime;
    }

    #endregion

    #region Interface Implementation

    public void OnPointerClick(PointerEventData eventData)
    {
        if (onClick != null)
            onClick.CallBackEvent(this.gameObject, eventData);



        if (onDoubleClick != null && timer < doubleClickDuration && !firstClick)
        {
            onDoubleClick.CallBackEvent(this.gameObject, eventData);
            firstClick = true;
        }
        else
        {
            firstClick = false;
        }



        timer = 0;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (onDown != null)
            onDown.CallBackEvent(this.gameObject, eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (onUp != null)
            onUp.CallBackEvent(this.gameObject, eventData);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (onEnter != null)
            onEnter.CallBackEvent(this.gameObject, eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (onExit != null)
            onExit.CallBackEvent(this.gameObject, eventData);
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (onSelect != null)
            onSelect.CallBackEvent(this.gameObject, eventData);
    }

    public void OnUpdateSelected(BaseEventData eventData)
    {
        if (onUpdateSelect != null)
            onUpdateSelect.CallBackEvent(this.gameObject, eventData);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (onDeSelect != null)
            onDeSelect.CallBackEvent(this.gameObject, eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (onDrag != null)
            onDrag.CallBackEvent(this.gameObject, eventData);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (onDragBegin != null)
            onDragBegin.CallBackEvent(this.gameObject, eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (onDragEnd != null)
            onDragEnd.CallBackEvent(this.gameObject, eventData);
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (onDrop != null)
            onDrop.CallBackEvent(this.gameObject, eventData);
    }

    public void OnScroll(PointerEventData eventData)
    {
        if (onScroll != null)
            onScroll.CallBackEvent(this.gameObject, eventData);
    }

    public void OnMove(AxisEventData eventData)
    {
        if (onMove != null)
            onMove.CallBackEvent(this.gameObject, eventData);
    }

    #endregion

    public static EventTriggerListener Get(GameObject go)
    {
        return go.GetOrAddComponent<EventTriggerListener>();
    }

    void OnDestory()
    {
        this.RemoveAllListener();
    }

    private void RemoveAllListener()
    {
        this.RemoveListener(onClick);
        this.RemoveListener(onDoubleClick);
        this.RemoveListener(onDown);
        this.RemoveListener(onEnter);
        this.RemoveListener(onExit);
        this.RemoveListener(onUp);
        this.RemoveListener(onDrop);
        this.RemoveListener(onDrag);
        this.RemoveListener(onDragBegin);
        this.RemoveListener(onDragEnd);
        this.RemoveListener(onScroll);
        this.RemoveListener(onMove);
        this.RemoveListener(onUpdateSelect);
        this.RemoveListener(onSelect);
        this.RemoveListener(onDeSelect);
    }

    private void RemoveListener(EventSystemHandler _event)
    {
        if (null != _event)
        {
            _event.RemoveListener();
            _event = null;
        }
    }

    public void AddListener(EnumEventSystemEventType _type, EventSystemEventHandler _event, params object[] _params)
    {
        EventSystemHandler handler = GetHandleByEventType(_type, true);
        if (handler == null)
        {
            handler = new EventSystemHandler();

        }
        handler.SetListener(_event, _params);
    }

    public void RemoveListenerByEventType(EnumEventSystemEventType _type)
    {
        EventSystemHandler handler = GetHandleByEventType(_type);
        if (handler != null)
        {
            handler.RemoveListener();
        }
    }

    public void RemoveAllListenerAllType()
    {
        RemoveAllListener();
    }

    private EventSystemHandler GetHandleByEventType(EnumEventSystemEventType _type, bool bInstrantiate = false)
    {
        EventSystemHandler handler = null;

        switch (_type)
        {
            case EnumEventSystemEventType.OnClick:
                    {
                        if (bInstrantiate && onClick == null)
                            onClick = new EventSystemHandler();
                        handler = onClick;
                    }
                break;
            case EnumEventSystemEventType.OnDoubleClick:
                {
                    if (bInstrantiate && onDoubleClick == null)
                        onDoubleClick = new EventSystemHandler();
                    handler = onDoubleClick;
                }
                break;
            case EnumEventSystemEventType.OnDown:
                {
                    if (bInstrantiate && onDown == null)
                        onDown = new EventSystemHandler();
                    handler = onDown;
                }
                break;
            case EnumEventSystemEventType.OnEnter:
                {
                    if (bInstrantiate && onEnter == null)
                        onEnter = new EventSystemHandler();
                    handler = onEnter;
                }
                break;
            case EnumEventSystemEventType.OnExit:
                {
                    if (bInstrantiate && onExit == null)
                        onExit = new EventSystemHandler();
                    handler = onExit;
                }
                break;
            case EnumEventSystemEventType.OnUp:
                {
                    if (bInstrantiate && onUp == null)
                        onUp = new EventSystemHandler();
                    handler = onUp;
                }
                break;
            case EnumEventSystemEventType.OnSelect:
                {
                    if (bInstrantiate && onSelect == null)
                        onSelect = new EventSystemHandler();
                    handler = onSelect;
                }
                break;
            case EnumEventSystemEventType.OnUpdateSelect:
                {
                    if (bInstrantiate && onUpdateSelect == null)
                        onUpdateSelect = new EventSystemHandler();
                    handler = onUpdateSelect;
                }
                break;
            case EnumEventSystemEventType.OnDeSelect:
                {
                    if (bInstrantiate && onDeSelect == null)
                        onDeSelect = new EventSystemHandler();
                    handler = onDeSelect;
                }
                break;
            case EnumEventSystemEventType.OnDrag:
                {
                    if (bInstrantiate && onDrag == null)
                        onDrag = new EventSystemHandler();
                    handler = onDrag;
                }
                break;
            case EnumEventSystemEventType.OnDragBegin:
                {
                    if (bInstrantiate && onDragBegin == null)
                        onDragBegin = new EventSystemHandler();
                    handler = onDragBegin;
                }
                break;
            case EnumEventSystemEventType.OnDragEnd:
                {
                    if (bInstrantiate && onDragEnd == null)
                        onDragEnd = new EventSystemHandler();
                    handler = onDragEnd;
                }
                break;
            case EnumEventSystemEventType.OnDrop:
                {
                    if (bInstrantiate && onDrop == null)
                        onDrop = new EventSystemHandler();
                    handler = onDrop;
                }
                break;
            case EnumEventSystemEventType.OnScroll:
                {
                    if (bInstrantiate && onScroll == null)
                        onScroll = new EventSystemHandler();
                    handler = onScroll;
                }
                break;
            case EnumEventSystemEventType.OnMove:
                {
                    if (bInstrantiate && onMove == null)
                        onMove = new EventSystemHandler();
                    handler = onMove;
                }
                break;
        }

        return handler;
    }

    internal static void Get(object p)
    {
        throw new NotImplementedException();
    }
}
