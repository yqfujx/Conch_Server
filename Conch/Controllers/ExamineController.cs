using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Conch.Models;
using Conch.Common;
using System.Data;
using Newtonsoft.Json;

namespace Conch.Controllers
{
    public class ExamineController : ApiController
    {
        private ExamineItemRepository ExaminesRepository { get; set; }
        public ExamineController()
        {
            ExaminesRepository = new ExamineItemRepository();
        }

        ~ExamineController()
        {
            ExaminesRepository = null;
        }

        /// <summary>
        /// 接口方法，返回所有待审批记录列表
        /// </summary>
        /// <returns></returns>
        /// GET: /api/Examin
        /// 
        public ResultData Get()
        {
            List<ExamineItem> list;
            var result = ExaminesRepository.All(out list);

            if (result == ResultType.Success)
            {
                return new ResultData
                {
                    Result = (int)ResultType.Success,
                    Data = list
                };
            }
            else
            {
                return new ResultData
                {
                    Result = (int)result,
                    Desc = EnumExtension.GetEnumDescription(result)
                };
            }
        }

        /// <summary>
        /// 接口方法，返回指定ID号的单条审批记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// GET: /api/Examine/{id}
        /// 
        //public ResultData Get(string token, string id)
        //{
        //    ExamineItem item = null;
        //    var result = ExaminesRepository.Item(id, out item);

        //    if (result == ResultType.Success)
        //    {
        //        return new ResultData
        //        {
        //            Result = (int)ResultType.Success,
        //            Data = item
        //        };
        //    }
        //    else
        //    {
        //        return new ResultData
        //        {
        //            Result = (int)result,
        //            Desc = EnumExtension.GetEnumDescription(result)
        //        };
        //    }
        //}

        /// <summary>
        /// 接口方法，对指定ID的记录进行审批操作
        /// </summary>
        /// <param name="id"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        /// POST: /api/Examine
        public ResultData Post(ExamineAction action)
        {
            ExamineResult examineResult;
            var ret = ExaminesRepository.Process(action, out examineResult);
            ResultData result = new ResultData
            {
                Result = Convert.ToInt32(ret),
                Desc = EnumExtension.GetEnumDescription(ret)
            };

            if (ret == ResultType.Success)
            {
                result.Data = examineResult;
            }

            return result;
        }
    }
}
