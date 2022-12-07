using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace excel2pb
{
    public class DataSourceHeader
    {
        public string Area;

        public string Rule;

        public string Type;

        public string Name;

    }

    public class DataSourceBody
    {

    }


    public interface IDataSource
    {
        DataTable GetDataTable();

        DataSourceHeader[] GetHeaders();
    }
}
