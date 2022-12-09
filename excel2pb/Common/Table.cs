using System.Collections.Generic;

namespace excel2pb
{
    public class Table
    {
        List<Row> m_Rows = new List<Row>();

        List<Column> m_Columns = new List<Column>();

        public string Name { get; set; }

        public string Namespace { get; set; }

        public Row[] Rows => m_Rows.ToArray();

        public Column[] Columns => m_Columns.ToArray();

        public Column NewColumn()
        {
            var column = Column.New(this);
            m_Columns.Add(column);
            return column;
        }

        public Column[] NewColumnArray(int size)
        {
            Column[] columns = new Column[size];
            for (int i = 0; i < size; i++)
                columns[i] = Column.New(this);
            return columns;
        }

        public int GetColumnIndex(string columnName)
        {
            int index = -1;
            for (int i = 0; i < m_Columns.Count; i++)
            {
                if(m_Columns[i].ColumnName == columnName)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }

        public Row NewRow()
        {
            var row =  new Row(this);
            m_Rows.Add(row);
            return row;
        }

        public Row[] NewRowArray(int size)
        {
            Row[] rows = new Row[size];
            for (int i = 0; i < size; i++)
                rows[i] = new Row(this);
            return rows;
        }
    }
}
