using System.Collections.Generic;

namespace excel2pb
{
    public class Row
    {
        Table m_Table = null;

        List<string> m_ItemArray = new List<string>();

        public string[] ItemArray
        {
            get
            {
                int offset = m_Table.Columns.Length - m_ItemArray.Count;
                if (offset > 0)
                    m_ItemArray.AddRange(new string[offset]);

                return m_ItemArray.ToArray();
            }
        }

        public Row(Table table)
        {
            m_Table = table;
        }

        public string this[string columnName]
        {

            get
            {
                var index = m_Table.GetColumnIndex(columnName);
                if(index < 0)
                    return null;
                return this[index];
            }
            set
            {
                var index = m_Table.GetColumnIndex(columnName);
                if (index < 0)
                    return;
                this[index] = value;
            }
        }

        public string this[int columnIndex]
        {
            get
            {
                if (columnIndex >= m_Table.Columns.Length)
                    return null;

                if (columnIndex >= m_ItemArray.Count)
                    return null;

                return m_ItemArray[columnIndex];
            }
            set
            {
                if (columnIndex >= m_Table.Columns.Length)
                    return;

                int offset = columnIndex - m_ItemArray.Count + 1;
                if (offset > 0)
                    m_ItemArray.AddRange(new string[offset]);

                m_ItemArray[columnIndex] = value;
            }
        }

        public Table Table => m_Table;
    }
}
