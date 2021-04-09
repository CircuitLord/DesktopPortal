using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(ScrollRect))]
public abstract class PooledScrollRect<T,Y> : MonoBehaviour where Y : PooledScrollRectElement<T>
{
    public float ElementSize;
    public Y Template;

    [SerializeField]
    protected List<T> _scrollListData = new List<T>();
    protected int _totalNumElements
    {
        get
        {
            return _scrollListData.Count;
        }
    }
    protected ScrollRect _scrollRect;

    private LayoutElement _spacer;
    private List<Y> _activeElements = new List<Y>();
    private int _lastCulledAbove = -1;

    private enum RepurposeMethod
    {
        TopGoesToBottom,
        BottomGoesToTop
    }

    void OnEnable()
    {
        if(_scrollRect == null)
            _scrollRect = GetComponent<ScrollRect>();
        if (_spacer == null)
            _spacer = SpawnSpacer();

        _scrollRect.onValueChanged.AddListener(ScrollMoved);

        ScrollMoved(Vector2.zero);
    }

    void OnDisable()
    {
        _scrollRect.onValueChanged.RemoveListener(ScrollMoved);
    }

    void ScrollMoved(Vector2 delta)
    {
        //Set the size of the content container to match the size of all of its 'children'
        AdjustContentSize(ElementSize * _totalNumElements);

        //How many elements can fit into the content area
        float scrollAreaSize = _scrollRect.vertical ? ((RectTransform)_scrollRect.transform).rect.height : ((RectTransform)_scrollRect.transform).rect.width;
        int numElementsVisibleInScrollArea = Mathf.CeilToInt(scrollAreaSize / ElementSize);

        //basically the number of elements culled 'above you', clamped between 0 and  'number of total emenents - number of elements that would fit in the display area' 
        //since there is nothing lower than those last few elements
        int numElementsCulledAbove = Mathf.Clamp(Mathf.FloorToInt(GetScrollRectNormalizedPosition() * (_totalNumElements - numElementsVisibleInScrollArea)), 0, Mathf.Clamp(_totalNumElements - (numElementsVisibleInScrollArea + 1), 0, int.MaxValue));

        //Adjust the spacers width/height and have it fill the empty space that is supposed to be taken up by all the ui elements that are being culled
        AdjustSpacer(numElementsCulledAbove * ElementSize);

        int requiredElementsInList = Mathf.Min((numElementsVisibleInScrollArea + 1), _totalNumElements);
        bool refreshRequired = _activeElements.Count != requiredElementsInList || _lastCulledAbove != numElementsCulledAbove;

        if (refreshRequired)
        {
            if (_activeElements.Count != requiredElementsInList)
            {
                InitializeElements(requiredElementsInList, numElementsCulledAbove);
            }
            else
            {
                RepurposeMethod repurposeMethod = numElementsCulledAbove > _lastCulledAbove ? RepurposeMethod.TopGoesToBottom : RepurposeMethod.BottomGoesToTop;
                RepurposeElement(repurposeMethod, numElementsCulledAbove);
            }
        }

        _lastCulledAbove = numElementsCulledAbove;
    }

    void AdjustContentSize(float size)
    {
        Vector2 currentSize = _scrollRect.content.sizeDelta;

        if (_scrollRect.vertical)
            currentSize.y = size;
        else
            currentSize.x = size;

        _scrollRect.content.sizeDelta = currentSize;
    }

    void AdjustSpacer(float size)
    {

        if (_scrollRect.vertical)
            _spacer.minHeight = size;
        else
            _spacer.minWidth = size;
    }

    float GetScrollRectNormalizedPosition()
    {
        return Mathf.Clamp01(_scrollRect.vertical ? 1 - _scrollRect.verticalNormalizedPosition : _scrollRect.horizontalNormalizedPosition);
    }

    LayoutElement SpawnSpacer()
    {
        var newLayoutElement = (new GameObject("Spacer")).AddComponent<LayoutElement>();

        if (_scrollRect.vertical)
        {
            newLayoutElement.minHeight = 100;
        }
        else
        {
            newLayoutElement.minWidth = 100;
        }

        newLayoutElement.transform.SetParent(_scrollRect.content.transform, false);

        return newLayoutElement;
    }

    void InitializeElements(int requiredElementsInList, int numElementsCulledAbove)
    {
        for (int i = 0; i < _activeElements.Count; i++)
            Destroy(_activeElements[i].gameObject);

        _activeElements.Clear();

        //initialize
        for (int i = 0; i < requiredElementsInList && i + numElementsCulledAbove < _totalNumElements; i++)
        {
            var newelement = Instantiate(Template);
            newelement.transform.SetParent(_scrollRect.content, false);

            newelement.Setup((i + numElementsCulledAbove), _scrollListData);
            _activeElements.Add(newelement);
        }
    }

    void RepurposeElement(RepurposeMethod repurposeMethod, int numElementsCulledAbove)
    {
        if (repurposeMethod == RepurposeMethod.TopGoesToBottom)
        {
            var top = _activeElements[0];
            _activeElements.RemoveAt(0);
            _activeElements.Add(top);
            top.transform.SetSiblingIndex(_activeElements[_activeElements.Count - 2].transform.GetSiblingIndex() + 1);
            top.Setup((numElementsCulledAbove + _activeElements.Count - 1), _scrollListData);
        }
        else
        {
            var bottom = _activeElements[_activeElements.Count - 1];
            _activeElements.RemoveAt(_activeElements.Count - 1);
            _activeElements.Insert(0, bottom);
            bottom.transform.SetSiblingIndex(_activeElements[1].transform.GetSiblingIndex());
            bottom.Setup((numElementsCulledAbove), _scrollListData);
        }
    }

}

public abstract class PooledScrollRectElement<T> : MonoBehaviour
{
    public abstract void Setup(int placement, List<T> allData);
}
