using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; 

namespace EmployeeItems
{
    public class EmployeeMaster : MonoBehaviour
    {
        public List<GameObject> chairList = new List<GameObject>();

        private void Update()
        {
            chairList = FindObjectsOfType<ChairHolder>().Select(item => item.gameObject).ToList();
        }


        public bool GetAnyChairFree()
        {
            List<bool> bools = new List<bool>(chairList.Select(item => !item.GetComponent<ChairHolder>().spotFilled && item.GetComponentInChildren<Collider>().enabled).ToList());
            return bools.Any(item => item == true);
        }

        public List<GameObject> GetFreeChairs()
        {
            return chairList.Where(item => (!item.GetComponent<ChairHolder>().spotFilled && item.GetComponentInChildren<Collider>().enabled == true)).ToList();
        }



    }




}
