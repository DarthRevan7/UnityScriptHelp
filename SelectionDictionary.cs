using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SelectionSystem
{
    public class SelectionDictionary : MonoBehaviour
    {
        public Dictionary<int, GameObject> selectedTable = new Dictionary<int, GameObject>();

        public void addSelected(GameObject gameObj)
        {
            Collider c = gameObj.GetComponent<Collider>();
            if (c.CompareTag("Building") || c.CompareTag("Unit"))
            {
                AddSelected(gameObj);
            }
        }

        public void AddSelected(GameObject go)
        {
            int id = go.GetInstanceID();

            if (!(selectedTable.ContainsKey(id)))
            {
                selectedTable.Add(id, go);
                go.AddComponent<selection_component>();
                Debug.Log("Added " + id + " to selected dict");
            }
        }

        public void deselect(int id)
        {
            Destroy(selectedTable[id].GetComponent<selection_component>());
            selectedTable.Remove(id);
        }

        public void deselectAll()
        {
            foreach (KeyValuePair<int, GameObject> pair in selectedTable)
            {
                if (pair.Value != null)
                {
                    Destroy(selectedTable[pair.Key].GetComponent<selection_component>());
                }
            }
            selectedTable.Clear();
        }
    }
}