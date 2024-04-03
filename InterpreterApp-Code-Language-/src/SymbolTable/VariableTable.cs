using InterpreterApp.Analysis.Tree.Expression;
using InterpreterApp.Analysis.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterApp.Analysis.Table
{
    public class VariableTable
    {
        private Dictionary<string, KeyValuePair<DataType, object>> variable_table;
        public VariableTable()
        {
            variable_table = new Dictionary<string, KeyValuePair<DataType, object>>();
        }

        public void AddVariable(string identifier, DataType data_type, object val)
        {
            variable_table.Add(identifier, new KeyValuePair<DataType, object>(data_type, val));
        }

        public void AddIdentifier(string identifier, DataType data_type)
        {
            variable_table.Add(identifier, new KeyValuePair<DataType, object>(data_type, null));
        }

        public void AddValue(string identifier, object val)
        {
            variable_table[identifier] = new KeyValuePair<DataType, object>(variable_table[identifier].Key, val);
        }

        public DataType GetType(string identifier)
        {
            return variable_table[identifier].Key;
        }

        public object GetValue(string identifier)
        {
            return variable_table[identifier].Value;
        }

        public bool Exist(string identifier)
        {
            return variable_table.ContainsKey(identifier);
        }

    }
}
