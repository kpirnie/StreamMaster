﻿using StreamMaster.Domain.API;
using StreamMaster.Domain.Repository;

namespace StreamMaster.Domain.Services
{
    public interface ISMChannelStreamLinksRepository : IRepositoryBase<SMChannelStreamLink>
    {
        void CreateSMChannelStreamLink(SMChannel smChannel, string smStreamId, int? Rank);
        void CreateSMChannelStreamLink(SMChannel smChannel, SMStream smStream, int? Rank);
        new IQueryable<SMChannelStreamLink> GetQuery(bool tracking = false);
        //Task CreateSMChannelStreamLink(int SMChannelId, SMStream SMStream, int? CurrentRank);
        Task CreateSMChannelStreamLink(int SMChannelId, string SMStreamId, int? Rank);
        Task DeleteSMChannelStreamLink(SMChannelStreamLink sMChannelStreamLink);
        Task DeleteSMChannelStreamLinks(IQueryable<SMChannelStreamLink> linksToDelete);
        Task DeleteSMChannelStreamLinksFromParentId(int smchannelId);
        List<SMChannelStreamLink> GetSMChannelStreamLinks();
        Task<APIResponse> SetSMStreamRank(List<SMChannelStreamRankRequest> request);
        Task UpdateSMChannelDtoRanks(SMChannelDto smChannel);
    }
}