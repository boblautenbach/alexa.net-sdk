using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Alfred.Api.Controllers
{
    public class testController : ApiController
    {
        public List<string> Get()
        {

            List<string> SlotC = new List<string>();
            string line = String.Empty;
            var textFilePath = System.Web.HttpContext.Current.Request.MapPath("~/App_Data/serviceitems.txt");
            var filestream = new System.IO.FileStream(textFilePath,
                                              System.IO.FileMode.Open,
                                              System.IO.FileAccess.Read,
                                              System.IO.FileShare.ReadWrite);
           

            using (var file = new System.IO.StreamReader(filestream, System.Text.Encoding.UTF8))
            {
                while ((line = file.ReadLine()) != null)
                {
                    SlotC.Add(line);
                }
                
            }
            return SlotC;
        }
    }
}
