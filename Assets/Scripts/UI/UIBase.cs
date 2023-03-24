using System.Collections;
using UnityEngine;

namespace XiaoCao
{
    public class UIBase : MonoBehaviour
    {
        [HideInInspector]
        public Canvas canvas;
        [HideInInspector]
        public RectTransform canvasRect;
        [HideInInspector]
        public bool IsCanvasInited;

        private RectTransform rect;
        public RectTransform Rect
        {
            get
            {
                if (rect == null)
                {
                    rect = transform as RectTransform;
                }
                return rect;
            }
        }

        private Camera mainCam;
        public Camera MainCam
        {
            get
            {
                if(mainCam == null)
                {
                    mainCam = Camera.main;
                }
                return mainCam;
            }
            set
            {
                mainCam = value;
            }
        }


        public void InitCanvas(Canvas canvas)
        {
            this.canvas = canvas;
            canvasRect = canvas.transform as RectTransform;
            IsCanvasInited = true;
        }

        public virtual void Show()
        {

        }
        public virtual void Hide()
        {

        }

    }
}