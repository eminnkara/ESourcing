using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ESourcing.Products.Settings
{
    public interface IProductDatabaseSettings
    {
        string ConnectionStrings { get; set; }
        string DatabaseName { get; set; }
        string CollectionName { get; set; }
    }
}
