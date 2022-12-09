
namespace excel2pb
{
    public class Column
    {
        Table m_Table;

        public int m_ColumnIndex;

        Column(Table table) { m_Table = table; }

        public static Column New(Table table)
        {
            Column col = new Column(table);

            return col;
        }

        public string ColumnName { get; set; }

        public string ValueType { get; set; }
        
        public string Comment { get; set; }

        public string Tag { get; set; }

        public int ColumnIndex => m_ColumnIndex;

        public Table Table => m_Table;
    }
}
