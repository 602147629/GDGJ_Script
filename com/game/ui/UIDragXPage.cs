﻿﻿﻿using UnityEngine;
using System.Collections.Generic;

namespace com.game.ui
{

    public class DragXPostion
    {
        public float x;
        public bool moved;
    }

    //[AddComponentMenu("NGUI/Interaction/Drag XPage")]
    public class UIDragXPage : MonoBehaviour
    {
        public float startPageX;
        public int pageSize;
        public Transform target;
        public Vector3 scale = Vector3.one;
        public string page_moved_name = "OnTurnPage";
        public string x_postion_notify_name = "OnDragLastMoveX";
        public string open_all_page = "OpenAllPage";
        private Plane mPlane;
        private Vector3 mLastPos;
        private float leftEdge;
        private float rightEdge;
        private float lastLocalPostionX;
        private float pressLastTime;

        private UIPageController pageController;

        [SerializeField]
        private bool mLoop;
        public bool Loop
        {
            get { return mLoop; }
            set { mLoop = value; }
        }


        public int GetPageSize()
        {
            return pageSize;
        }

        public int GetPageCount()
        {
            return pageController.pageCount;
        }

        void Start()
        {
            pageController = NGUITools.FindInParents<UIPageController>(gameObject);

        }

        private void SetEdge()
        {
            leftEdge = startPageX - pageSize * (pageController.pageCount - 1);
            rightEdge = startPageX;
        }

        //	public void SetPageInformation(Transform root,int perPageSize,int pageTotalCount)
        //	{
        //		pageSize = perPageSize;
        //		pageController.pageCount = pageTotalCount;
        //		target = root;
        //		SetEdge();
        //	}

        private void stopTarget()
        {
            SpringPosition sp = target.GetComponent<SpringPosition>();
            if (sp != null)
                sp.enabled = false;
            DragXPostion xData = new DragXPostion();
            xData.moved = false;
            target.SendMessage(x_postion_notify_name, xData, SendMessageOptions.DontRequireReceiver);
        }

        private void moveTarget(Vector3 dest)
        {
            SpringPosition sp = SpringPosition.Begin(target.gameObject, dest, 5f);
            sp.ignoreTimeScale = true;
            sp.worldSpace = false;
            DragXPostion xData = new DragXPostion();
            xData.moved = true;
            xData.x = dest.x;
            target.SendMessage(x_postion_notify_name, xData, SendMessageOptions.DontRequireReceiver);
        }

        private int getCurrentPage()
        {
            return (int)((target.transform.localPosition.x - startPageX) / pageSize);
        }

        private void sendPage(int page)
        {
            target.SendMessage(page_moved_name, page, SendMessageOptions.DontRequireReceiver);
        }

        private void sendOpenAllPage()
        {
            target.SendMessage(open_all_page, SendMessageOptions.DontRequireReceiver);
        }

        void OnTurnToPage(int page)
        {
            if (page >= 0 && page < pageController.pageCount)
            {
                stopTarget();
                Vector3 localPostion = target.transform.localPosition;
                localPostion.x = startPageX - pageSize * page;
                moveTarget(localPostion);
            }
        }

        private float getDestX(int page)
        {
            return startPageX - pageSize * page;
        }

        void OnImmediatelyTurnToPage(int page)
        {
            if (page >= 0 && page < pageController.pageCount)
            {
                stopTarget();
                Vector3 localPostion = target.transform.localPosition;
                localPostion.x = getDestX(page);
                target.transform.localPosition = localPostion;
            }
        }

        void Update()
        {
            SetEdge();
            if (mLoop && pageSize > 0 && pageController.pageCount >= 2)
            {

                UIPageController pageCtrl = target.gameObject.GetComponent<UIPageController>();
                List<GameObject> pages = pageCtrl.GetAllPages();
                float pageY = pages[0].transform.localPosition.y;
                float pageZ = pages[0].transform.localPosition.z;

                pages[0].transform.localPosition = new Vector3(0, pageY, pageZ);
                pages[pageController.pageCount - 1].transform.localPosition = new Vector3(pageSize * (pageController.pageCount - 1), pageY, pageZ);

                Vector3 localPostion = target.transform.localPosition;

                if ((localPostion.x < pageSize) && (localPostion.x > 0))
                {
                    pages[pageController.pageCount - 1].transform.localPosition = new Vector3(-pageSize, pageY, pageZ);
                }


                float fPageToalSize = -pageSize * (pageController.pageCount - 1);

                if (localPostion.x <= fPageToalSize)
                {
                    pages[0].transform.localPosition = new Vector3(pageSize * pageController.pageCount, pageY, pageZ);
                }
            }

        }

        void OnPress(bool pressed)
        {
            UIPageController PageControl = target.gameObject.GetComponent<UIPageController>();
            List<GameObject> pages = PageControl.GetAllPages();
            float pageY = pages[0].transform.localPosition.y;
            float pageZ = pages[0].transform.localPosition.z;

            if (mLoop)
            {
                pages[0].transform.localPosition = new Vector3(0, pageY, pageZ);
                pages[11].transform.localPosition = new Vector3(pageSize * (pageController.pageCount - 1), pageY, pageZ);
            }

            if (enabled && gameObject.active && target != null && pageSize > 0 && pageController.pageCount >= 1)
            {
                if (pressed)
                {
                    sendOpenAllPage();
                    stopTarget();
                    // Remember the hit position
                    mLastPos = UICamera.lastHit.point;
                    pressLastTime = Time.timeSinceLevelLoad;
                    lastLocalPostionX = target.transform.localPosition.x;
                    // Create the plane to drag along
                    Transform trans = UICamera.currentCamera.transform;
                    mPlane = new Plane(trans.rotation * Vector3.back, mLastPos);

                }
                else
                {
                    Vector3 localPostion = target.transform.localPosition;
                    int dstPage = 0;
                    if (localPostion.x <= leftEdge)
                    {
                        //Turn right
                        localPostion.x = leftEdge;
                        dstPage = pageController.pageCount - 1;

                        if (mLoop)
                        {

                            float xTransSincePress = target.transform.localPosition.x - lastLocalPostionX;
                            float timeSincePress = Time.timeSinceLevelLoad - pressLastTime;
                            float xSpeed = xTransSincePress / timeSincePress;

                            if (Mathf.Abs(xSpeed) > 200)
                            {

                                float offX = target.transform.localPosition.x + pageSize * (pageController.pageCount - 1);
                                dstPage = 0;
                                localPostion.x = getDestX(dstPage);

                                float y = target.transform.localPosition.y;
                                float z = target.transform.localPosition.z;
                                target.transform.localPosition = new Vector3(localPostion.x + pageSize + offX, y, z);

                                pages[pageController.pageCount - 1].transform.localPosition = new Vector3(-pageSize, pageY, pageZ);
                            }
                        }

                    }
                    else if (localPostion.x >= rightEdge)
                    {
                        //Turn Left
                        localPostion.x = rightEdge;
                        dstPage = 0;

                        if (mLoop)
                        {

                            float xTransSincePress = target.transform.localPosition.x - lastLocalPostionX;
                            float timeSincePress = Time.timeSinceLevelLoad - pressLastTime;
                            float xSpeed = xTransSincePress / timeSincePress;

                            if (Mathf.Abs(xSpeed) > 200)
                            {


                                float offX = target.transform.localPosition.x;
                                dstPage = GetPageCount() - 1;
                                localPostion.x = getDestX(dstPage);

                                float y = target.transform.localPosition.y;
                                float z = target.transform.localPosition.z;
                                target.transform.localPosition = new Vector3(localPostion.x - pageSize + offX, y, z);

                                pages[0].transform.localPosition = new Vector3(pageSize * pageController.pageCount, pageY, pageZ);

                            }

                        }

                    }
                    else
                    {
                        float xTransSincePress = localPostion.x - lastLocalPostionX;
                        float timeSincePress = Time.timeSinceLevelLoad - pressLastTime;
                        float xSpeed = xTransSincePress / timeSincePress;

                        float transX = (localPostion.x - startPageX) / pageSize;
                        if (Mathf.Abs(xSpeed) > 200)
                        {
                            if (xSpeed > 0)
                            {
                                transX = Mathf.Ceil(transX);
                            }
                            else
                            {
                                transX = Mathf.Floor(transX);
                            }
                        }
                        int turnPage = Mathf.RoundToInt(transX);
                        dstPage = -turnPage;
                        localPostion.x = getDestX(dstPage);
                    }
                    sendPage(dstPage);
                    moveTarget(localPostion);
                }
            }
        }

        void OnDrag(Vector2 delta)
        {
            if (enabled && gameObject.active && target != null && pageSize > 0 && pageController.pageCount >= 1)
            {
                Ray ray = UICamera.currentCamera.ScreenPointToRay(UICamera.lastTouchPosition);
                float dist = 0f;
                if (mPlane.Raycast(ray, out dist))
                {
                    Vector3 currentPos = ray.GetPoint(dist);
                    Vector3 offset = currentPos - mLastPos;
                    mLastPos = currentPos;
                    if (offset.x != 0f || offset.y != 0f)
                    {
                        offset = target.InverseTransformDirection(offset);
                        offset.Scale(scale);
                        offset = target.TransformDirection(offset);
                    }
                    // Adjust the position
                    target.position += offset;
                }
            }
        }

    }

}