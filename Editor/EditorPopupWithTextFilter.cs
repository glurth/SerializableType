using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/*public class EditorPopupWithTextFilterNew
{
    #region stateInformation
    //-1 indicates no element is selected, otherwise the value represents an index into the string list member "containing"
    int selectedElement = -1;
    // the text currently inside the userEditable TextField
    string currentFilterText;
    bool filterTextChanged = false;
    Vector2 popupScrollPosition;
    bool hasFocus = false; //set to true when filter textbox gets focus, set to false when user selects a valid entry (via mouse or enterkey), or move to another control
    #endregion
    //draw info
   
    float drawnHeight = 0;

    public string currentTextOutput => currentFilterText;
    public void SetText(string newText)
    {
        Debug.Log("SetText called.  current filtertext: '" + currentFilterText + "'" + "    new filtertext: '" + newText + "'");
        if (currentFilterText != newText)
            filterTextChanged = true;
        currentFilterText = newText;
    }
    public float GetHeight()
    {
        return drawnHeight;
    }
    public int Draw(Rect position, List<string> fullList, GUIContent label = null)
    {
        // Debug.Log($"Draw function Detected event: {Event.current.type}");
        position.height = EditorGUIUtility.singleLineHeight;

        if (label == null) label = new GUIContent("Filter");
        GUI.SetNextControlName("FilterText");
        string newFilter = EditorGUI.TextField(position, label, currentFilterText);
        if (newFilter != currentFilterText)
        {
            currentFilterText = newFilter;
            filterTextChanged = true;
        }
        if (filterTextChanged) //if filter text has been changed by user or via code, SetText: then recompute filter list
        {
            //get contents of current selection, incase it's available after changing the filter
            string previousSelectedText = "";
            if (selectedElement != -1 && filteredContents != null && filteredContents.Count > 0)
                previousSelectedText = filteredContents[selectedElement];


            Filter(fullList);

            //check if contents of selection before filtering is available after changing the filter- and select it if so
            int foundPreviousSelectionAt = -1;
            if (previousSelectedText != "")
                foundPreviousSelectionAt = filteredContents.IndexOf(previousSelectedText);
            selectedElement = foundPreviousSelectionAt;
        }
        hasFocus |= (GUI.GetNameOfFocusedControl() == "FilterText");
        if (hasFocus)
        {
            
            Rect popupPosition = position;
            popupPosition.xMin += 30;
            popupPosition.height += 300;
            HandlePopupScrollEvents(popupPosition.height);
            DrawPopupTextListWindow(popupPosition);
            HandlePopupScrollEvents(popupPosition.height);
            //HandleSelection();
        }

        return selectedElement;
    }
    List<string> filteredContents = new List<string>();
    void Filter(List<string> fullList)
    {
        filteredContents.Clear();
        if (currentFilterText == "" || currentFilterText == "*" || currentFilterText == string.Empty)
            filteredContents = new List<string>(fullList);
        else
        {
            string filterAsLowercase = currentFilterText.ToLower();
            foreach (string s in fullList)
            {
                if (s.ToLower().Contains(filterAsLowercase))
                    filteredContents.Add(s);
            }
        }
    }

    /// <summary>
    /// Draws the popup list
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="overrideSelection"></param>
    /// <returns>when enter is pressed, while an item on the list is highlighted, it returns the index of that item.  returns -1 otherwise</returns>
    int DrawPopupTextListWindow(Rect pos)
    {
        Debug.Log($"HoverTextListWindow function Detected event: {Event.current.type}");
        Debug.Log("drawing popup list");

        Rect listSizeRect = new Rect(Vector2.zero, pos.size);
        drawnHeight += pos.size.y;
        listSizeRect.height = filteredContents.Count * EditorGUIUtility.singleLineHeight;

        Rect insideScrollRect = listSizeRect;
        insideScrollRect.height = EditorGUIUtility.singleLineHeight;

        //Debug.Log("drawing list rect height:" + pos.ToString());
        EditorGUI.DrawRect(pos, Color.white);
        popupScrollPosition = GUI.BeginScrollView(pos, popupScrollPosition, listSizeRect);
        int counter = 0;
        foreach (string s in filteredContents)
        {
            if (counter == selectedElement)
                EditorGUI.DrawRect(insideScrollRect, Color.grey);
            EditorGUI.LabelField(insideScrollRect, s);
            insideScrollRect.y += EditorGUIUtility.singleLineHeight;
            //Debug.Log("drawing list element");
            counter++;
        }

        GUI.EndScrollView();

        return 0;// HandleEvents(pos);
    }
    void HandlePopupScrollEvents(float viewHeight)
    {
        if (Event.current.type == EventType.ScrollWheel)
        {
            Debug.Log("Scroll wheel event detected");
            popupScrollPosition.y += Event.current.delta.y * EditorGUIUtility.singleLineHeight;
            popupScrollPosition.y = Mathf.Clamp(popupScrollPosition.y, 0, filteredContents.Count * EditorGUIUtility.singleLineHeight - viewHeight);

            Event.current.Use();
          //  GUI.FocusControl("FilterText");
        }
    }

}*/
public class EditorPopupWithTextFilter
{

    //-1 indicates no element is selected, otherwise the value represents an index into the string list member "containing"
    int selectedElement = -1;
    // the text currently inside the userEditable TextField
    string currentFilterText="g5948uiorthdfs03454ygfrf56ERREF";
    bool textChanged = false;
    public string currentTextOutput => currentFilterText;
    public void SetText(string newText)
    {
       // Debug.Log("SetText called.  current filtertext: '" + currentFilterText + "'" + "    new filtertext: '" + newText + "'");
        if (currentFilterText != newText)
            textChanged = true;
        currentFilterText = newText;
        
    }
    bool textHadFocusLastPass = false;
    List<string> containing = new List<string>();
    bool overrideHideList = false;
    Rect popupPosition;

    float drawnHeight = 0;
    public float GetHeight()
    {
        return drawnHeight;
    }
    bool hasFocus = false;
    public int Draw(Rect position, List<string> fullList, GUIContent label = null)
    {
        // Debug.Log($"Draw function Detected event: {Event.current.type}");
        position.height = EditorGUIUtility.singleLineHeight;
        //HandleScrollEventOnly(new Rect(0,0,100,300));
        if (label == null) label = new GUIContent("Filter");

        GUI.SetNextControlName("FilterText");
        string newFilter = EditorGUI.TextField(position, label, currentFilterText);

        GUI.SetNextControlName("Next");
        position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        drawnHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        if (textChanged || newFilter != currentFilterText) //if filter text has been changed by user- recompute filter list
        {
            //Debug.Log("FilterString change detected.  current filtertext: '"+ currentFilterText+"'" + "new filtertext: '" + newFilter + "'");
            textChanged = false;
            overrideHideList = false;
            string previousSelectedText = "";
            if (selectedElement != -1 && containing != null && containing.Count > selectedElement)
                previousSelectedText = containing[selectedElement];

            currentFilterText = newFilter;
            //Debug.Log("current filtertext set to: '" + currentFilterText + "'");
            containing.Clear();
            if (currentFilterText == "" || currentFilterText == "*" || currentFilterText == string.Empty)
                containing = new List<string>(fullList);
            else
            {
                string filterAsLowercase = currentFilterText.ToLower();
                foreach (string s in fullList)
                {
                    if (s.ToLower().Contains(filterAsLowercase))
                        containing.Add(s);
                }
            }
            int foundPreviousSelectionAt = 0;
            if (previousSelectedText != "")
                foundPreviousSelectionAt = containing.IndexOf(previousSelectedText);
            selectedElement = foundPreviousSelectionAt;
        }// end filter text changed


        //  Debug.Log("Checking Focus");

        if (!hasFocus && Event.current.type != EventType.ScrollWheel)
            hasFocus = GUI.GetNameOfFocusedControl() == "FilterText";
        if (hasFocus)//GUI.GetNameOfFocusedControl() == "FilterText" )//|| (Event.current.type == EventType.Repaint))
        {
            //Debug.Log("Filter text has focus.  had it lastpass:"+ textHadFocusLastPass);
            popupPosition = position;
            popupPosition.xMin += 30;
            popupPosition.height += 300;
            int hoverSelection = -1;
            if (!textHadFocusLastPass)
            {
                overrideHideList = false;
                hoverSelection = HoverTextListWindow(popupPosition, -1);
            }
            else
            {
                if (!overrideHideList)
                    hoverSelection = HoverTextListWindow(popupPosition);
            }
            textHadFocusLastPass = true;
            if (hoverSelection != -1)// a list element has been selected
            {
                //Debug.Log("Selected list element: " + hoverSelection);
                overrideHideList = true;
                if (hoverSelection < containing.Count)
                    currentFilterText = containing[hoverSelection];
                else
                {
                    if (containing.Count == 0)
                        currentFilterText = "";
                    else
                        currentFilterText = containing[containing.Count - 1];
                }
                containing.Clear();
                string filterAsLowercase = currentFilterText.ToLower();
                foreach (string s in fullList)
                {
                    if (s.ToLower().Contains(filterAsLowercase))
                        containing.Add(s);
                }
            }
        }//filter text has focus
        else
        {
            textHadFocusLastPass = false;
            //Debug.Log(currentFilterText + $" - not focused event: {Event.current.type}");
            HandleEvents(popupPosition);
        }

        // Debug.Log("current filtertext set to: '" + currentFilterText + "'");
        //selected element is element in index into "containing" not index into fullList, which is what we need to return
        if (selectedElement == -1 || currentFilterText.Length < 1) return -1;

        return fullList.IndexOf(currentFilterText);

    }

    Vector2 scrollPos = Vector2.zero;
    /// <summary>
    /// Draws the popup list
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="overrideSelection"></param>
    /// <returns>when enter is pressed, while an item on the list is highlighted, it returns the index of that item.  returns -1 otherwise</returns>
    int HoverTextListWindow(Rect pos, int overrideSelection = int.MinValue)
    {
      //  Debug.Log($"HoverTextListWindow function Detected event: {Event.current.type}");
      //  Debug.Log("drawing popup list");

        if (overrideSelection != int.MinValue)
            selectedElement = overrideSelection;

        Rect listSizeRect = new Rect(Vector2.zero, pos.size);
        drawnHeight += pos.size.y;
        listSizeRect.height = containing.Count * EditorGUIUtility.singleLineHeight;

        Rect insideScrollRect = listSizeRect;
        insideScrollRect.height = EditorGUIUtility.singleLineHeight;

        //Debug.Log("drawing list rect height:" + pos.ToString());
        EditorGUI.DrawRect(pos, Color.white);
        scrollPos=GUI.BeginScrollView(pos, scrollPos, listSizeRect);
        int counter = 0;
        foreach (string s in containing)
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
            scrollPos.y = Mathf.Clamp(scrollPos.y, 0, containing.Count * EditorGUIUtility.singleLineHeight - pos.height);

            Event.current.Use();
            GUI.FocusControl("FilterText");
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
                int clickedElement =(int)( mousePos.y / EditorGUIUtility.singleLineHeight);
               // Debug.Log("click element " + clickedElement);
                Event.current.Use();
                selectedElement = clickedElement;
                GUI.FocusControl("Next");
                return selectedElement;
            }
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
            if (Event.current.keyCode == KeyCode.DownArrow && selectedElement < containing.Count - 1)
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
                GUI.FocusControl("Next");
                return selectedElement;
            }
        }
        // Handle scroll wheel events
        return HandleScrollEventOnly(pos);
    }
}
