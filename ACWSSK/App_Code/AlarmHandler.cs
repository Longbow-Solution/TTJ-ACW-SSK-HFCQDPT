using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACWSSK.App_Code
{
    public class AlarmHandler
    {
        const string TraceCategory = "ACWSSK.App_Code.AlarmHandler";

        private CRUD query;
        private Object alarmLock = new Object();

        public AlarmHandler()
        {
            query = new CRUD(GeneralVar.DBConnString);
        }

        public int GetAlarmCategoryId(string alarmCategoryCode)
        {
            int id = 0;
            try
            {
                if (GeneralVar.DB_AlarmCategories != null)
                {
                    var alarmCategory = GeneralVar.DB_AlarmCategories.FirstOrDefault(ac => ac.AlarmCategoryCode == alarmCategoryCode);
                    if (alarmCategory != null)
                        id = alarmCategory.AlarmCategoryId;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[ERROR] GetAlarmCategoryId: {0}", ex.ToString()), TraceCategory);
            }
            return id;
        }

        public void InsertAlarm(int categoryId, string txType, string description, CRUD crud = null)
        {
            if (categoryId == 0)
                return;

            lock (alarmLock)
            {
                try
                {
                    int componentId = GeneralVar.CurrentComponent.Id;
                    int txId = GeneralVar.LastTx["acw_TxAlarm"].NextId();
                    DateTime txDate = DateTime.Now;
                    string referenceNo = GeneralVar.LastTx["acw_TxAlarm"].NextReferenceNo();

                    if (crud != null)
                    {
                        crud.BeginTransaction();
                        int rowAffecetd = crud.acw_TxAlarm_Insert(componentId, txId, txDate, txType, referenceNo, categoryId, description, "N");
                        if (rowAffecetd > 0)
                            crud.CommitTransaction();
                        else
                            crud.RollBackTransaction();
                    }
                    else
                    {
                        query.BeginTransaction();
                        int rowAffecetd = query.acw_TxAlarm_Insert(componentId, txId, txDate, txType, referenceNo, categoryId, description, "N");
                        if (rowAffecetd > 0)
                            query.CommitTransaction();
                        else
                            query.RollBackTransaction();
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[ERROR] InsertAlarm: {0}", ex.ToString()), TraceCategory);
                }
            }
        }
    }
}
