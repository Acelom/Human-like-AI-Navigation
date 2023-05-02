using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using MoneyMaster;

namespace CommonItems
{
    public class CommonMaster : MonoBehaviour
    {

        public List<GameObject> registerList = new List<GameObject>();
        public List<GameObject> shelfList = new List<GameObject>();
        public List<GameObject> floorList = new List<GameObject>();

        public bool empRegisterFree = false;
        public bool cusRegisterFree = false;

        public int totalCustomers;
        public int totalEmployees;

        private MoneyManager money; 
        [SerializeField] private int wage;
        [SerializeField] private float wageTimerLimit;
        private float wageTimer;

        private void Awake()
        {
            money = GameObject.FindObjectOfType<MoneyManager>(); 
        }


        private void Update()
        {
            empRegisterFree = false;
            cusRegisterFree = false;

            registerList = FindObjectsOfType<RegisterHolder>().Where(item => item.GetComponentInChildren<Collider>().enabled).Select(item => item.gameObject).ToList();
            shelfList = FindObjectsOfType<ShelfHandler>().Where(item => item.GetComponentInChildren<Collider>().enabled).Select(item => item.gameObject).ToList();
            floorList = FindObjectsOfType<FloorHandler>().Select(item => item.gameObject).ToList();

            totalEmployees = GameObject.FindObjectsOfType<EmployeeAI>().Length;
            totalCustomers = GameObject.FindObjectsOfType<CustomerAI>().Length;

            if (wageTimer > wageTimerLimit)
            {
                money.currMoney -= wage * totalEmployees;
                wageTimer = 0; 
            }
            else
            {
                wageTimer += Time.deltaTime; 
            }

        }

        public bool GetAnyShelfFree()
        {
            List<bool> bools = new List<bool>(shelfList.Select(item => !item.GetComponent<ShelfHandler>().spotFilled && item.GetComponentInChildren<Collider>().enabled).ToList());
            return bools.Any(item => item == true); 
        }

        public List<GameObject> GetFreeShelfs()
        {
            return shelfList.Where(item => (!item.GetComponent<ShelfHandler>().spotFilled && item.GetComponentInChildren<Collider>().enabled == true)).ToList();
        }

        public bool GetEmpRegisterAnyFree()
        {
            List<bool> bools = new List<bool>(registerList.Select(item => !item.GetComponent<RegisterHolder>().employeeFilled && item.GetComponentInChildren<Collider>().enabled).ToList()); 

            return bools.Any(item => item == true);
        }

        public bool GetCusRegisterAnyFree()
        {
            List<bool> bools =  new List<bool>(registerList.Select(item => !item.GetComponent<RegisterHolder>().customerFilled && item.GetComponentInChildren<Collider>().enabled).ToList());
            return bools.Any(item => item == true); 
        }

        public bool GetAnyRegisterWorked()
        {
            List<bool> bools = new List<bool>(registerList.Select(item => item.GetComponent<RegisterHolder>().employeeFilled &&
            !item.GetComponent<RegisterHolder>().customerFilled && item.GetComponentInChildren<Collider>().enabled).ToList());
            return bools.Any(item => item == true);
        }

        public List<GameObject> GetFreeEmpRegisters()
        {
            return registerList.Where(item => !item.GetComponent<RegisterHolder>().employeeFilled && 
            item.transform.GetChild(0).GetComponent<Collider>().enabled).ToList();
        }

        public List<GameObject> GetFreeCusRegisters()
        {
            return registerList.Where(item => !item.GetComponent<RegisterHolder>().customerFilled &&
            item.GetComponent<RegisterHolder>().employeeFilled &&
            item.transform.GetChild(0).GetComponent<Collider>().enabled).ToList();     
        }


    }

}
