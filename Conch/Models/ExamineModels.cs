using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Conch.Models
{
    public class ExamineItem
    {
        /// <summary>
        /// 审批编号(S+业务流水号+XX（两位数字，自增）
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 业务流水号
        /// </summary>
        public string SerialNo { get; set; }
        /// <summary>
        /// 提货单号
        /// </summary>
        public string BillOfLading { get; set; }
        public string Customer { get; set; }
        /// <summary>
        /// 破损袋数
        /// </summary>
        public int DamagedCount { get; set; }
        /// <summary>
        /// 破损规格
        /// </summary>
        public string DamagedSpec { get; set; }
        /// <summary>
        /// 车牌号
        /// </summary>
        public string VehicleId { get; set; }
        /// <summary>
        /// 提交人员工号
        /// </summary>
        public string Submitter { get; set; }
        /// <summary>
        ///  审核人员工号
        /// </summary>
        public string Processor { get; set; }
        /// <summary>
        /// 提交时间
        /// </summary>
        public DateTime SubmittingTime { get; set; }

        public enum ExamineItemStatus
        {
            Pending, Approve, Reject
        }
    }

    public class ExamineAction
    {
        /// <summary>
        /// 审批编号(S+业务流水号+XX（两位数字，自增）
        /// </summary>
        public string Id { get; set; }
        public string Processor { get; set; }
        /// <summary>
        /// 审核结果  0 待审批 1审批通过，同意补包 2审批不通过，不同意补包
        /// </summary>
        public int Status { get; set; }
    }
    public class ExamineResult
    {
        /// <summary>
        /// 审批编号(S+业务流水号+XX（两位数字，自增）
        /// </summary>
        public string Id { get; set; }
        public string Processor { get; set; }
        /// <summary>
        /// 审核结果  0 待审批 1审批通过，同意补包 2审批不通过，不同意补包
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 审核时间
        /// </summary>
        public string ProcessTime { get; set; }
    }
    public class ExamineItemRepository
    {
        /// <summary>
        /// 返回单条待审批记录
        /// </summary>
        /// <returns></returns>
        public ResultType Item(string id, out ExamineItem item)
        {
            var conn = new SqlConnection(WebApiConfig.ConnectionString);
            var sql = "SELECT f_id, f_rule, f_getid, f_customer, f_torncount, f_tornspec, f_carno, f_userid, f_refertime FROM [dbo].[T_TornApprove] WHERE f_examflag = 0 AND f_id = @id";
            var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@id", id));
            var ret = ResultType.NotFound;

            item = null;
            try
            {
                conn.Open();
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    item = new ExamineItem
                    {
                        Id = reader.GetString(0),
                        SerialNo = reader.GetString(1),
                        BillOfLading = reader.GetString(2),
                        Customer = reader.GetString(3),
                        DamagedCount = reader.GetInt32(4),
                        DamagedSpec = reader.GetString(5),
                        VehicleId = reader.GetString(6),
                        Submitter = reader.GetString(7),
                        SubmittingTime = reader.GetDateTime(8)
                    };
                    ret = ResultType.Success;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                ret = ResultType.DatabaseError;
            }
            finally
            {
                conn.Close();
            }
            return ret;
        }

        /// <summary>
        /// 返回所有待审批记录
        /// </summary>
        /// <returns></returns>
        public ResultType All(out List<ExamineItem> list)
        {
            var conn = new SqlConnection(WebApiConfig.ConnectionString);
            var sql = "SELECT f_id, f_rule, f_getid, f_customer, f_torncount, f_tornspec, f_carno, f_userid, f_refertime FROM [dbo].[T_TornApprove] WHERE f_examflag = 0";
            var cmd = new SqlCommand(sql, conn);
            var dataSet = new DataSet();
            var ret = ResultType.Success;

            list = null;
            try
            {
                conn.Open();
                var adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dataSet);
                conn.Close();

                list = dataSet.Tables[0].AsEnumerable().Select(dataRow => new ExamineItem
                {
                    Id = dataRow.Field<string>("f_id"),
                    SerialNo = dataRow.Field<string>("f_rule"),
                    BillOfLading = dataRow.Field<string>("f_getid"),
                    Customer = dataRow.Field<string>("f_customer"),
                    DamagedCount = dataRow.Field<int>("f_torncount"),
                    DamagedSpec = dataRow.Field<string>("f_tornspec"),
                    VehicleId = dataRow.Field<string>("f_carno"),
                    Submitter = dataRow.Field<string>("f_userid"),
                    SubmittingTime = dataRow.Field<DateTime>("f_refertime")
                }).ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                ret = ResultType.DatabaseError;
            }
            finally
            {
                conn.Close();
            }

            return ret;
        }

        public ResultType Process(ExamineAction action, out ExamineResult result)
        {
            var conn = new SqlConnection(WebApiConfig.ConnectionString);
            var sql = "[dbo].[Pr_AppExamine]";
            var cmd = new SqlCommand(sql, conn);
            cmd.CommandType = CommandType.StoredProcedure;
            var retVal = cmd.Parameters.Add("ret_val", SqlDbType.Int);
            retVal.Direction = ParameterDirection.ReturnValue;
            cmd.Parameters.Add(new SqlParameter("@rec_id", action.Id));
            cmd.Parameters.Add(new SqlParameter("@status", action.Status));
            cmd.Parameters.Add(new SqlParameter("@processor", action.Processor));
            ResultType ret = ResultType.Success;

            result = null;
            try
            {
                conn.Open();
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    result = new ExamineResult
                    {
                        Id = reader.GetString(0),
                        Status = reader.GetInt32(1),
                        ProcessTime = reader.GetDateTime(2).ToString("yyyy-MM-dd'T'HH:mm:ss"),
                        Processor = reader.GetString(3)
                    };
                }
                switch (retVal.Value)
                {
                    case -1:    // 操作状态非法
                        ret = ResultType.BadParameter;
                        break;
                    case -2:    // 用户权限不足
                        ret = ResultType.Unauthorized;
                        break;
                    case -3:    // 记录号不存在
                        ret = ResultType.BadParameter;
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                ret = ResultType.DatabaseError;
            }
            finally
            {
                conn.Close();
            }
            return ret;
        }
    }
}