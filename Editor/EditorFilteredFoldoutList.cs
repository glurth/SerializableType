using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// This Unity-editor class can be used to allow the user to select an element from a very long list.  
/// Includes a "filter" field to let the user narrow down the list.
/// It will draw the field and handle user input.  It is intended to be instantiated and used by CustomPropertyDrawers.
/// </summary>
public class EditorFilteredFoldoutList
{
    public EditorFilteredFoldoutList(string controlID)
    {
        idString = controlID;
        filterControlName = "FilterText" + idString;
        scrollControlName = "Scroll" + idString;
        scrollOpen = false;
    }
    public EditorFilteredFoldoutList(string controlID, string startingFilterText)
    {
       // Debug.Log("Creating new EditorFilteredFoldoutList: " + controlID);
        idString = controlID;
        filterControlName = "FilterText" + idString;
        scrollControlName = "Scroll" + idString;
        if(startingFilterText!=null)
            filterDisplayText = startingFilterText;
        scrollOpen = false;
    }

    string idString;


    List<GUIContent> fullListRef=null;
    List<GUIContent> displayList = new List<GUIContent>();
    bool recomputeDisplayList = true;
    string filterControlName = null;
    string scrollControlName = null;
    float scrollMaxHeight = 300;
    public void SetText(string newText)
    {
        // Debug.Log("SetText called.  current filtertext: '" + currentFilterText + "'" + "    new filtertext: '" + newText + "'");
        if (filterDisplayText != newText)
            recomputeDisplayList = true;
        filterDisplayText = newText;
    }

    string filterDisplayText;
    bool scrollOpen=false;
    /// <summary>
    /// index into the filtered list, of the current filter text.  will be -1 if the filter text does not match any items on the list
    /// </summary>
    int scrollSelectionIndex;
    Vector2 scrollPos=Vector2.zero;

    /// <summary>
    /// Main function called from property drawer- will render the controls and check for events.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="fullList"></param>
    /// <param name="label"></param>
    /// <returns>the index, in fullList, of the selected item. -1 if nothing selected</returns>
    public int Draw(Rect position, List<GUIContent> fullList, GUIContent label = null)
    {
        if (fullListRef != fullList)
        {
            fullListRef = fullList;
            recomputeDisplayList = true;
        }
        string focusedControlName = GUI.GetNameOfFocusedControl();

        //draw filter field
        Rect filterRect = position;
        filterRect.height = EditorGUIUtility.singleLineHeight;

        if (label == null) label = new GUIContent("Filter");
        //catch events the textfield will use
        if (focusedControlName == filterControlName)
        {
            HandleFilterFieldEvents(filterRect);//checks for downkey and upkey to open up list and or move selected element
        }

        string newFilter = "";
        //Debug.Log("filter field text :" + filterDisplayText);
        EditorGUI.BeginChangeCheck();
        GUI.SetNextControlName(filterControlName);
        newFilter = EditorGUI.TextField(filterRect, label, filterDisplayText);

        bool userChangeDetected = EditorGUI.EndChangeCheck();// (newFilter != filterDisplayText);
        recomputeDisplayList |= userChangeDetected;  //force recompute if text changed by user
        //check for need to recompute filter list
        if (recomputeDisplayList) 
        {
            recomputeDisplayList = false;

            filterDisplayText = newFilter;
            RecomputeFilteredDisplayList();
            //find index of filterText in filtered List, if any
            int foundIndex = displayList.FindIndex((x) => { return x.text == filterDisplayText; });
            if (foundIndex != -1)
            {
                scrollSelectionIndex = foundIndex;
            }
        }
        if (userChangeDetected)
            scrollOpen = true;


        //draw scroll area, if open
        
        Rect scrollRect = position;
        scrollRect.yMin += filterRect.height + EditorGUIUtility.standardVerticalSpacing;//make room for filter
        scrollRect.height = scrollMaxHeight;        
        if (scrollOpen)
        {
          //  Debug.Log("scroll open on id:" + idString + " focused control: " + focusedControlName);
            DrawList(scrollRect);
            HandleListEvents(scrollRect);
            if (focusedControlName == scrollControlName)// || focusedControlName == filterDisplayText)
                HandleKeyEvents();
        }

        if (!string.IsNullOrEmpty(setFocusControlName))
        {
            GUI.FocusControl(setFocusControlName);
            setFocusControlName = null;
        }

        return fullList.FindIndex((x) => { return x.text == filterDisplayText; });
    }

    void RecomputeFilteredDisplayList()
    {
        string filterStringToUse = "*";
        Debug.Log("recomputing list. filter is: " + filterDisplayText);
        if (fullListRef.FindIndex((x) => { return x.text == filterDisplayText; }) != -1)
        {
            Debug.Log("filter found in fullList");
           
            displayList = fullListRef;
            return;
        }
        filterStringToUse = filterDisplayText;
        if (filterStringToUse == "" || filterStringToUse == "*" || filterStringToUse == string.Empty)
        {
            displayList = fullListRef;
        }
        else
        {
            Debug.Log("filtering now");
            string filterAsLowercase = filterStringToUse.ToLower();
            int lastStartsWidthIndex = 0;
            displayList = new List<GUIContent>();
            foreach (GUIContent listElement in fullListRef)
            {
                string sLower = listElement.text.ToLower();
                if (sLower.Contains(filterAsLowercase))
                {
                    if (sLower.StartsWith(filterAsLowercase))
                        displayList.Insert(lastStartsWidthIndex++, listElement);
                    else
                        displayList.Add(listElement);
                }
            }
        }

    }
   
    string setFocusControlName=null;
    void SetFocusScroll() { }// setFocusControlName = scrollControlName; }// GUI.FocusControl(setFocusControlName); }
    void SetFocusFilter() { }// setFocusControlName = filterControlName; }// GUI.FocusControl(setFocusControlName); }

    float doubleClickTimer;
    
    /// <summary>
    /// checks input events and will set scrollOpen, and scrollSelectionIndex as appropriate
    /// </summary>
    /// <param name="pos"></param>
    void HandleFilterFieldEvents(Rect pos)
    {


        bool mouseOver = pos.Contains(Event.current.mousePosition);
        // Debug.Log("Filter rect: " + pos + " mouse pos: " + Event.current.mousePosition + "   mouseover: " + mouseOver + " event:" + Event.current.type);
        if (mouseOver && Event.current.type == EventType.ScrollWheel)
        {
            if(!scrollOpen)
                EnsureIndexInView(scrollSelectionIndex);
            scrollOpen = true;
            SetFocusScroll();
        }
        bool doubleClickDetected = false;
        if (mouseOver && Event.current.type == EventType.MouseUp)//Event.current.isMouse)
        {
            //Debug.Log("Checking double click at :" + Time.realtimeSinceStartup + "  lastClickTime:" + doubleClickTimer);
            if (Time.realtimeSinceStartup - doubleClickTimer < 0.4f)
            {
                doubleClickDetected = true;
            }
            doubleClickTimer = Time.realtimeSinceStartup;
            if (doubleClickDetected)
            {
                // Debug.Log("Double click at :" + Time.realtimeSinceStartup + "  lastClickTime:" + doubleClickTimer);
                if (!scrollOpen)
                {
                    EnsureIndexInView(scrollSelectionIndex);
                    SetFocusScroll();
                }
                scrollOpen = !scrollOpen;
            }
        }
        HandleKeyEvents();
    }
    void HandleListEvents(Rect pos)
    {

        bool mouseOver = pos.Contains(Event.current.mousePosition);
        if (mouseOver && Event.current.type == EventType.ScrollWheel)
        {
            scrollPos.y += Event.current.delta.y * EditorGUIUtility.singleLineHeight;
            scrollPos.y = Mathf.Clamp(scrollPos.y, 0, displayList.Count * EditorGUIUtility.singleLineHeight - pos.height);
            Event.current.Use();
        }
        bool doubleClickDetected = false;
        if (mouseOver && Event.current.type == EventType.MouseUp)//Event.current.isMouse)
        {
           // Debug.Log("Checking double click at :" + Time.realtimeSinceStartup + "  lastClickTime:" + doubleClickTimer);
            if (Time.realtimeSinceStartup - doubleClickTimer < 0.4f)
            {
                doubleClickDetected = true;
            }
            doubleClickTimer = Time.realtimeSinceStartup;
            Event.current.Use();

            Vector2 mousePos = Event.current.mousePosition - pos.position; // position inside drawRect
            mousePos += scrollPos;// position inside scrollRect
            int clickedElement = (int)(mousePos.y / EditorGUIUtility.singleLineHeight);
            if (clickedElement < displayList.Count)
            {
                // Debug.Log("click element " + clickedElement + " value: " + fiteredListConents[clickedElement]);
                scrollSelectionIndex = clickedElement;
                filterDisplayText = displayList[clickedElement].text;
            }


            if (doubleClickDetected)
            {
                scrollOpen = !scrollOpen;
                SetFocusFilter();
                EditorWindow.focusedWindow.Repaint();
            }

        }
    }
    void HandleKeyEvents()
    {
        if (Event.current.type == EventType.KeyDown)//Event.current.isKey)// focusedControlAtStart==filterControlName)
        {
         
            if (Event.current.keyCode == KeyCode.UpArrow && scrollSelectionIndex > 0)
            {
                if (scrollOpen)
                {
                    scrollSelectionIndex--;
                    filterDisplayText = displayList[scrollSelectionIndex].text;
                }
                else
                    scrollOpen = true;
                EnsureIndexInView(scrollSelectionIndex);
                Event.current.Use();
                SetFocusScroll();
            }
            if (Event.current.keyCode == KeyCode.DownArrow && scrollSelectionIndex < displayList.Count - 1)
            {
                if (scrollOpen)
                {
                    scrollSelectionIndex++;
                    filterDisplayText = displayList[scrollSelectionIndex].text;
                }
                else
                    scrollOpen = true;
                EnsureIndexInView(scrollSelectionIndex);
                Event.current.Use();
                SetFocusScroll();
            }
            if (Event.current.keyCode == KeyCode.Return && scrollSelectionIndex != -1)
            {
                if (scrollOpen)
                    filterDisplayText = displayList[scrollSelectionIndex].text;
                Event.current.Use();
                scrollOpen = false;
                EditorWindow.focusedWindow.Repaint();
                SetFocusFilter();
            }
            if (Event.current.keyCode == KeyCode.Escape && scrollSelectionIndex != -1)
            {
                scrollOpen = false;
                EditorWindow.focusedWindow.Repaint();
            }
        }

    }

    void DrawList(Rect pos)
    {
        
        Rect listSizeRect = new Rect(Vector2.zero, pos.size);
        listSizeRect.height = displayList.Count * EditorGUIUtility.singleLineHeight;
        if (!scrollOpen) listSizeRect.height = 0;
        Rect insideScrollRect = listSizeRect;
        insideScrollRect.height = EditorGUIUtility.singleLineHeight;

        //Debug.Log("drawing list rect height:" + pos.ToString());
        EditorGUI.DrawRect(pos, Color.white);//background
        GUI.SetNextControlName(scrollControlName);  //SetText the name of the scroll window so we can set focus if needed
        scrollPos = GUI.BeginScrollView(pos, scrollPos, listSizeRect);
        {
            int counter = 0;
            // Calculate how many items are visible in the scroll view
            int visibleItemCount = Mathf.FloorToInt(pos.height / EditorGUIUtility.singleLineHeight) + 1;

            // Compute the first visible index based on the current scroll position
            int firstVisibleIndex = Mathf.FloorToInt((scrollPos.y) / EditorGUIUtility.singleLineHeight);
            firstVisibleIndex = Mathf.Max(firstVisibleIndex, 0);
            int endAt = Mathf.Min(displayList.Count, firstVisibleIndex + visibleItemCount);
            // Start iterating from the first visible index
            for (int i = firstVisibleIndex; i < endAt; i++)
            {
                insideScrollRect.y = (i * EditorGUIUtility.singleLineHeight);
                
                if (i == scrollSelectionIndex)
                    EditorGUI.DrawRect(insideScrollRect, Color.grey);
                if (insideScrollRect.Contains(Event.current.mousePosition))
                    EditorGUI.DrawRect(insideScrollRect, Color.yellow);
                
                DrawStyledLabel(insideScrollRect, displayList[i], filterDisplayText);
                //Debug.Log("drawing list element");
                counter++;
            }
        }
        GUI.EndScrollView();
        EditorWindow.focusedWindow.Repaint();
        return;
    }

    void EnsureIndexInView(int index)
    {
        float indexScrollPos = index * EditorGUIUtility.singleLineHeight;
        int visibleItemCount = Mathf.FloorToInt(scrollMaxHeight / EditorGUIUtility.singleLineHeight);
        float height = (visibleItemCount - 1) * EditorGUIUtility.singleLineHeight;
        float scrollMinInView = indexScrollPos - height;
        float scrollMaxInView = indexScrollPos;// + height;
//        Debug.Log("indexScrollPos: " + indexScrollPos);
//        Debug.Log("scrollMinInView: " + scrollMinInView + "   scrollPos.y:" + scrollPos.y);
//        Debug.Log("scrollMaxInView: " + scrollMaxInView + "   scrollPos.y:" + scrollPos.y);
        if (scrollPos.y < scrollMinInView)
            scrollPos.y = scrollMinInView;
        if (scrollPos.y > scrollMaxInView)
            scrollPos.y = scrollMaxInView;
    }

    public float GetHeight()
    {
        if (scrollOpen)
            return scrollMaxHeight + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        return EditorGUIUtility.singleLineHeight;
    }
    static void DrawStyledLabel(Rect rect, GUIContent label, string boldPart)
    {
        if (string.IsNullOrEmpty(label.text) || string.IsNullOrEmpty(boldPart))
        {
            EditorGUI.LabelField(rect, label);
            return;
        }

        int boldStart = label.text.IndexOf(boldPart, System.StringComparison.OrdinalIgnoreCase);
        if (boldStart == -1)
        {
            EditorGUI.LabelField(rect, label); // Draw normally if not found
            return;
        }

        GUIStyle normalStyle = new GUIStyle(EditorStyles.label);
        GUIStyle boldStyle = new GUIStyle(EditorStyles.boldLabel);
        boldStyle.padding.right = 0;
        boldStyle.padding.left = 0;
        normalStyle.padding.right = 0;

        GUIContent prefix = new GUIContent(label);
        GUIContent match = new GUIContent(label);
        GUIContent suffix = new GUIContent(label);

        prefix.text = label.text.Substring(0, boldStart);
        match.text = label.text.Substring(boldStart, boldPart.Length);
        suffix.text = label.text.Substring(boldStart + boldPart.Length);

        Vector2 prefixSize = normalStyle.CalcSize(new GUIContent(prefix));
        Vector2 boldSize = boldStyle.CalcSize(new GUIContent(match));

        EditorGUI.LabelField(new Rect(rect.x, rect.y, prefixSize.x, rect.height), prefix, normalStyle);
        EditorGUI.LabelField(new Rect(rect.x + prefixSize.x, rect.y, boldSize.x, rect.height), match, boldStyle);
        normalStyle.padding.left = 0;
        EditorGUI.LabelField(new Rect(rect.x + prefixSize.x + boldSize.x, rect.y, rect.width, rect.height), suffix, normalStyle);
    }
}
