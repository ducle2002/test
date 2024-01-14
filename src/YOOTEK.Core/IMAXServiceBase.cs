using Abp;
using System;

namespace Yootek
{
    public abstract class YootekServiceBase : AbpServiceBase
    {
        protected YootekServiceBase()
        {
            LocalizationSourceName = YootekConsts.LocalizationSourceName;
        }
        public class FieldNameAttribute : Attribute
        {
            public string FieldNameString;

            public FieldNameAttribute(string text)
            {
                this.FieldNameString = text;
            }
        }
    }
}
