using System.Threading.Tasks;

namespace IMAX.Net.Sms
{
    public interface ISmsSender
    {
        Task SendAsync(string number, string message);
    }
}