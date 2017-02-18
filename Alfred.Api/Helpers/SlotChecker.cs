using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alfred.Api.Helpers
{
    //Should use this class to double-check any custom slots 
    //to ensure the intent is interpreting the slot value correctly
    public static class SlotChecker
    {
        public static string SERVICE_ITEMS = "serviceitems.txt";
        public static string SERVICES = "services.txt";
        public static string SERVICE_ISSUES = "serviceissues.txt";
        public static string INTERNET_ISSUES = "internetItems.txt";
        public static string GIFT_SHOP_ITEMS = "giftshopitems.txt";
        public static string RESORT_LOCATIONS = "resortlocations.txt";
        public static string TRANSPORTATION = "transportation.txt";
        public static string MEAL_TYPES = "mealtypes.txt";
        public static string CRITTERS = "critters.txt";
        public static string FOOD_TYPES = "foodtypes.txt";
        public static string MED_PLACES = "medplace.txt";
        public static string BEVERAGES = "beverages.txt";
        public static string CLEANINGCREW = "cleaning.txt";
        public static string SCHEDULEDCLEANING = "scheduledclean.txt";


        public static List<string> GetSlotData(string filePath)
        {

            List<string> listData = new List<string>();
            string line = String.Empty;
            var textFilePath = System.Web.HttpContext.Current.Request.MapPath("~/App_Data/" + filePath);
            var filestream = new System.IO.FileStream(textFilePath,
                                              System.IO.FileMode.Open,
                                              System.IO.FileAccess.Read,
                                              System.IO.FileShare.ReadWrite);


            using (var file = new System.IO.StreamReader(filestream, System.Text.Encoding.UTF8))
            {
                while ((line = file.ReadLine()) != null)
                {
                    listData.Add(line);
                }

            }
            return listData;
        }
    }
}