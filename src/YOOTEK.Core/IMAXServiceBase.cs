using Abp;
using System;

namespace IMAX
{
    public abstract class IMAXServiceBase : AbpServiceBase
    {
        protected IMAXServiceBase()
        {
            LocalizationSourceName = IMAXConsts.LocalizationSourceName;
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
