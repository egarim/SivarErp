using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sivar.Erp.Core.Contracts
{
    public interface IBusinessEntity
    {
  
        string Code { get; set; }
        string Name { get; set; }
        string Address { get; set; }
        string City { get; set; }
        string State { get; set; }
        string ZipCode { get; set; }
        string Country { get; set; }
        string PhoneNumber { get; set; }
        string Email { get; set; }
    }
}