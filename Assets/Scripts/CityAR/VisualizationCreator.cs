using DefaultNamespace;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;

namespace CityAR
{
    public class VisualizationCreator : MonoBehaviour
    {

        public GameObject districtPrefab;
        public GameObject buildingPrefab;
        private DataObject _dataObject;
        private GameObject _platform;
        private Data _data;

        private void Start()
        {
            _platform = GameObject.Find("Platform");
            _data = _platform.GetComponent<Data>();
            _dataObject = _data.ParseData();
            BuildNumberOfLinesCity();
        }


        private IEnumerator clearPlatform(int metric){
            List<GameObject> children = new List<GameObject>();
            foreach(Transform child in _platform.transform) children.Add(child.gameObject);
            foreach(GameObject child in children){
                Destroy(child);
            }
            yield return new WaitForSeconds(0.5f);
            _dataObject.project.x = 0;
            _dataObject.project.z = 0;
            _dataObject.project.w = 1;
            _dataObject.project.h = 1;
            _dataObject.project.deepth = 1;
            BuildDistrict(_dataObject.project, false, metric);
        }


        public async void BuildNumberOfLinesCity()
        {
            if (_dataObject.project.files.Count > 0)
            {
                StartCoroutine(clearPlatform(1));
            }
        }
        

        public async void BuildNumberOfMethodsCity()
        {
            if (_dataObject.project.files.Count > 0)
            {
                StartCoroutine(clearPlatform(2));
            }
        }
        

        public async void BuildNumberOfAbstractClassesCity()
        {
            if (_dataObject.project.files.Count > 0)
            {
                  StartCoroutine(clearPlatform(3));
            }
        }
        

        public async void BuildNumberOfInterfacesCity()
        {
            if (_dataObject.project.files.Count > 0)
            {
                StartCoroutine(clearPlatform(4));
            }
        }



        /*
         * entry: Single entry from the data set. This can be either a folder or a single file.
         * splitHorizontal: Specifies whether the subsequent children should be split horizontally or vertically along the parent
         * metric: 1 - numberOfLines; 2 - numberOfMethods; 3 - numberOfAbstractClasses; 4 - numberOfInterfaces
         */
        private void BuildDistrict(Entry entry, bool splitHorizontal, int metric)
        {
            if (entry.type.Equals("File"))
            {
                //TODO if entry is from type File, create building

                entry.deepth = entry.parentEntry.deepth + 1;
                entry.x = entry.parentEntry.x;
                entry.z = entry.parentEntry.z;
                entry.w = 0.04f;
                entry.h = 0.04f;
                entry.color = GetColorForDepth(entry.deepth);

                BuildBuilding(entry, metric);

          
            }
            else
            {
                float x = entry.x;
                float z = entry.z;


                entry.color = GetColorForDepth(entry.deepth);

                BuildDistrictBlock(entry, false);

                foreach (Entry subEntry in entry.files) {
                    subEntry.x = x;
                    subEntry.z = z;
                    
                    if (subEntry.type.Equals("Dir"))
                    {
                        float ratio;
                        switch(metric){
                            case 1:
                                ratio = (float)subEntry.numberOfLines / entry.numberOfLines;
                                break;
                            case 2:
                                ratio = (float)subEntry.numberOfMethods / entry.numberOfMethods;
                                break;
                            case 3:
                                ratio = (float)subEntry.numberOfAbstractClasses / entry.numberOfAbstractClasses;
                                break;
                            case 4:
                                ratio = (float)subEntry.numberOfInterfaces / entry.numberOfInterfaces;
                                break;
                            default:
                                ratio = (float)subEntry.numberOfLines / entry.numberOfLines;
                                break;
                        }
                        subEntry.deepth = entry.deepth + 1;

                        if (splitHorizontal) {
                            subEntry.w = ratio * entry.w; // split along horizontal axis
                            subEntry.h = entry.h;
                            x += subEntry.w;
                        } else {
                            subEntry.w = entry.w;
                            subEntry.h = ratio * entry.h; // split along vertical axis
                            z += subEntry.h;
                        }
                    }
                    else
                    {
                        subEntry.parentEntry = entry;
                    }
                    BuildDistrict(subEntry, !splitHorizontal, metric);
                }

                if (!splitHorizontal)
                {
                    entry.x = x;
                    entry.z = z;
                    if (ContainsDirs(entry))
                    {
                        entry.h = 1f - z;
                    }
                    entry.deepth += 1;
                    BuildDistrictBlock(entry, true);
                }
                else
                {
                    entry.x = -x;
                    entry.z = z;
                    if (ContainsDirs(entry))
                    {
                        entry.w = 1f - x;
                    }
                    entry.deepth += 1;
                    BuildDistrictBlock(entry, true);
                }
            }
        }


        private void BuildBuilding(Entry entry, int metric)
        {
            if(entry == null)
            {
                return;
            }

            
            if(entry.numberOfLines > 0){
                GameObject buildingInstance = Instantiate(buildingPrefab, _platform.transform, true);
            
                buildingInstance.name = entry.name;
                buildingInstance.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = entry.color;
                switch(metric){
                    case 1:
                        if(entry.numberOfLines == 0){
                            buildingInstance.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
                        }
                        buildingInstance.transform.localScale = new Vector3(entry.w, entry.numberOfLines * 0.1f, entry.h);
                        break;
                    case 2:
                        if(entry.numberOfMethods == 0){
                            buildingInstance.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
                        }
                        buildingInstance.transform.localScale = new Vector3(entry.w, entry.numberOfMethods * 0.5f, entry.h);
                        break;
                    case 3:
                        if(entry.numberOfAbstractClasses == 0){
                            buildingInstance.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
                        }
                        buildingInstance.transform.localScale = new Vector3(entry.w, entry.numberOfAbstractClasses * 20, entry.h);
                        break;
                    case 4:
                        if(entry.numberOfInterfaces == 0){
                            buildingInstance.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
                        }                    
                        buildingInstance.transform.localScale = new Vector3(entry.w, entry.numberOfInterfaces * 20, entry.h);
                        break;
                }

                Vector3 scale = buildingInstance.transform.localScale;
                float scaleX = scale.x - (entry.deepth * 0.005f);
                float scaleZ = scale.z - (entry.deepth * 0.005f);
                float shiftX = (scale.x - scaleX) / 2f;
                float shiftZ = (scale.z - scaleZ) / 2f;
                buildingInstance.transform.localScale = new Vector3(scaleX, scale.y, scaleZ);
                buildingInstance.transform.GetChild(0).GetComponent<ShowToolTip>().entry = entry;
                buildingInstance.transform.GetChild(0).GetComponent<ShowToolTip>().metric = metric;
            }

        }

        /*
         * entry: Single entry from the data set. This can be either a folder or a single file.
         * isBase: If true, the entry has no further subfolders. Buildings must be placed on top of the entry
         */
        private void BuildDistrictBlock(Entry entry, bool isBase)
        {
            if (entry == null)
            {
                return;
            }
            
            float w = entry.w; // w -> x coordinate
            float h = entry.h; // h -> z coordinate
            
            if (w * h > 0)
            {
                GameObject prefabInstance = Instantiate(districtPrefab, _platform.transform, true);


                if (!isBase)
                {
                    prefabInstance.name = entry.name;
                    prefabInstance.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = entry.color;
                    prefabInstance.transform.localScale = new Vector3(entry.w, 1f,entry.h);
                    prefabInstance.transform.localPosition = new Vector3(entry.x, entry.deepth, entry.z);
                }
                else
                {
                    prefabInstance.name = entry.name+"Base";
                    prefabInstance.transform.GetChild(0).rotation = Quaternion.Euler(90, 0, 0);
                    prefabInstance.transform.localScale = new Vector3(entry.w, 1,entry.h);
                    prefabInstance.transform.localPosition = new Vector3(entry.x, entry.deepth+0.001f, entry.z);
                }
                
                Vector3 scale = prefabInstance.transform.localScale;
                float scaleX = scale.x - (entry.deepth * 0.005f);
                float scaleZ = scale.z - (entry.deepth * 0.005f);
                float shiftX = (scale.x - scaleX) / 2f;
                float shiftZ = (scale.z - scaleZ) / 2f;
                prefabInstance.transform.localScale = new Vector3(scaleX, scale.y, scaleZ);
                Vector3 position = prefabInstance.transform.localPosition;
                prefabInstance.transform.localPosition = new Vector3(position.x - shiftX, position.y, position.z + shiftZ);

                if(isBase){
                    // num rows based on base size
                    // Cell width and height

                    prefabInstance.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
                    GridObjectCollection goc = prefabInstance.transform.GetChild(0).GetComponent<GridObjectCollection>();
                    if(scaleX < scaleZ){
                        goc.Layout = 0;
                        goc.CellWidth = 0.3f;
                        goc.CellHeight = 0.1f;
                        goc.Columns = Math.Min((int)Math.Round(scaleX / 0.04f), 4);
                    }
                    if(scaleX >= scaleZ){
                        goc.Rows = Math.Min((int)Math.Round(scaleZ / 0.04f), 4);
                    }

                    foreach (Entry subEntry in entry.files) {
                        if(subEntry.type.Equals("File")){
                            GameObject.Find(subEntry.name).transform.parent = prefabInstance.transform.GetChild(0).transform;
                        }
                    }

                    goc.UpdateCollection();
                }

            }
        }

        private bool ContainsDirs(Entry entry)
        {
            foreach (Entry e in entry.files)
            {
                if (e.type.Equals("Dir"))
                {
                    return true;
                }
            }

            return false;
        }
        
        private Color GetColorForDepth(int depth)
        {
            Color color;
            switch (depth)
            {
                case 1:
                    color = new Color(179f / 255f, 209f / 255f, 255f / 255f);
                    break;
                case 2:
                    color = new Color(128f / 255f, 179f / 255f, 255f / 255f);
                    break;
                case 3:
                    color = new Color(77f / 255f, 148f / 255f, 255f / 255f);
                    break;
                case 4:
                    color = new Color(26f / 255f, 117f / 255f, 255f / 255f);
                    break;
                case 5:
                    color = new Color(0f / 255f, 92f / 255f, 230f / 255f);
                    break;
                default:
                    color = new Color(0f / 255f, 71f / 255f, 179f / 255f);
                    break;
            }

            return color;
        }
    }
}