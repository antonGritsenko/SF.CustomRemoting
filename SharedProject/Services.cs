using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;

namespace SharedProject
{
    public interface IService1 : IService
    {
        Task ServiceOneMethod();

    }

    public interface IService2 : IService
    {
        Task ServiceTwoMethod();

    }
}
