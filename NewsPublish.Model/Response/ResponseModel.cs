using System;
using System.Collections.Generic;
using System.Text;

namespace NewsPublish.Model.Response
{
    public class ResponseModel
    {
        public int Code { get; set; }
        public string Result { get; set; }
        public dynamic Data { get; set; }
    }
}
