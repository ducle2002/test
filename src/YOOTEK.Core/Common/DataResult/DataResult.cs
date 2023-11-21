﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMAX.Common.DataResult
{

    public interface IDataResult
    {
        string Message { get; set; }
        string Error { get; set; }
        bool Success { get; set; }
        object Data { get; set; }
    }


    public class DataResult : IDataResult
    {
        public string Message { get; set; }
        public string Error { get; set; }
        public bool Success { get; set; } = true;
        public object Data { get; set; }
        public int? TotalRecords { get; set; }
        public int? Result_Code { get; set; }

        public static DataResult ResultSuccess(object data, string message) => new DataResult()
        {
            Data = data,
            Message = message,
            Success = true

        };


        public static DataResult ResultSuccess(object data, string message, int totalReCords) => new DataResult()
        {
            Data = data,
            Message = message,
            Success = true,
            TotalRecords = totalReCords

        };

        public static DataResult ResultError(string err, string message) => new DataResult()
        {
            Data = null,
            Error = err,
            Message = message,
            Success = false
        };

        public static DataResult ResultSuccess(string message) => new DataResult()
        {
            Message = message,
            Success = true
        };

        public static DataResult ResultFail(string message) => new DataResult()
        {
            Message = message,
            Success = false
        };

        public static DataResult ResultCode(object data, string message, int result_code) => new DataResult()
        {
            Data = data,
            Message = message,
            Success = (result_code == 200),
            Result_Code = result_code

        };

    }


    public class DataRattingResult : IDataResult
    {
        public string Message { get; set; }
        public string Error { get; set; }
        public bool Success { get; set; } = true;
        public object Data { get; set; }
        public int? TotalRecords { get; set; }

        public RateDetail Details { get; set; }

        public static DataRattingResult ResultSuccess(object data, string message, int totalReCords, RateDetail rateDetails) => new DataRattingResult()
        {
            Data = data,
            Message = message,
            Success = true,
            TotalRecords = totalReCords,
            Details = rateDetails

        };

    }

    public class RateDetail
    {
        public int TotalRate5 { get; set; }
        public int TotalRate4 { get; set; }
        public int TotalRate3 { get; set; }
        public int TotalRate2 { get; set; }
        public int TotalRate1 { get; set; }
        public int TotalImage { get; set; }
        public int TotalComment { get; set; }
    }
}
