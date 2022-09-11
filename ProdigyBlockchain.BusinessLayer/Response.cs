using System;
using System.Collections.Generic;
using System.Text;

namespace Prodigy.BusinessLayer
{
    public class Response<T>
    {
        public Response()
        {

        }

        public Response(T obj)
        {
            this.Data = obj;
        }

        public Response(string exception)
        {
            this.SetException(exception);
        }

        public Response(List<string> exceptions)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var ex in exceptions)
                sb.AppendLine(ex);

            this.SetException(sb.ToString());
        }

        public bool Success { get; set; } = true;

        public string Exception { get; private set; }
        public T Data { get; set; }

        public void SetException(Exception e)
        {
            Exception = e.Message;
            Success = false;
        }

        public void SetException(string e)
        {
            Exception = e;
            Success = false;
        }
    }
}
