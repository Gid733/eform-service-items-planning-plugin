﻿/*
The MIT License (MIT)

Copyright (c) 2007 - 2020 Microting A/S

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

namespace ServiceItemsPlanningPlugin.Handlers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Infrastructure.Helpers;
    using Messages;
    using Microsoft.EntityFrameworkCore;
    using Microting.ItemsPlanningBase.Infrastructure.Data;
    using Rebus.Bus;
    using Rebus.Handlers;
    using Constants = Microting.eForm.Infrastructure.Constants.Constants;

    public class ScheduledItemExecutedHandler : IHandleMessages<ScheduledItemExecuted>
    {
        private readonly ItemsPlanningPnDbContext _dbContext;
        private readonly eFormCore.Core _sdkCore;
        private readonly IBus _bus;

        public ScheduledItemExecutedHandler(eFormCore.Core sdkCore, DbContextHelper dbContextHelper, IBus bus)
        {
            _sdkCore = sdkCore;
            _dbContext = dbContextHelper.GetDbContext();
            _bus = bus;
        }

        #pragma warning disable 1998
        public async Task Handle(ScheduledItemExecuted message)
        {
            var planning = await _dbContext.Plannings
                .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
                .SingleOrDefaultAsync(x => x.Id == message.PlanningId);

            var siteIds = planning.PlanningSites
                .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
                .Select(x => x.SiteId)
                .ToList();

            if (!siteIds.Any())
            {
                Console.WriteLine("SiteIds not set");
                return;
            }

            Console.WriteLine($"SiteIds {siteIds}");

            await _bus.SendLocal(new ItemCaseCreate(planning.Id, planning.Item.Id, planning.RelatedEFormId, planning.Name));
        }
    }
}
