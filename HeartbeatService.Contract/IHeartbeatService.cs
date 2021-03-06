﻿using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HeartbeatService.Contract
{
    public interface IHeartbeatService : IService
    {
        Task SubmitHeartbeatAsync(Heartbeat heartbeat);
        Task<List<Heartbeat>> GetAllHeartbeatsAsync(CancellationToken cancellationToken);
    }
}
