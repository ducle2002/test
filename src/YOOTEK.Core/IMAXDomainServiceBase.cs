using Abp.Dependency;
using Abp.Domain.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Yootek
{
    public abstract class YootekDomainServiceBase : DomainService
    {
        /* Add your common members for all your domain services. */

        protected YootekDomainServiceBase()
        {
            LocalizationSourceName = YootekConsts.LocalizationSourceName;
        }


        protected string RemoveUnicode(string text)
        {
            string[] arr1 = new string[] { "á", "à", "ả", "ã", "ạ", "â", "ấ", "ầ", "ẩ", "ẫ", "ậ", "ă", "ắ", "ằ", "ẳ", "ẵ", "ặ",
            "đ",
            "é","è","ẻ","ẽ","ẹ","ê","ế","ề","ể","ễ","ệ",
            "í","ì","ỉ","ĩ","ị",
            "ó","ò","ỏ","õ","ọ","ô","ố","ồ","ổ","ỗ","ộ","ơ","ớ","ờ","ở","ỡ","ợ",
            "ú","ù","ủ","ũ","ụ","ư","ứ","ừ","ử","ữ","ự",
            "ý","ỳ","ỷ","ỹ","ỵ",};
            string[] arr2 = new string[] { "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a",
            "d",
            "e","e","e","e","e","e","e","e","e","e","e",
            "i","i","i","i","i",
            "o","o","o","o","o","o","o","o","o","o","o","o","o","o","o","o","o",
            "u","u","u","u","u","u","u","u","u","u","u",
            "y","y","y","y","y",};

            text = text.ToLower();
            for (int i = 0; i < arr1.Length; i++)
            {
                text = text.Replace(arr1[i], arr2[i]);
            }
            text = text.Replace(" ", "");
            return text;
        }

        protected string FileToBase64String(string pathFile)
        {
            try
            {
                string Yootek_FILE = GetUrlFileDefaut();
                var folderPath = Path.Combine(Yootek_FILE, pathFile);

                byte[] bytes = File.ReadAllBytes(folderPath);
                string pdfBase64 = Convert.ToBase64String(bytes);

                return pdfBase64;
            }
            catch
            {
                return null;
            }
        }

        protected string GetUrlFileDefaut()
        {
            string Yootek_FILE = ConfigurationManager.AppSettings["Yootek_FILE"];
            if (string.IsNullOrEmpty(Yootek_FILE))
            {
                Yootek_FILE = @"C:\Yootek_FILE";
            }

            if (!Directory.Exists(Yootek_FILE))
            {
                Directory.CreateDirectory(Yootek_FILE);
            }
            return Yootek_FILE;
        }

        protected string GetUniqueKey()
        {
            int maxSize = 10;
            char[] chars = new char[36];
            string a;
            a = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            chars = a.ToCharArray();
            int size = maxSize;
            byte[] data = new byte[1];
            RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
            crypto.GetNonZeroBytes(data);
            size = maxSize;
            data = new byte[size];
            crypto.GetNonZeroBytes(data);
            StringBuilder result = new StringBuilder(size);
            foreach (byte b in data)
            { result.Append(chars[b % (chars.Length - 1)]); }
            return result.ToString();
        }
    }
}
