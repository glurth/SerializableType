using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// This Unity-editor class can be used to allow the user to select an element from a very long list.  Includes a "filter" field to let the user narrow down the list.
/// It will draw the field and handle user input.  It is intended to be instantiated and used by CustomPropertyDrawers.
/// </summary>
public class EditorPopupWithTextFilter
{

    //-1 indicates no element is selected, otherwise the value represents an index into the string list member "containing"
    int selectedElement = -1;
    // the text inside the userEditable TextField last time we looked
    string currentFilterText="g5948uiorthdfs03454ygfrf56ERREF";
    //set to true when the programmer calls SetText.  Refilters list when changed
    bool textChanged = false;
    //recorded internally used to make draw & selection decisions
    bool textHadFocusLastPass = false;
    //recorded internally used to make draw & selection decisions
    bool hasFocus = false;

    bool scrollPoppedOut = false;

    //the filtered list displayed in the drop down
    List<string> fiteredListConents = new List<string>();

    //placement and size of the draw popup window
    Rect popupPosition;
    // computed currently drawn height of the control (with/without dropdown expanded), summed internally- provided to user via GetHeight
    float drawnHeight = 0;
    // current scroll position of list, only y used
    Vector2 scrollPos = Vector2.zero;

    string _filterControlName = null;
    string _nextControlName = null;

    string filterControlName
    {
        get
        {
            if (_filterControlName == null)
                _filterControlName = "FilterText" + System.Guid.NewGuid().ToString();
            return _filterControlName;
        }
    }
    string nextControlName
    {
        get
        {
            if (_nextControlName == null)
                _nextControlName = "Next" + System.Guid.NewGuid().ToString();
            return _nextControlName;
        }
    }
       

    //user adjustable value- hight of dropdown. default 300
    public float popupMaxHeight = 300f;
    // the text inside the userEditable TextField last time we looked
    public string currentTextOutput => currentFilterText;
    //  change the filter text programatically
    public void SetText(string newText)
    {
        // Debug.Log("SetText called.  current filtertext: '" + currentFilterText + "'" + "    new filtertext: '" + newText + "'");
        if (currentFilterText != newText)
            textChanged = true;
        currentFilterText = newText;
    }

    /// <summary>
    /// get the summed height of the entire control  (with/without dropdown expanded)
    /// </summary>
    /// <returns></returns>
    public float GetHeight()
    {
        return drawnHeight;
    }

    /// <summary>
    /// Public Function called by user to draw the filter/selection text control, and if appropriate, popout list.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="fullList"></param>
    /// <param name="label"></param>
    /// <returns></returns>
    public int Draw(Rect position, List<string> fullList, GUIContent label = null)
    {

        string focusedControlName = GUI.GetNameOfFocusedControl();
        bool hadFocus = hasFocus;
        bool filterControlHasFocus = focusedControlName == filterControlName;
        bool scrollControlHasFocus = focusedControlName == nextControlName;
        //Debug.Log("focuses  filter:" + filterControlHasFocus + " scroll: " + scrollControlHasFocus + " focus name: "+ focusedControlName);
        hasFocus = filterControlHasFocus || scrollControlHasFocus;
        if (hadFocus && !hasFocus)  //if we just LOST focus
        {
            scrollPoppedOut = false;
            HandleUtility.Repaint(); // Repaints the control
        }
        if (hasFocus && Event.current.type == EventType.KeyDown)//check for key events now before consumed
        {
            //Debug.Log("keydown while filter focused");
            scrollPoppedOut = true;
        }

        bool mouseOverControl = position.Contains(Event.current.mousePosition);
        // Debug.Log($"Draw function Detected event: {Event.current.type}");
        position.height = EditorGUIUtility.singleLineHeight;
        if (label == null) label = new GUIContent("Filter");

        GUI.SetNextControlName(filterControlName);
        string newFilter = EditorGUI.TextField(position, label, currentFilterText);

        scrollPoppedOut &= hasFocus;
        if (mouseOverControl && Event.current.type == EventType.ScrollWheel)
        {
           // Debug.Log("Scroll on mouseover filter");
            GUI.FocusControl(filterControlName);
            scrollPoppedOut = true;
            hasFocus = true;
        }

        position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        drawnHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        if (textChanged || newFilter != currentFilterText) //if filter text has been changed by user- recompute filter list
        {
            //Debug.Log("FilterString change detected.  current filtertext: '"+ currentFilterText+"'" + "new filtertext: '" + newFilter + "'");
            textChanged = false;
            string previousSelectedText = "";
            if (selectedElement != -1 && fiteredListConents != null && fiteredListConents.Count > selectedElement)
                previousSelectedText = fiteredListConents[selectedElement];

            currentFilterText = newFilter;
            //Debug.Log("current filtertext set to: '" + currentFilterText + "'");
            fiteredListConents.Clear();
            if (currentFilterText == "" || currentFilterText == "*" || currentFilterText == string.Empty)
                fiteredListConents = new List<string>(fullList);
            else
            {
                string filterAsLowercase = currentFilterText.ToLower();
                foreach (string s in fullList)
                {
                    if (s.ToLower().Contains(filterAsLowercase))
                        fiteredListConents.Add(s);
                }
            }
            int foundPreviousSelectionAt = 0;
            if (previousSelectedText != "")
                foundPreviousSelectionAt = fiteredListConents.IndexOf(previousSelectedText);
            selectedElement = foundPreviousSelectionAt;
        }// end filter text changed
        if (scrollPoppedOut)
        {
            popupPosition = position;
            popupPosition.xMin += 30;
            popupPosition.height = popupMaxHeight;

            float maxHeight = (fiteredListConents.Count + 2) * EditorGUIUtility.singleLineHeight;
            if (maxHeight < popupPosition.height)
                popupPosition.height = maxHeight;

            int hoverSelection = -1;
            if (!textHadFocusLastPass)
            {
                hoverSelection = HoverTextListWindow(popupPosition, -1);
            }
            else
            {
                hoverSelection = HoverTextListWindow(popupPosition);
            }
            if (filterControlHasFocus)
                textHadFocusLastPass = true;
            if (hoverSelection != -1)// a list element has been selected
            {
                //Debug.Log("Selected list element: " + hoverSelection);
                if (hoverSelection < fiteredListConents.Count)
                    currentFilterText = fiteredListConents[hoverSelection];
                else
                {
                    if (fiteredListConents.Count == 0)
                        currentFilterText = "";
                    else
                        currentFilterText = fiteredListConents[fiteredListConents.Count - 1];
                }
                fiteredListConents.Clear();
                string filterAsLowercase = currentFilterText.ToLower();
                foreach (string s in fullList)
                {
                    if (s.ToLower().Contains(filterAsLowercase))
                        fiteredListConents.Add(s);
                }
                selectedElement = fiteredListConents.IndexOf(currentFilterText);
            }
        }//filter text has focus
        else
        {
            if (!filterControlHasFocus)
                textHadFocusLastPass = false;
        }
        // Debug.Log(currentFilterText + $" - not focused event: {Event.current.type}");
        HandleEvents(popupPosition);
        // Debug.Log("current filtertext set to: '" + currentFilterText + "'");

        //selected element is element in index into "containing" not index into fullList, which is what we need to return
        if (selectedElement == -1 || currentFilterText.Length < 1) return -1;

        return fullList.IndexOf(currentFilterText);

    }



    /// <summary>
    /// Draws the popup list
    /// </summary>
    /// <param name="pos">where to draw it and how big</param>
    /// <param name="overrideSelection">if used, thus ekement will be drawn as selected/highlight</param>
    /// <returns>when enter is pressed, while an item on the list is highlighted, it returns the index of that item.  returns -1 otherwise</returns>
    int HoverTextListWindow(Rect pos, int overrideSelection = int.MinValue)
    {
      //  Debug.Log($"HoverTextListWindow function Detected event: {Event.current.type}");
      //  Debug.Log("drawing popup list");

        if (overrideSelection != int.MinValue)
            selectedElement = overrideSelection;

        Rect listSizeRect = new Rect(Vector2.zero, pos.size);
        drawnHeight += pos.size.y;
        listSizeRect.height = fiteredListConents.Count * EditorGUIUtility.singleLineHeight;

        Rect insideScrollRect = listSizeRect;
        insideScrollRect.height = EditorGUIUtility.singleLineHeight;

        //Debug.Log("drawing list rect height:" + pos.ToString());
        EditorGUI.DrawRect(pos, Color.white);
        GUI.SetNextControlName(nextControlName);
        scrollPos =GUI.BeginScrollView(pos, scrollPos, listSizeRect);
        int counter = 0;
        foreach (string s in fiteredListConents)
        {
            if (counter == selectedElement)
                EditorGUI.DrawRect(insideScrollRect, Color.grey);
            EditorGUI.LabelField(insideScrollRect, s);
            insideScrollRect.y += EditorGUIUtility.singleLineHeight;
            //Debug.Log("drawing list element");
            counter++;
        }

        GUI.EndScrollView();

        return HandleEvents(pos);
    }
    int HandleScrollEventOnly(Rect pos)
    {

        // Handle scroll wheel events
        if (Event.current.type == EventType.ScrollWheel)
        {
            //Debug.Log("Scroll wheel event detected");
            scrollPos.y += Event.current.delta.y * EditorGUIUtility.singleLineHeight;
            scrollPos.y = Mathf.Clamp(scrollPos.y, 0, fiteredListConents.Count * EditorGUIUtility.singleLineHeight - pos.height);

          //  Event.current.Use();
         //   GUI.FocusControl(filterControlName);
        }
        return -1;
    }
    int HandleEvents(Rect pos)
    {
        if (Event.current.type == EventType.MouseUp)//Event.current.isMouse)
        {
            if (pos.Contains(Event.current.mousePosition))
            {

                //convert mouse screen pos into scrollRectPos
                Vector2 mousePos = Event.current.mousePosition - pos.position; // position inside drawRect
                mousePos += scrollPos;// position inside scrollRect
                int clickedElement = (int)(mousePos.y / EditorGUIUtility.singleLineHeight);
                // Debug.Log("click element " + clickedElement);
                Event.current.Use();
                selectedElement = clickedElement;
                GUI.FocusControl(nextControlName);
                return selectedElement;
            }
            else
                hasFocus = false;
        }
        if (Event.current.isKey)
        {
            void PutInView()
            {
                float elementTop = selectedElement * EditorGUIUtility.singleLineHeight;
                if (scrollPos.y > elementTop)
                {
                    scrollPos.y = elementTop;
                }
                float elementBottom = (selectedElement + 2) * EditorGUIUtility.singleLineHeight;
                if (scrollPos.y + pos.height < elementBottom)
                {
                    scrollPos.y = elementBottom - pos.height;
                }
            }
            //  Debug.Log("Key event");
            if (Event.current.keyCode == KeyCode.UpArrow && selectedElement > 0)
            {

                selectedElement--;
                //ensure new selected element is displayed in scroll rect- adjust scroll pos if necessary. (user may have also scrolled away)
                PutInView();
                Event.current.Use();
                //   Debug.Log("Key event: uparrow selection: " + selectedElement);
            }
            if (Event.current.keyCode == KeyCode.DownArrow && selectedElement < fiteredListConents.Count - 1)
            {

                selectedElement++;
                //ensure new selected element is displayed in scroll rect- adjust scroll pos if necessary. (user may have also scrolled away)
                PutInView();

                Event.current.Use();
                //  Debug.Log("Key event: dwonarrow selection: " + selectedElement);
            }
            if (Event.current.keyCode == KeyCode.Return && selectedElement != -1)
            {
                Event.current.Use();
                GUI.FocusControl(nextControlName);
                return selectedElement;
            }
        }
        // Handle scroll wheel events
        return HandleScrollEventOnly(pos);
    }
}
