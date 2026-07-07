//Enter the required .NET namespace
using System;
using System.Collections.Generic;
using System.Text;

//Define namespace for this interface
namespace ECommerceAuction.UserService.Domain.Common
{
    /// <summary>
    ///IAggregateRoot interface - marks entities as aggregate root
    ///Aggregate root is the root entity in an aggregate (group of related entities)
    ///In DDD (Domain-Driven Design), each aggregate has only one aggregate root
    ///This interface is used to identify entities as aggregate root
    /// </summary>
    public interface IAggregateRoot
    {
        //Empty interface - used only for identification (marker interface)
    }
}
