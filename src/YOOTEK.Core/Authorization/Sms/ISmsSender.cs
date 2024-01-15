using System.Threading.Tasks;

namespace Yootek.Net.Sms
{
    public interface ISmsSender
    {
        Task SendAsync(string number, string message);
    }
}