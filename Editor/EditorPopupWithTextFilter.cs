using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EditorPopupWithTextFilter 
{

    //-1 indicates no element is selected, otherwise the value represents an index into the string list member "containing"
    int selectedElement = -1;
    // the text currently inside the userEditable TextField
    string currentFilterText="";
    public string currentTextOutput => currentFilterText;
    public void SetText(string newText)
    {
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
    public int Draw(Rect position, List<string> fullList, GUIContent label=null)
    {

        position.height = EditorGUIUtility.singleLineHeight;
        GUI.SetNextControlName("FilterText");
        if (label == null) label = new GUIContent("Filter");
        string newFilter = EditorGUI.TextField(position, label, currentFilterText);

        GUI.SetNextControlName("Next");
        position.y += EditorGUIUtility.singleLineHeight+ EditorGUIUtility.standardVerticalSpacing;
        drawnHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        if (newFilter != currentFilterText) //if filter text has been changed by user- recompute filter list
        {
            overrideHideList = false;
            string previousSelectedText = "";
            if(selectedElement!=-1 && containing!=null && containing.Count>0)
                previousSelectedText =containing[selectedElement];

            currentFilterText = newFilter;
            containing.Clear();
            if (currentFilterText == "" || currentFilterText == string.Empty)
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
            int foundPreviousSelectionAt = -1;
            if(previousSelectedText!="")
                foundPreviousSelectionAt = containing.IndexOf(previousSelectedText);
            selectedElement = foundPreviousSelectionAt;
        }// end filter text changed

        if (containing.Count > 0)
        {
            if (GUI.GetNameOfFocusedControl() == "FilterText")
            {
                //  Debug.Log("Filter text has focus");
                popupPosition = position;
                popupPosition.xMin += 30;
                popupPosition.height += 300;
                int hoverSelection=-1;
                if (!textHadFocusLastPass)
                {
                    overrideHideList = false;
                    hoverSelection = HoverTextListWindow(popupPosition,  -1);
                }
                else
                {
                    if(!overrideHideList)
                        hoverSelection = HoverTextListWindow(popupPosition);
                }
                textHadFocusLastPass = true;
                if (hoverSelection != -1)// a list element has been selected
                {
                    overrideHideList = true;
                    currentFilterText = containing[hoverSelection];
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
                
            }
        }
        //selected element is element in index into "containing" not index into fullList, which is what we need to return
        if (selectedElement == -1 || currentFilterText.Length<1) return -1;
      
        return fullList.IndexOf(currentFilterText);

    }

    Vector2 scrollPos = Vector2.zero;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="overrideSelection"></param>
    /// <returns>when enter is pressed, while an item on the list is highlighted, it returns the index of that item.  returns -1 otherwise</returns>
    int HoverTextListWindow(Rect pos, int overrideSelection = int.MinValue)
    {
        if (overrideSelection != int.MinValue)
            selectedElement = overrideSelection;

        Rect listSizeRect = new Rect(Vector2.zero, pos.size);
        drawnHeight += pos.size.y;
        listSizeRect.height = containing.Count * EditorGUIUtility.singleLineHeight;

        Rect insideScrollRect = listSizeRect;
        insideScrollRect.height = EditorGUIUtility.singleLineHeight;

        EditorGUI.DrawRect(pos, Color.white);
        scrollPos=GUI.BeginScrollView(pos, scrollPos, listSizeRect);
        int counter = 0;
        foreach (string s in containing)
        {
            if (counter == selectedElement)
                EditorGUI.DrawRect(insideScrollRect, Color.grey);
            EditorGUI.LabelField(insideScrollRect, s);
            insideScrollRect.y += EditorGUIUtility.singleLineHeight;
            
            counter++;
        }

        GUI.EndScrollView();

        return HandleEvents(pos);
        /*if (Event.current.isKey)
        {
          //  Debug.Log("Key event");
            if (Event.current.keyCode == KeyCode.UpArrow && selectedElement > 0)
            {

                selectedElement--;
                if (selectedElement < containing.Count - 1)
                    scrollPos.y -= EditorGUIUtility.singleLineHeight;
                Event.current.Use();
             //   Debug.Log("Key event: uparrow selection: " + selectedElement);
            }
            if (Event.current.keyCode == KeyCode.DownArrow && selectedElement < containing.Count - 1)
            {

                selectedElement++;
                if (selectedElement > 1)
                    scrollPos.y += EditorGUIUtility.singleLineHeight;
                Event.current.Use();
              //  Debug.Log("Key event: dwonarrow selection: " + selectedElement);
            }
            if (Event.current.keyCode == KeyCode.Return && selectedElement != -1)
            {
                Event.current.Use();
                return selectedElement;
            }
        }

        return -1;*/
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
            //  Debug.Log("Key event");
            if (Event.current.keyCode == KeyCode.UpArrow && selectedElement > 0)
            {

                selectedElement--;
                if (selectedElement < containing.Count - 1)
                    scrollPos.y -= EditorGUIUtility.singleLineHeight;
                Event.current.Use();
                //   Debug.Log("Key event: uparrow selection: " + selectedElement);
            }
            if (Event.current.keyCode == KeyCode.DownArrow && selectedElement < containing.Count - 1)
            {

                selectedElement++;
                if (selectedElement > 1)
                    scrollPos.y += EditorGUIUtility.singleLineHeight;
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

        return -1;
    }
}
