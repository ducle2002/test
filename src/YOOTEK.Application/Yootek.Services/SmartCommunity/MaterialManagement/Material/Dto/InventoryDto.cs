using Abp.AutoMapper;
using Yootek.EntityDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Services
{
    [AutoMap(typeof(Inventory))]
    public class InventoryDto: Inventory
    {
    }


    public class InventoryDictionaryDto
    {
        public long Id {  get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int CountMaterial { get; set; }
        public int CountAmount { get; set; }
        public List<MaterialDto> Materials { get; set; }
    }
}
