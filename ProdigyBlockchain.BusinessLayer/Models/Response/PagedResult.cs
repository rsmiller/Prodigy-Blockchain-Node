using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prodigy.BusinessLayer.Models.Response
{
    public class PagedResult<T>
    {
        public T Data { get; set; }
        public int TotalResults { get; set; } = 0;
        public bool Success { get; set; } = true;
        public string Exception { get; private set; }

        public PagedResult()
        {

        }

        public PagedResult(T obj)
        {
            this.Data = obj;
        }

        public PagedResult(string exception)
        {
            this.SetException(exception);
        }

        public PagedResult(List<string> exceptions)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var ex in exceptions)
                sb.AppendLine(ex);

            this.SetException(sb.ToString());
        }

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
