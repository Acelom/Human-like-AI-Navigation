using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

namespace MoneyMaster
{
    public class MoneyManager : MonoBehaviour
    {
        public float currMoney;
        public float startingMoney;
        private Text moneyLabel; 

        public enum STOCKQUALITY
        {
            low = 5,
            average = 10,
            high = 15,
        };

        public STOCKQUALITY vegQuality = STOCKQUALITY.average;
        public STOCKQUALITY cansQuality = STOCKQUALITY.average;
        public STOCKQUALITY breadQuality = STOCKQUALITY.average;
        public float vegProfitPercentage = 0;
        public float canProfitPercentage = 0;
        public float breadProfitPercentage = 0;


        private void Awake()
        {
            currMoney = startingMoney;
            moneyLabel = GameObject.FindGameObjectWithTag("Money").GetComponent<Text>() ; 
        }

        private void Update()
        {
            currMoney = Mathf.RoundToInt(currMoney); 
            moneyLabel.text = currMoney.ToString();

            Mathf.Clamp(vegProfitPercentage, -100, 100);
            Mathf.Clamp(canProfitPercentage, -100, 100);
            Mathf.Clamp(breadProfitPercentage, -100, 100);
        }
    }
}
