using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegisterHolder : MonoBehaviour
{
    [SerializeField] private Transform employeeSpot;
    [SerializeField] private Transform customerSpot;
    [SerializeField] private Transform basketSpot;

    public bool employeeFilled = false;
    public bool customerFilled = false;

    public GameObject employee = null;
    public GameObject customer = null; 

    public Transform GetEmployeeSpot()
    {
        return employeeSpot;
    }

    public Transform GetCustomerSpot()
    {
        return customerSpot; 
    }

    public Transform GetBasketSpot()
    {
        return basketSpot; 
    }
}
