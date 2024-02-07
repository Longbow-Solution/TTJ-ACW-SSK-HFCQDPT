using ACWSSK.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ACWSSK.App_Code
{
    public class CRUD
    {
        const string TraceCategory = "ACWSSK.App_Code.CRUD";

        #region Variables

        private SqlCeConnection dbconn = null;
        private SqlCeCommand dbcmd = null;
        private SqlCeTransaction dbtran = null;
        private SqlCeDataAdapter dbadapt = null;

        #endregion

        #region Constructor

        public CRUD(string connection)
        {
            OpenConnection(connection);
        }

        #endregion

        #region General Function

        public bool OpenConnection(string connection)
        {
            try
            {
                if (dbconn == null || dbconn.State != ConnectionState.Open)
                {
                    dbconn = new SqlCeConnection(connection);
                    dbconn.Open();
                }
                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] OpenConnection: {0}", ex.ToString()), TraceCategory);
                return false;
            }
        }

        public bool CloseConnection()
        {
            try
            {
                if (dbconn != null && dbconn.State == ConnectionState.Open)
                    dbconn.Close();
                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] CloseConnection: {0}", ex.ToString()), TraceCategory);
                return false;
            }
        }

        private object ConstructSQLParam(object input)
        {
            if (input == null)
                return "NULL";
            else if (input.GetType() == typeof(string))
                return string.Format("'{0}'", input.ToString().Replace("'", "''"));
            else
                return input;
        }

        #endregion

        #region SQL Transation

        public void BeginTransaction()
        {
            dbtran = dbconn.BeginTransaction();
        }

        public void CommitTransaction()
        {
            if (dbtran != null)
            {
                dbtran.Commit(CommitMode.Immediate);
                dbtran = null;
            }
        }

        public void RollBackTransaction()
        {
            if (dbtran != null)
            {
                dbtran.Rollback();
                dbtran = null;
            }
        }

        #endregion

        #region __syncStatus

        public int CheckInitialized()
        {
            try
            {
                string query = @"
					SELECT S.[SettingName], S.[SettingValue]
					FROM [__syncStatus] S
					WHERE S.[SettingName] = 'IsInitialized'";

                dbcmd = new SqlCeCommand(query, dbconn);
                SqlCeDataAdapter da = new SqlCeDataAdapter(dbcmd);
                DataSet ds = new DataSet();
                da.Fill(ds);

                if (ds.Tables[0].Rows.Count == 0)
                    return -1;

                int initialized = Convert.ToInt32(ds.Tables[0].Rows[0]["SettingValue"]);
                return initialized;
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] CheckInitialized: {0}", ex.ToString()), TraceCategory);
                return -1;
            }
        }

        public int InitializeDatabase(Dictionary<string, string> settings)
        {
            int rowAffected = 0;

            try
            {
                string query = string.Empty;

                if (settings != null)
                {
                    foreach (var p in settings)
                    {
                        query = string.Format(@"
							UPDATE [__syncStatus]
							SET [SettingValue] = {1}
							WHERE [SettingName] = {0}",
                            ConstructSQLParam(p.Key),
                            ConstructSQLParam(p.Value));

                        dbcmd = new SqlCeCommand(query, dbconn);
                        if (dbtran != null)
                            dbcmd.Transaction = dbtran;

                        int rows = dbcmd.ExecuteNonQuery();
                        if (rows == 0)
                        {

                            query = string.Format(@"
								INSERT INTO [__syncStatus] ([SettingName], [SettingValue])
								VALUES ({0}, {1})",
                                ConstructSQLParam(p.Key),
                                ConstructSQLParam(p.Value));

                            dbcmd = new SqlCeCommand(query, dbconn);
                            if (dbtran != null)
                                dbcmd.Transaction = dbtran;

                            rowAffected += dbcmd.ExecuteNonQuery();
                        }
                        else rowAffected += rows;
                    }
                }

                query = @"
					UPDATE [__syncStatus]
					SET [SettingValue] = '1'
					WHERE [SettingName] = 'IsInitialized'";

                dbcmd = new SqlCeCommand(query, dbconn);
                if (dbtran != null)
                    dbcmd.Transaction = dbtran;

                rowAffected += dbcmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] InitializeDatabase: {0}", ex.ToString()), TraceCategory);
                rowAffected = 0;
            }

            return rowAffected;
        }

        #endregion

        #region acw_Component

        public Component GetComponent(string componentCode)
        {
            try
            {
                string query = @"
					SELECT C.[ComponentId], C.[ComponentType], C.[ComponentCode], C.[ComponentName], C.[ComponentConfig],
                        C.[Address1],
						C.[Address2],
						C.[Address3],
						C.[Address4],
						A.[AreaId] AS AreaId,
						A.[AreaCode] AS AreaCode,
						A.[AreaName] AS AreaName,
						A.[StateId] AS StateId,
						L.[ListText] AS StateName,
						EA1.[AttributeText] AS eWalletMerchantCode,
						EA2.[AttributeText] AS eWalletMerchantKey,
						EA3.[AttributeText] AS CompanyName,
						EA4.[AttributeText] AS Careline,
						EA5.[AttributeText] AS Email,
                        EA6.[AttributeText] AS SSMNo,
                        EA7.[AttributeText] AS SalesTaxNo
					FROM [acw_Component] C
					LEFT JOIN [acw_Area] A ON A.[AreaId] = C.[AreaId] AND A.[IsDeleted] = 0
					LEFT JOIN [List] L ON L.[ListId] = A.[StateId]
					LEFT JOIN [Entity] E ON E.[EntityId] = C.[CompanyId] AND E.[IsDeleted] = 0  
					
					LEFT JOIN [Attribute] A1 ON A1.[ProfileId] = E.[ProfileId] AND A1.[AttributeCategory] = 'Setting' AND A1.[AttributeName] = 'eWallet Merchant Code'
					LEFT JOIN [EntityAttribute] EA1 ON EA1.[EntityId] = E.[EntityId] AND EA1.[AttributeId] = A1.[AttributeId]
					LEFT JOIN [Attribute] A2 ON A2.[ProfileId] = E.[ProfileId] AND A2.[AttributeCategory] = 'Setting' AND A2.[AttributeName] = 'eWallet Merchant Key'
					LEFT JOIN [EntityAttribute] EA2 ON EA2.[EntityId] = E.[EntityId] AND EA2.[AttributeId] = A2.[AttributeId]

					LEFT JOIN [Attribute] A3 ON A3.[ProfileId] = E.[ProfileId] AND A3.[AttributeCategory] = '' AND A3.[AttributeName] = 'Name'
					LEFT JOIN [EntityAttribute] EA3 ON EA3.[EntityId] = E.[EntityId] AND EA3.[AttributeId] = A3.[AttributeId]

                    LEFT JOIN [Attribute] A4 ON A4.[ProfileId] = E.[ProfileId] AND A4.[AttributeCategory] = 'Contact Information' AND A4.[AttributeName] = 'Careline'
					LEFT JOIN [EntityAttribute] EA4 ON EA4.[EntityId] = E.[EntityId] AND EA4.[AttributeId] = A4.[AttributeId]
					LEFT JOIN [Attribute] A5 ON A5.[ProfileId] = E.[ProfileId] AND A5.[AttributeCategory] = 'Contact Information' AND A5.[AttributeName] = 'Email'
					LEFT JOIN [EntityAttribute] EA5 ON EA5.[EntityId] = E.[EntityId] AND EA5.[AttributeId] = A5.[AttributeId]

                    LEFT JOIN [Attribute] A6 ON A6.[ProfileId] = E.[ProfileId] AND A6.[AttributeCategory] = '' AND A6.[AttributeName] = 'SSM No'
					LEFT JOIN [EntityAttribute] EA6 ON EA6.[EntityId] = E.[EntityId] AND EA6.[AttributeId] = A6.[AttributeId]
                    LEFT JOIN [Attribute] A7 ON A7.[ProfileId] = E.[ProfileId] AND A7.[AttributeCategory] = '' AND A7.[AttributeName] = 'Sales Tax No'
					LEFT JOIN [EntityAttribute] EA7 ON EA7.[EntityId] = E.[EntityId] AND EA7.[AttributeId] = A7.[AttributeId]

					WHERE C.[ComponentCode] = {0} AND C.[IsDeleted] = 0";

                query = string.Format(query, ConstructSQLParam(componentCode));

                dbcmd = new SqlCeCommand(query, dbconn);
                SqlCeDataAdapter da = new SqlCeDataAdapter(dbcmd);
                DataSet ds = new DataSet();
                da.Fill(ds);

                if (ds.Tables[0].Rows.Count == 0)
                    return null;

                int componentId = Convert.ToInt32(ds.Tables[0].Rows[0]["ComponentId"]);
                string componentName = ds.Tables[0].Rows[0]["ComponentName"].ToString();
                string componentConfig = ds.Tables[0].Rows[0]["ComponentConfig"].ToString();
                int areaId = Convert.ToInt32(ds.Tables[0].Rows[0]["AreaId"]);
                string areaCode = ds.Tables[0].Rows[0]["AreaCode"].ToString();
                string areaName = ds.Tables[0].Rows[0]["areaName"].ToString();
                int stateId = Convert.ToInt32(ds.Tables[0].Rows[0]["StateId"]);
                string stateName = ds.Tables[0].Rows[0]["StateName"].ToString();

                string eWalletMerchantCode = ds.Tables[0].Rows[0]["eWalletMerchantCode"].ToString();
                string eWalletMerchantKey = ds.Tables[0].Rows[0]["eWalletMerchantKey"].ToString();

                string companyName = ds.Tables[0].Rows[0]["CompanyName"].ToString();
                string address1 = ds.Tables[0].Rows[0]["Address1"].ToString();
                string address2 = ds.Tables[0].Rows[0]["Address2"].ToString();
                string address3 = ds.Tables[0].Rows[0]["Address3"].ToString();
                string address4 = ds.Tables[0].Rows[0]["Address4"].ToString();
                string careline = ds.Tables[0].Rows[0]["Careline"].ToString();
                string email = ds.Tables[0].Rows[0]["Email"].ToString();

                string ssmNo = ds.Tables[0].Rows[0]["SSMNo"].ToString();
                string salesTaxNo = ds.Tables[0].Rows[0]["SalesTaxNo"].ToString();

                TimeSpan networkOpeningTime = TimeSpan.MinValue;
                TimeSpan networkClosingTime = TimeSpan.MinValue;

                string edcSettlementTime = null;
                string qdopenId = string.Empty;
                int qdiotId = 0;
                string qdsecret = string.Empty;

                string reg = @"(\w+)=([^\x7C]*)";
                MatchCollection matches = Regex.Matches(ds.Tables[0].Rows[0]["ComponentConfig"].ToString(), reg);
                foreach (Match m in matches)
                {
                    if (m.Groups[1].Value == "NOT")
                        networkOpeningTime = TimeSpan.Parse(m.Groups[2].Value);
                    else if (m.Groups[1].Value == "NCT")
                        networkClosingTime = TimeSpan.Parse(m.Groups[2].Value);
                    else if (m.Groups[1].Value == "EDC")
                        edcSettlementTime = m.Groups[2].Value;
                    else if (m.Groups[1].Value == "OPN")
                        qdopenId = m.Groups[2].Value;
                    else if (m.Groups[1].Value == "IOT")
                        qdiotId = Convert.ToInt32(m.Groups[2].Value);
                    else if (m.Groups[1].Value == "SCR")
                        qdsecret = m.Groups[2].Value;
                }

                Component c = new Component();
                c.Id = componentId;
                c.ComponentCode = componentCode;
                c.ComponentName = componentName;
                c.AreaId = areaId;
                c.AreaCode = areaCode;
                c.AreaName = areaName;
                c.StateId = stateId;
                c.StateName = stateName;
                c.NetworkOpeningTime = networkOpeningTime;
                c.NetworkClosingTime = networkClosingTime;
                c.EWalletMerchantCode = eWalletMerchantCode;
                c.EWalletMerchantKey = eWalletMerchantKey;
                c.QDOpenId = qdopenId;
                c.QDIotId = qdiotId;
                c.QDSecret = qdsecret;

                c.CompanyName = companyName;
                c.Address1 = address1;
                c.Address2 = address2;
                c.Address3 = address3;
                c.Address4 = address4;
                c.Email = email;
                c.Careline = careline;

                c.SSMNo = ssmNo;
                c.SalesTaxNo = salesTaxNo;

                c.EDCSettlementTime = new List<TimeSpan>();
                if (!string.IsNullOrEmpty(edcSettlementTime))
                {
                    try
                    {
                        foreach (string time in edcSettlementTime.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                            c.EDCSettlementTime.Add(TimeSpan.Parse(time));
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] GetComponent: {0}", ex.ToString()), TraceCategory);
                    }
                }

                return c;
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] GetComponent: {0}", ex.ToString()), TraceCategory);
                return null;
            }
        }

        #endregion

        #region acw_Area

        public Area GetArea(int AreaId, int RevisionId)
        {
            Area result = null;
            try
            {
                string query = @"
					SELECT H.[AreaId], H.[AreaCode], H.[AreaName], H.[StateId], H.[TaxId], H.[SurchargeId], 
                    H.[HolidayGroupId], C.[TariffId], C.[FareId], L.[ListText] AS StateName, 
                    T.[RateName] AS TaxName, S.[RateName] AS SurchargeName, G.[HolidayGroupName] AS HolidayGroupName
                    FROM [acw_Area] H
                    INNER JOIN [acw_AreaRevision] R ON R.[AreaId] = H.[AreaId] AND R.[RevisionId] = {1}
                    LEFT JOIN [List] L ON L.[ListId] = H.[StateId]
                    LEFT JOIN [Rate] T ON T.[RateId] = H.[TaxId]
                    LEFT JOIN [Rate] S ON S.[RateId] = H.[SurchargeId]
                    LEFT JOIN [acw_HolidayGroup] G ON G.[HolidayGroupId] = H.[HolidayGroupId]
                    LEFT JOIN [acw_AreaTariff] C ON C.[AreaId] = R.[AreaId] AND C.[RevisionId] = R.[RevisionId]
                    WHERE H.[AreaId] = {0} AND H.[IsDeleted] = 0";

                query = string.Format(query, ConstructSQLParam(AreaId), ConstructSQLParam(RevisionId));

                dbcmd = new SqlCeCommand(query, dbconn);
                SqlCeDataAdapter da = new SqlCeDataAdapter(dbcmd);
                DataSet ds = new DataSet();
                da.Fill(ds);

                if (ds.Tables[0].Rows.Count == 0)
                    return new Area();
                else 
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        if (result == null) 
                        {
                            result = new Area() 
                            {
                                AreaId = Convert.ToInt32(ds.Tables[0].Rows[i]["AreaId"]),
                                AreaCode = ds.Tables[0].Rows[i]["AreaCode"].ToString(),
                                AreaName = ds.Tables[0].Rows[i]["AreaName"].ToString(),
                                StateId = Convert.ToInt32(ds.Tables[0].Rows[i]["StateId"]),
                                StateName = ds.Tables[0].Rows[i]["StateName"].ToString(),
                                TaxId = Convert.ToInt32(ds.Tables[0].Rows[i]["TaxId"]),
                                TaxName = ds.Tables[0].Rows[i]["TaxName"].ToString(),
                                SurchargeId = Convert.ToInt32(ds.Tables[0].Rows[i]["SurchargeId"]),
                                SurchargeName = ds.Tables[0].Rows[i]["SurchargeName"].ToString(),
                                HolidayGroupId = Convert.ToInt32(ds.Tables[0].Rows[i]["HolidayGroupId"]),
                                HolidayGroupName = ds.Tables[0].Rows[i]["HolidayGroupName"].ToString(),
                            };
                            result.Tariffs = new List<AreaTariff>();
                        }

                        result.Tariffs.Add(new AreaTariff() 
                        {
                            TariffId = Convert.ToInt32(ds.Tables[0].Rows[i]["TariffId"]),
                            FareId = Convert.ToInt32(ds.Tables[0].Rows[i]["FareId"])
                        });
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] GetZone: {0}", ex.ToString()), TraceCategory);
                return null;
            }
        }

        public int GetActiveAreaRevision(int AreaId) 
        {
            int result = 0;

            try
            {
                string query = @"SELECT Max([RevisionId]) AS RevisionId FROM [acw_AreaRevision] WHERE [AreaId] = {0} AND [EffectiveDate] <= {1} ";
                query = string.Format(query, AreaId, ConstructSQLParam(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                SqlCeDataAdapter da = new SqlCeDataAdapter(query, dbconn);
                DataSet ds = new DataSet();
                da.Fill(ds);

                if (ds.Tables[0].Rows.Count == 0)
                    return 0;

                return Convert.ToInt32(ds.Tables[0].Rows[0]["RevisionId"]);
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] GetActiveAreaRevision: {0}", ex.ToString(), TraceCategory));
            }
            return result;
        }

        #endregion

        #region acw_Fare

        public Fare GetFare(int FareId, int RevisionId)
        {
            Fare result = null;
            try
            {
                string query = @"
					SELECT H.[FareId], H.[FareCode], H.[FareName], C.[ConfigId], C.[StartTime], C.[Value] AS Amount
                    FROM [acw_Fare] H
                    INNER JOIN [acw_FareRevision] R ON R.[FareId] = H.[FareId] 
                    AND R.[RevisionId] = {1}
                    LEFT JOIN [acw_FareConfig] C ON C.[FareId] = R.[FareId] AND C.[RevisionId] = R.[RevisionId]
                    WHERE H.[FareId] = {0} AND H.[IsDeleted] = 0";

                query = string.Format(query, ConstructSQLParam(FareId), ConstructSQLParam(RevisionId));

                dbcmd = new SqlCeCommand(query, dbconn);
                SqlCeDataAdapter da = new SqlCeDataAdapter(dbcmd);
                DataSet ds = new DataSet();
                da.Fill(ds);

                if (ds.Tables[0].Rows.Count == 0)
                    return null;
                else
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        if (result == null)
                        {
                            result = new Fare()
                            {
                                FareId = Convert.ToInt32(ds.Tables[0].Rows[i]["FareId"]),
                                FareCode = ds.Tables[0].Rows[i]["FareCode"].ToString(),
                                FareName = ds.Tables[0].Rows[i]["FareName"].ToString()
                            };
                            result.FareConfigs = new List<FareConfig>();
                        }

                        result.FareConfigs.Add(new FareConfig()
                        {
                            StartTime = Convert.ToDateTime(ds.Tables[0].Rows[i]["StartTime"]),
                            Amount = Convert.ToDecimal(ds.Tables[0].Rows[i]["Amount"])
                        });
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] GetFare: {0}", ex.ToString()), TraceCategory);
                return null;
            }
        }

        public int GetActiveFareRevision(int FareId)
        {
            int result = 0;

            try
            {
                string query = @"SELECT Max([RevisionId]) AS RevisionId FROM [acw_FareRevision] WHERE [FareId] = {0} AND [EffectiveDate] <= {1} ";
                query = string.Format(query, FareId, ConstructSQLParam(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));

                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("GetActiveFareRevision Query: {0}", query, TraceCategory));
                SqlCeDataAdapter da = new SqlCeDataAdapter(query, dbconn);
                DataSet ds = new DataSet();
                da.Fill(ds);

               if (ds.Tables[0].Rows.Count == 0)
                    return 0;

               return Convert.ToInt32(ds.Tables[0].Rows[0]["RevisionId"] == DBNull.Value ? "0" : ds.Tables[0].Rows[0]["RevisionId"]);
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] GetActiveFareRevision: {0}", ex.ToString(), TraceCategory));
            }
            return result;
        }

        #endregion

        #region Rate

        public Rate GetRate(int RateId, int RevisionId)
        {
            Rate result = null;
            try
            {
                string query = @"
					SELECT H.[RateId], H.[RateCode], H.[RateName], C.[Value]
                    FROM [Rate] H
                    INNER JOIN [RateRevision] R ON R.[RateId] = H.[RateId] 
                    AND R.[RevisionId] = {1}
                    LEFT JOIN [RateConfig] C ON C.[RateId] = R.[RateId] AND C.[RevisionId] = R.[RevisionId]
                    WHERE H.[RateId] = {0} AND H.[IsDeleted] = 0";

                query = string.Format(query, ConstructSQLParam(RateId), ConstructSQLParam(RevisionId));

                dbcmd = new SqlCeCommand(query, dbconn);
                SqlCeDataAdapter da = new SqlCeDataAdapter(dbcmd);
                DataSet ds = new DataSet();
                da.Fill(ds);

                if (ds.Tables[0].Rows.Count == 0)
                    return null;
                else
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        if (result == null)
                        {
                            result = new Rate()
                            {
                                RateId = Convert.ToInt32(ds.Tables[0].Rows[i]["RateId"]),
                                RateCode = ds.Tables[0].Rows[i]["RateCode"].ToString(),
                                RateName = ds.Tables[0].Rows[i]["RateName"].ToString(),
                                Value = Convert.ToInt32(ds.Tables[0].Rows[i]["Value"])
                            };
                            
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] GetRate: {0}", ex.ToString()), TraceCategory);
                return null;
            }
        }

        public int GetActiveRateRevision(int RateId)
        {
            int result = 0;

            try
            {
                string query = @"SELECT Max([RevisionId]) AS RevisionId FROM [RateRevision] WHERE [RateId] = {0} AND [EffectiveDate] <= {1} ";
                query = string.Format(query, RateId, ConstructSQLParam(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                SqlCeDataAdapter da = new SqlCeDataAdapter(query, dbconn);
                DataSet ds = new DataSet();
                da.Fill(ds);

                if (ds.Tables[0].Rows.Count == 0)
                    return 0;

                return Convert.ToInt32(ds.Tables[0].Rows[0]["RevisionId"]);
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] GetActiveRateRevision: {0}", ex.ToString(), TraceCategory));
            }
            return result;
        }

        #endregion

        #region acw_HolidayGroup

        public bool IsHoliday(int HolidayGroupId)
        {
            bool result = false;

            try
            {
                string query = @"SELECT S.* 
                                    FROM [acw_HolidaySet] S
                                    INNER JOIN [acw_HolidayGroup] G ON G.HolidayGroupId = S.HolidayGroupId 
                                    WHERE G.HolidayGroupId = {0} AND (({1} >= S.[DateFrom]
                                    AND {1} <= S.[DateTo]) OR 
                                    ({1} = S.[DateFrom]
                                    AND S.[DateTo] IS NULL))
                                    AND G.[IsDeleted] = 0";
                query = string.Format(query, HolidayGroupId, ConstructSQLParam(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                SqlCeDataAdapter da = new SqlCeDataAdapter(query, dbconn);
                DataSet ds = new DataSet();
                da.Fill(ds);

                if (ds.Tables[0].Rows.Count == 0)
                    return false;
                else
                    return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] IsHoliday: {0}", ex.ToString(), TraceCategory));
            }
            return result;
        }

        #endregion

        #region acw_TxAlarm

        public int acw_TxAlarm_Insert(int componentId, int txId, DateTime txDate, string txType, string referenceNo, int categoryId, string description, string status)
        {
            int rowAffected = 0;

            try
            {
                string query = @"
					INSERT INTO acw_TxAlarm ([ComponentId],[TxId],[TxDate],[TxType],[ReferenceNo],[CategoryId],[Description],[Status])
					VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7})";

                query = string.Format(query,
                    componentId,
                    txId,
                    ConstructSQLParam(txDate.ToString("yyyy-MM-dd HH:mm:ss.fff")),
                    ConstructSQLParam(txType),
                    ConstructSQLParam(referenceNo),
                    categoryId,
                    ConstructSQLParam(description),
                    ConstructSQLParam(status));

                dbcmd = new SqlCeCommand(query, dbconn);
                if (dbtran != null)
                    dbcmd.Transaction = dbtran;

                rowAffected = dbcmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] acw_TxAlarm_Insert: {0}", ex.ToString()), TraceCategory);
                rowAffected = 0;
            }

            return rowAffected;
        }

        #endregion

        #region acw_TxSales

        public int acw_TxSales_Insert(int componentId, int txId, DateTime txDate, string referenceNo, int areaId, int stateId, decimal netAmount, decimal taxAmount, decimal surchargeAmount, decimal totalAmount, decimal tenderAmount, string remarks, string status)
        {
            int rowAffected = 0;

            try
            {
                string query = string.Format(@"
					INSERT INTO [acw_TxSales] ([ComponentId], [TxId], [TxDate], [ReferenceNo], [AreaId], [StateId], [NetAmount], [TaxAmount], [SurchargeAmount], [TotalAmount], [TenderAmount], [Remarks], [Status], [PaymentTypeId], [PaymentNo], [PaymentData])
					VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15})",
                    componentId,														//0 
                    txId,																//1
                    ConstructSQLParam(txDate.ToString("yyyy-MM-dd HH:mm:ss.fff")),		//2
                    ConstructSQLParam(referenceNo),										//3
                    areaId,															    //4
                    stateId,															//5
                    netAmount,															//6
                    taxAmount,															//7
                    surchargeAmount,                                                    //8
                    totalAmount,														//9
                    tenderAmount,														//10
                    ConstructSQLParam(remarks),											//11
                    ConstructSQLParam(status),                                          //12       
                    0,															        //13
                    ConstructSQLParam(string.Empty),                                    //14
                    ConstructSQLParam(null));											//15

                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("acw_TxSales_Insert [Qeury] = {0}", query), TraceCategory);

                dbcmd = new SqlCeCommand(query, dbconn);
                if (dbtran != null)
                    dbcmd.Transaction = dbtran;

                rowAffected = dbcmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] acw_TxSales_Insert: {0}", ex.ToString()), TraceCategory);

                if (ex.Message.Contains("duplicate key"))
                { rowAffected = -99; }
                else { rowAffected = 0; }

            }

            return rowAffected;
        }

        public int acw_TxSales_Update(int componentId, int txId, string remarks, string status, int paymentTypeId, string paymentNo, string paymentData)
        {
            int rowAffected = 0;

            try
            {
                string query = string.Format(@"
					UPDATE [acw_TxSales] 
					SET 
                        [Remarks] = {2},
						[Status] = {3},
                        [PaymentTypeId] = {4},
                        [PaymentNo] = {5},
                        [PaymentData] = {6}
					WHERE [ComponentId] = {0} AND [TxId] = {1}",
                    componentId,														//0 
                    txId,																//1
                    ConstructSQLParam(remarks),                                         //2
                    ConstructSQLParam(status),                                          //3
                    paymentTypeId,                                                      //4
                    ConstructSQLParam(paymentNo),                                       //5
                    ConstructSQLParam(paymentData));								    //6

                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("acw_TxSales_Update [Qeury] = {0}", query), TraceCategory);
                dbcmd = new SqlCeCommand(query, dbconn);
                if (dbtran != null)
                    dbcmd.Transaction = dbtran;

                rowAffected = dbcmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] acw_TxSales_Update: {0}", ex.ToString()), TraceCategory);
                rowAffected = 0;
            }

            return rowAffected;
        }

        #endregion

        #region acw_TxEDCSettlement

        public int acw_TxEDCSettlement_Insert(int componentId, int txId, DateTime txDate, string referenceNo, string hostNo, string hostName, string batchNo, int batchCount, decimal batchAmount, string status, string remarks)
        {
            int rowAffected = 0;

            try
            {
                string query = @"
					INSERT INTO acw_TxEDCSettlement ([ComponentId],[TxId],[TxDate],[ReferenceNo],[HostNo],[HostName],[BatchNo],[BatchCount],[BatchAmount],[Status],[Remarks])
					VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9})";

                query = string.Format(query,
                    componentId, //0
                    txId,//1
                    ConstructSQLParam(txDate.ToString("yyyy-MM-dd HH:mm:ss.fff")), //2
                    ConstructSQLParam(referenceNo), //3
                    ConstructSQLParam(hostNo), //4
                    ConstructSQLParam(hostName), //5
                    ConstructSQLParam(batchNo), //6
                    ConstructSQLParam(batchCount), //7
                    ConstructSQLParam(batchAmount), //8
                    ConstructSQLParam(status), //9
                    ConstructSQLParam(remarks)); //10

                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("acw_TxEDCSettlement_Insert: {0}", query), TraceCategory);

                dbcmd = new SqlCeCommand(query, dbconn);
                if (dbtran != null)
                    dbcmd.Transaction = dbtran;

                rowAffected = dbcmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] acw_TxEDCSettlement_Insert: {0}", ex.ToString()), TraceCategory);
                rowAffected = 0;
            }

            return rowAffected;
        }

        #endregion

        #region LastTx

        public Dictionary<string, Transaction> GetLastTx()
        {
            try
            {
                Dictionary<string, Transaction> lastTx = new Dictionary<string, Transaction>();

                string query = string.Empty;

                #region TxSales

                query = @"
					SELECT TOP(1) [TxId], [ReferenceNo]
					FROM [acw_TxSales]
					WHERE [ComponentId] = {0}
					ORDER BY [TxId] DESC";

                query = string.Format(query, GeneralVar.CurrentComponent.Id);

                dbcmd = new SqlCeCommand(query, dbconn);
                SqlCeDataAdapter da = new SqlCeDataAdapter(dbcmd);
                DataSet ds = new DataSet();
                da.Fill(ds);

                if (ds.Tables[0].Rows.Count > 0)
                {
                    int id = Convert.ToInt32(ds.Tables[0].Rows[0]["TxId"]);
                    string referenceNo = ds.Tables[0].Rows[0]["ReferenceNo"].ToString();

                    lastTx.Add("acw_TxSales", new Transaction(id, referenceNo));
                }
                else
                {
                    query = @"
						SELECT [SettingName], [SettingValue]
						FROM __syncStatus
						WHERE SettingName = 'acw_TxSales'";

                    dbcmd = new SqlCeCommand(query, dbconn);
                    da = new SqlCeDataAdapter(dbcmd);
                    ds = new DataSet();
                    da.Fill(ds);

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        string value = ds.Tables[0].Rows[0]["SettingValue"].ToString();

                        if (string.IsNullOrEmpty(value))
                            lastTx.Add("acw_TxSales", new Transaction("S", GeneralVar.CurrentComponent.Id));
                        else
                        {
                            string[] s = value.Split('|');

                            int id = int.Parse(s[0]);
                            string referenceNo = s[1];

                            lastTx.Add("acw_TxSales", new Transaction(id, referenceNo));
                        }
                    }
                    else
                        lastTx.Add("acw_TxSales", new Transaction("S", GeneralVar.CurrentComponent.Id));
                }

                #endregion

                #region TxAlarm

                query = @"
					SELECT TOP(1) [TxId], [ReferenceNo]
					FROM [acw_TxAlarm]
					WHERE [ComponentId] = {0}
					ORDER BY [TxId] DESC";

                query = string.Format(query, GeneralVar.CurrentComponent.Id);

                dbcmd = new SqlCeCommand(query, dbconn);
                da = new SqlCeDataAdapter(dbcmd);
                ds = new DataSet();
                da.Fill(ds);

                if (ds.Tables[0].Rows.Count > 0)
                {
                    int id = Convert.ToInt32(ds.Tables[0].Rows[0]["TxId"]);
                    string referenceNo = ds.Tables[0].Rows[0]["ReferenceNo"].ToString();

                    lastTx.Add("acw_TxAlarm", new Transaction(id, referenceNo));
                }
                else
                {
                    query = @"
						SELECT [SettingName], [SettingValue]
						FROM __syncStatus
						WHERE SettingName = 'acw_TxAlarm'";

                    dbcmd = new SqlCeCommand(query, dbconn);
                    da = new SqlCeDataAdapter(dbcmd);
                    ds = new DataSet();
                    da.Fill(ds);

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        string value = ds.Tables[0].Rows[0]["SettingValue"].ToString();

                        if (string.IsNullOrEmpty(value))
                            lastTx.Add("acw_TxAlarm", new Transaction("A", GeneralVar.CurrentComponent.Id));
                        else
                        {
                            string[] s = value.Split('|');

                            int id = int.Parse(s[0]);
                            string referenceNo = s[1];

                            lastTx.Add("acw_TxAlarm", new Transaction(id, referenceNo));
                        }
                    }
                    else
                        lastTx.Add("acw_TxAlarm", new Transaction("A", GeneralVar.CurrentComponent.Id));
                }

                #endregion

                #region TxEDCSettlement

                query = @" 
					SELECT TOP(1) [TxId], [ReferenceNo]
					FROM [acw_TxEDCSettlement]
					WHERE [ComponentId] = {0}
					ORDER BY [TxId] DESC";

                query = string.Format(query, GeneralVar.CurrentComponent.Id);

                dbcmd = new SqlCeCommand(query, dbconn);
                da = new SqlCeDataAdapter(dbcmd);
                ds = new DataSet();
                da.Fill(ds);

                if (ds.Tables[0].Rows.Count > 0)
                {
                    int id = Convert.ToInt32(ds.Tables[0].Rows[0]["TxId"]);
                    string referenceNo = ds.Tables[0].Rows[0]["ReferenceNo"].ToString();

                    lastTx.Add("acw_TxEDCSettlement", new Transaction(id, referenceNo));
                }
                else
                {
                    query = @"
						SELECT [SettingName], [SettingValue]
						FROM __syncStatus
						WHERE SettingName = 'acw_TxEDCSettlement'";

                    dbcmd = new SqlCeCommand(query, dbconn);
                    da = new SqlCeDataAdapter(dbcmd);
                    ds = new DataSet();
                    da.Fill(ds);

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        string value = ds.Tables[0].Rows[0]["SettingValue"].ToString();

                        if (string.IsNullOrEmpty(value))
                            lastTx.Add("acw_TxEDCSettlement", new Transaction("H", GeneralVar.CurrentComponent.Id));
                        else
                        {
                            string[] s = value.Split('|');

                            int id = int.Parse(s[0]);
                            string referenceNo = s[1];

                            lastTx.Add("acw_TxEDCSettlement", new Transaction(id, referenceNo));
                        }
                    }
                    else
                        lastTx.Add("acw_TxEDCSettlement", new Transaction("H", GeneralVar.CurrentComponent.Id));
                }

                #endregion

                return lastTx;
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] GetLastTx: {0}", ex.ToString()), TraceCategory);
                return null;
            }
        }

        #endregion

        #region Payment Type

        public List<PaymentType> GetPaymentType()
        {
            List<PaymentType> paymentTypes = new List<PaymentType>();
            try
            {
                string query = @"SELECT [ListId], [ListValue], [ListText], [ListConfig] FROM [List] WHERE [ListName] = {0} AND IsDeleted = 0";
                query = string.Format(query, ConstructSQLParam("PaymentType"));

                SqlCeDataAdapter da = new SqlCeDataAdapter(query, dbconn);
                DataSet ds = new DataSet();
                da.Fill(ds);

                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    PaymentType pt = new PaymentType();
                    pt.PaymentTypeId = Convert.ToInt32(ds.Tables[0].Rows[i]["ListId"]);
                    pt.PaymentTypeCode = ds.Tables[0].Rows[i]["ListValue"].ToString();
                    pt.PaymentTypeName = ds.Tables[0].Rows[i]["ListText"].ToString();
                    
                    paymentTypes.Add(pt);
                }

                return paymentTypes;
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] GetPaymentType: {0}", ex.ToString()), TraceCategory);
                return null;
            }
        }

        #endregion

        #region Alarm Category

        public List<AlarmCategory> GetAlarmCategory()
        {
            List<AlarmCategory> alarmCategories = new List<AlarmCategory>();
            try
            {
                string query = @"SELECT [ListId], [ListValue], [ListText] FROM [List] WHERE [ListName] = {0} AND IsDeleted = 0";
                query = string.Format(query, ConstructSQLParam("AlarmCategory"));

                SqlCeDataAdapter da = new SqlCeDataAdapter(query, dbconn);
                DataSet ds = new DataSet();
                da.Fill(ds);

                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    AlarmCategory ac = new AlarmCategory();
                    ac.AlarmCategoryId = Convert.ToInt32(ds.Tables[0].Rows[i]["ListId"]);
                    ac.AlarmCategoryCode = ds.Tables[0].Rows[i]["ListValue"].ToString();
                    ac.AlarmCategoryName = ds.Tables[0].Rows[i]["ListText"].ToString();

                    alarmCategories.Add(ac);
                }

                return alarmCategories;
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] GetAlarmCategory: {0}", ex.ToString()), TraceCategory);
                return null;
            }
        }

        #endregion

        #region Setting

        public string GetSetting(string settingName)
        {
            string settingValue = null;

            try
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceVerbose, "GetSetting Started...", TraceCategory);

                string query = @"SELECT [SettingName], [SettingValue] FROM [Setting] WHERE [ApplicationCode] = {0} AND [SettingName] = {1}";
                query = string.Format(query, ConstructSQLParam(GeneralVar.ApplicationCode), ConstructSQLParam(settingName));
                SqlCeDataAdapter da = new SqlCeDataAdapter(query, dbconn);
                DataSet ds = new DataSet();
                da.Fill(ds);

                if (ds.Tables[0].Rows.Count == 0)
                    return null;

                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceVerbose, "GetSetting Completed", TraceCategory);

                return ds.Tables[0].Rows[0]["SettingValue"].ToString();
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] GetSetting: {0}", ex.ToString(), TraceCategory));
            }
            return settingValue;
        }

        public int SetSetting(string settingName, string settingValue)
        {
            int rowAffected = 0;

            try
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceVerbose, "GetSetting Started...", TraceCategory);

                string query = @"UPDATE [Setting] SET [SettingValue] = {2} WHERE [ApplicationCode] = {0} AND [SettingName] = {1}";
                query = string.Format(query, ConstructSQLParam(GeneralVar.ApplicationCode), ConstructSQLParam(settingName), ConstructSQLParam(settingValue));

                dbcmd = new SqlCeCommand(query, dbconn);
                if (dbtran != null)
                    dbcmd.Transaction = dbtran;

                rowAffected = dbcmd.ExecuteNonQuery();

                if (rowAffected > 0)
                    return rowAffected;

                query = @"INSERT INTO [Setting] ([ApplicationCode], [SettingName], [SettingValue], [IsSecure]) VALUES ({0}, {1}, {2}, 0)";
                query = string.Format(query, ConstructSQLParam(GeneralVar.ApplicationCode), ConstructSQLParam(settingName), ConstructSQLParam(settingValue));

                dbcmd = new SqlCeCommand(query, dbconn);
                if (dbtran != null)
                    dbcmd.Transaction = dbtran;

                rowAffected = dbcmd.ExecuteNonQuery();

                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceVerbose, "GetSetting Completed", TraceCategory);
                return rowAffected;
            }
            catch (Exception ex)
            {
                rowAffected = 0;
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] GetSetting: {0}", ex.ToString(), TraceCategory));
            }

            return rowAffected;
        }

        #endregion
    }
}
