namespace Utils
{
    using System.Collections.Generic;
    using System;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;
    
    public class NestedScrollRect : ScrollRect
    {
        //上层ScrollRect
        private ScrollRect _upperScroll;

        private ScrollRect upperScroll
        {
            get
            {
                if (!_upperScroll)
                {
                    Transform parent = transform.parent;
                    if (!parent) return null;
                    _upperScroll = parent.GetComponentInParent<ScrollRect>();
                }

                return _upperScroll;
            }
        }

        private bool shouldParentScroll { get; set; }
        private float parentValue { get; set; }
        private float childValue { get; set; }
        public Action OnEndFunc { get; set; } = null;
        public Action OnBeginFunc { get; set; } = null;
        private List<NestedScrollRect> otherBrothers = new ();

        protected override void Awake()
        {
            base.Awake();
            //找到父对象
            Transform parent = transform.parent;
            if (!parent) return;
            foreach (Transform t in parent)
            {
                if (t.GetComponent<NestedScrollRect>() && t!=transform)
                {
                    otherBrothers.Add(t.GetComponent<NestedScrollRect>());
                }
            }
            print(upperScroll);
            if(!upperScroll) return;
            upperScroll.onValueChanged.AddListener(e =>
            {
                parentValue = e.y;
                // print($"子母： {parentValue},{childValue}");
                if (parentValue < 0 && shouldParentScroll)
                {
                    SetShouldParentScroll(false);
                }
            });
            onValueChanged.AddListener(e =>
            {
                childValue = e.y;
                if (childValue >= 1 && !shouldParentScroll)
                {
                    SetShouldParentScroll(true);
                }
            });
            SetShouldParentScroll(true, true);
        }
        
        void SetShouldParentScroll(bool flag, bool isInitial = false)
        {
            print("设置是否允许上层滑动");
            verticalNormalizedPosition = 1;

            if (!isInitial)
            {
                upperScroll.verticalNormalizedPosition = isInitial ? 1 : 0;    
            }
            SetVertical(flag);
            foreach (var b in otherBrothers)
            {
                b.SetVertical(flag);   
            }
        }

        public void SetVertical(bool flag)
        {
            if(!upperScroll) return;
            // print("设置上层scroll" + flag + gameObject.name);
            currentStatus = true;
            shouldParentScroll = flag;
            vertical = !shouldParentScroll;
            upperScroll.vertical = shouldParentScroll;
        }

        public void SetParentEnable(bool flag)
        {
            if(!upperScroll) return;
            upperScroll.enabled = flag;
        }

        private bool currentStatus { get; set; } = false;
        private bool isOnDrag { get; set; } = false;

        public override void OnBeginDrag(PointerEventData eventData)
        {
            // print("设置" + eventData.delta);
            isOnDrag = true;
            OnBeginFunc?.Invoke();
            if (upperScroll)
            {
                if (eventData.delta.y > 0 && upperScroll.verticalNormalizedPosition<=0 && shouldParentScroll)
                {
                    SetShouldParentScroll(false);
                    return;
                }
                if (shouldParentScroll)
                {
                    upperScroll.OnBeginDrag(eventData);
                    return;
                }
            }

            base.OnBeginDrag(eventData);
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (currentStatus)
            {
                print("重置滑动");
                currentStatus = false;
                OnEndDrag(eventData);
                OnBeginDrag(eventData);
                OnDrag(eventData);
                return;
            }

            if (upperScroll)
            {
                if (shouldParentScroll)
                {
                    upperScroll.OnDrag(eventData);
                    return;
                }
            }

            base.OnDrag(eventData);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            isOnDrag = false;
            OnEndFunc?.Invoke();
            if (upperScroll)
            {
                if (shouldParentScroll)
                {
                    upperScroll.OnEndDrag(eventData);
                    return;
                }
            }

            base.OnEndDrag(eventData);
        }

        public override void OnScroll(PointerEventData data)
        {
            print(data.delta);
            if (upperScroll)
            {
                if (shouldParentScroll)
                {
                    upperScroll.OnScroll(data);
                    return;
                }
            }

            base.OnScroll(data);
        }
    }
}