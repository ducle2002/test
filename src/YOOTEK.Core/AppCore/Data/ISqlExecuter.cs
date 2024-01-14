using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.ApbCore.Data
{
    public interface ISqlExecuter
    {
        /// <summary>
        /// Thực thi câu truy vấn mà không cần trả về dữ liệu. Thường là các câu INSERT, UPDATE, DELETE
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        int Execute(string query, params object[] parameters);
        Task<int> ExecuteAsync(string query);

    }
}
