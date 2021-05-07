using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SelectionSystem
{
    public class SelectionSystem : MonoBehaviour
    {
        [SerializeField] Box box;                   //Instance of the class below
        [SerializeField] Collider[] selections;     //Container for selected objects 
        [SerializeField] int numberOfSelUnits = 0;  //Counter for selected units
        SelectionDictionary selectedUnits;          //Dictionary with selected units

        private Vector3 startPos, dragPos;          //Design the positions of the wired cube (you do not see it)
        private Camera mainCamera;                  //Main camera
        private Ray ray;                            //Ray casted from GUI to the 3D world

        private bool dragSelect = false;            //Tells the script if you are dragging the mouse or not

        private Vector3 p1;                         //Initial position of the screen selector


        // Start is called before the first frame update
        void Start()
        {
            mainCamera = Camera.main;                               //Obtain the reference to the Main Camera Object
            selectedUnits = GetComponent<SelectionDictionary>();    //Here is the SelectionDictionary 
        }

        // Update is called once per frame
        void Update()
        {
            HandleUnitMovement();
            HandleSelection();
        }

        public void HandleUnitMovement()
        {
            RaycastHit hit;                                 //Variable that will contain reference of where the ray hit something

            //Follows the code for moving a unit
            if (Input.GetMouseButtonDown(1))
            {
                //RaycastHit hit;
                ray = mainCamera.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit, 1000.0f) && numberOfSelUnits > 0)
                {
                    foreach (var obj in selectedUnits.AsList())
                    {
                        Unit unit = obj.GetComponent<Unit>();
                        if (unit != null)
                        {
                            unit.MoveUnit(hit.point);
                        }
                    }
                }
            }
        }


        private void HandleSelection()
        {
            Raycasthit hit;


            if (Input.GetMouseButtonDown(0))                //If left mouse button is pressed
            {
                p1 = Input.mousePosition;                   //p1 is set to actual position of mouse
                if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))  //If the player is NOT holding SHIFT
                {
                    selectedUnits.DeselectAll();                //It means that he does not want to make a multiple selection.
                    numberOfSelUnits = 0;                       //Clear the dictionary of selected units.
                }
                ray = mainCamera.ScreenPointToRay(Input.mousePosition);     //Prepare the ray to be casted
                Physics.Raycast(ray, out hit, 1000.0f);                     //Cast the ray from mouse position to the 3D world and returns the hit point of contact inside hit variable

                startPos = hit.point;                       //startPos of Box is set to this hit point
                box.baseMin = startPos;                     //As the box.baseMin
            }

            if (Input.GetMouseButton(0) && (p1 - Input.mousePosition).magnitude > 40.0f)        //If the left mouse button is being held down and you are actually dragging the mouse
            {
                dragSelect = true;      //Set this boolean to true

                ray = mainCamera.ScreenPointToRay(Input.mousePosition);     //Prepare the ray to be casted from screen's mouse position
                Physics.Raycast(ray, out hit, 1000.0f);                     //Cast the ray from that position

                dragPos = hit.point;        //Set dragPos and box.baseMax
                box.baseMax = dragPos;

            }

            if (Input.GetMouseButtonUp(0))          //If the left mouse button is released
            {
                if (!dragSelect)         //If there is no box selection
                {
                    ray = mainCamera.ScreenPointToRay(Input.mousePosition);         //Create the ray
                    if (Physics.Raycast(ray, out hit, 1000.0f))                     //Cast the ray. Raycast returns true if hit something
                    {
                        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))     //If the player is holding shift, he wants to make multiple selection
                        {
                            if (hit.collider.CompareTag("Unit"))        //If a unit is hit
                            {
                                if (selectedUnits.ContainsObjects(hit.transform.gameObject))        //and it is already selected
                                {
                                    selectedUnits.Deselect(hit.transform.gameObject.GetInstanceID());       //the player wants to deselect it
                                }
                                else                               //If not already selected
                                {
                                    selectedUnits.AddSelected(hit.transform.gameObject);        //the unit is added to the selection
                                    numberOfSelUnits++;
                                }
                            }
                            else
                            {
                                //If the player is holding shift and hits nothing, then it could be a mistake, so I chose to do nothing
                            }
                        }
                        else    //If the player is not holding shift, then he wants to select only one unit!
                        {
                            if (hit.collider.CompareTag("Unit") || hit.collider.CompareTag("Building"))        //If the player hits a Building (Edificio) or a Unit (Unità)
                            {
                                selectedUnits.DeselectAll();                            //Clear the selection
                                selectedUnits.AddSelected(hit.transform.gameObject);    //Add that unit/building
                                numberOfSelUnits = 1;
                            }
                            else  //If the player hits nothing
                            {
                                selectedUnits.DeselectAll();        //Clear the selection
                                numberOfSelUnits = 0;
                            }
                        }
                    }
                }


                else    //If the player wants to do a box selection
                {
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))        //If the player wants to do a multiple selection
                    {
                        selections = Physics.OverlapBox(box.Center, box.Extents, Quaternion.identity);      //Create an array with Colliders of units.
                        foreach (Collider selected in selections)           //For every collider hit by the box
                        {
                            if (selected.CompareTag("Unit"))               //If it is tagged as Unit (Unità)
                            {
                                selectedUnits.AddSelected(selected.gameObject);     //Add to selection
                            }
                        }
                        numberOfSelUnits = selectedUnits.GetLength();
                    }
                    else            //If the player does not want to do a multiple selection
                    {
                        selections = Physics.OverlapBox(box.Center, box.Extents, Quaternion.identity);      //Same as before
                        foreach (Collider selected in selections)                                           //For every collider in the array created
                        {
                            if (selected.CompareTag("Unit"))                                               //If the collider is tagged as Unit
                            {
                                selectedUnits.AddSelected(selected.gameObject);                             //The unit is selected
                            }
                        }
                        numberOfSelUnits = selectedUnits.GetLength();
                    }


                }

                dragSelect = false;         //Important to do this here! At the end of the chain of if statements, we must set dragSelect to false, because the player may have changed 
                                            //idea very quickly or may have made a mistake.


            }//End of GetMouseButtonUp if statement
        }

        private void OnGUI()
        {
            if (dragSelect == true)
            {
                var rect = Utils.GetScreenRect(p1, Input.mousePosition);
                Utils.DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.25f));
                Utils.DrawScreenRectBorder(rect, 2, new Color(0.8f, 0.8f, 0.95f));
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(box.Center, box.Size);
        }
    }

    [System.Serializable]
    public class Box
    {
        public Vector3 baseMin, baseMax;

        public Vector3 Center
        {
            get
            {
                Vector3 center = baseMin + (baseMax - baseMin) / 2.0f;
                center.y = (baseMax - baseMin).magnitude / 2.0f;
                return center;
            }
        }

        public Vector3 Size
        {
            get
            {
                return new Vector3(Mathf.Abs(baseMax.x - baseMin.x), (baseMax - baseMin).magnitude, Mathf.Abs(baseMax.z - baseMin.z));
            }
        }

        public Vector3 Extents
        {
            get
            {
                return Size / 2.0f;
            }
        }
    }
}
