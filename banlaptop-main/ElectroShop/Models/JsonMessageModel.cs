using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ElectroShop.Models
{
    public class JsonMessageModel
    {
        private string Stt = "";
        private string Msg = "";
        private int Stt_code = 0;
        public int Return_ID { get; set; }
        public int Status_Code
        {
            get
            {
                return Stt_code;
            }

            set
            {
                if (value == 200)
                {
                    Msg = "Successful!";
                    Stt = "OK";
                }
                else
                {
                    Stt = "ERROR";

                    if (value == 404)
                    {
                        Msg = "Not found!";
                    }
                    else
                        if (value == 204)
                    {
                        Msg = "No returned content!";
                    }
                    else
                        if (value == 400)
                    {
                        Msg = "Bad request!";
                    }
                    else
                        if(value == 202)
                    {
                        Msg = "Request is accepted for processing but processing is not completed!";
                    }

                }
                Stt_code = value;
            }
        }
        public string Message
        {
            set
            {
                Msg = value;
            }

            get
            {
                return Msg;
            }
        }
        public string Status
        {
            get
            {
                return Stt;
            }

            set
            {
                Stt = value;
            }
        }

    }
}